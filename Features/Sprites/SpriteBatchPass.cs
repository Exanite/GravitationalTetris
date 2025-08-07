using System;
using System.Collections.Generic;
using Exanite.Core.Pooling;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Engine.Graphics;
using Exanite.ResourceManagement;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Sprites;

public class SpriteBatchPass : ITrackedDisposable
{
    public bool IsDisposed { get; private set; }

    private const int MaxSpritesPerBatch = 1024;
    private const int MaxTexturesPerBatch = 64;

    private readonly ShaderPipelineCache<PipelineCacheKey, PipelineCacheState> pipelines;

    private readonly CycledBuffer<SpriteUniformData> uniformBuffers;
    private readonly CycledBuffer<SpriteInstanceData> instanceBuffers;

    private readonly DisposableCollection disposables = new();

    public SpriteBatchPass(GraphicsContext graphicsContext, ResourceManager resourceManager)
    {
        uniformBuffers = new CycledBuffer<SpriteUniformData>(graphicsContext, new BufferDesc()
        {
            Usages = BufferUsageFlags.UniformBufferBit,
            MapType = AllocationMapType.SequentialWrite,
        }, 1).AddTo(disposables);

        instanceBuffers = new CycledBuffer<SpriteInstanceData>(graphicsContext, new BufferDesc()
        {
            Usages = BufferUsageFlags.VertexBufferBit,
            MapType = AllocationMapType.SequentialWrite,
        }, MaxSpritesPerBatch).AddTo(disposables);

        var vertexModule = resourceManager.GetResource(BaseMod.SpriteVertexModule);
        var fragmentModule = resourceManager.GetResource(BaseMod.SpriteFragmentModule);

        var sampler = new TextureSampler(graphicsContext, new TextureSamplerDesc(Filter.Nearest)).AddTo(disposables);

        pipelines = new ShaderPipelineCache<PipelineCacheKey, PipelineCacheState>(key =>
        {
            var pipeline = new ShaderPipeline(graphicsContext, new ShaderPipelineDesc()
            {
                ShaderModules = [vertexModule.Value, fragmentModule.Value],

                Topology = PrimitiveTopology.TriangleStrip,
                InstanceLayout = SpriteInstanceData.Layout,

                ColorAttachmentFormats = [key.ColorFormat],
                ColorAttachmentBlends =
                [
                    new ShaderPipelineColorAttachmentBlendDesc()
                    {
                        ColorBlendOp = BlendOp.Add,
                        SrcColorBlendFactor = BlendFactor.SrcAlpha,
                        DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,

                        AlphaBlendOp = BlendOp.Add,
                        SrcAlphaBlendFactor = BlendFactor.Zero,
                        DstAlphaBlendFactor = BlendFactor.One,
                    },
                ],

                DepthAttachmentFormat = key.DepthFormat,
            });

            pipeline.Layout.GetVariable("TextureSampler").SetSampler(sampler);

            return new PipelineCacheState(pipeline);
        }).AddTo(disposables);
    }

    public void Render(GraphicsCommandBuffer commandBuffer, Texture2D colorTarget, Texture2D depthTarget, SpriteBatch batch)
    {
        var pipeline = pipelines.GetPipeline(new PipelineCacheKey(colorTarget.Desc.Format, depthTarget.Desc.Format));

        commandBuffer.AddBarrier(new Barrier()
        {
            SrcStages = PipelineStageFlags2.ColorAttachmentOutputBit,
            SrcAccesses = AccessFlags2.ColorAttachmentWriteBit,

            DstStages = PipelineStageFlags2.ColorAttachmentOutputBit,
            DstAccesses = AccessFlags2.ColorAttachmentWriteBit,
        });

        commandBuffer.AddBarrier(new Barrier()
        {
            SrcStages = PipelineStageFlags2.LateFragmentTestsBit,
            SrcAccesses = AccessFlags2.DepthStencilAttachmentWriteBit,

            DstStages = PipelineStageFlags2.EarlyFragmentTestsBit,
            DstAccesses = AccessFlags2.DepthStencilAttachmentReadBit | AccessFlags2.DepthStencilAttachmentWriteBit,
        });

        using (commandBuffer.BeginRenderPass(new RenderPassDesc([colorTarget], depthTarget)))
        {
            using var _ = ListPool<Texture2D>.Acquire(out var textures);
            using var __ = ListPool<SpriteInstanceData>.Acquire(out var sprites);

            foreach (var sprite in batch.Sprites)
            {
                // Sprites use bindless textures
                // Check if the textures list already contains the sprite's texture, if so, we use it
                uint? textureIndex = default;
                for (var i = 0; i < textures.Count; i++)
                {
                    if (textures[i] == sprite.Texture)
                    {
                        textureIndex = (uint)i;

                        break;
                    }
                }

                // Otherwise, we try to add the sprite's texture to the textures list
                if (!textureIndex.HasValue)
                {
                    if (textures.Count >= MaxTexturesPerBatch)
                    {
                        // Too many textures in this batch, split into new batch
                        Submit(commandBuffer, pipeline, batch.UniformSettings, sprites, textures);
                        sprites.Clear();
                        textures.Clear();
                    }

                    // Add the texture
                    textureIndex = (uint)textures.Count;
                    textures.Add(sprite.Texture);
                }

                if (sprites.Count >= MaxSpritesPerBatch)
                {
                    // Too many sprites in this batch, split into new batch
                    Submit(commandBuffer, pipeline, batch.UniformSettings, sprites, textures);
                    sprites.Clear();
                    textures.Clear();
                }

                // Add the sprite to the batch
                sprites.Add(new SpriteInstanceData()
                {
                    Model = sprite.Model,

                    Color = sprite.Color,

                    UvOffset = sprite.UvOffset,
                    UvSize = sprite.UvSize,

                    TextureIndex = textureIndex.Value,
                });
            }

            // Submit any remaining sprites
            Submit(commandBuffer, pipeline, batch.UniformSettings, sprites, textures);
            sprites.Clear();
            textures.Clear();
        }
    }

    private void Submit(GraphicsCommandBuffer commandBuffer, PipelineCacheState pipeline, SpriteUniformDrawSettings settings, List<SpriteInstanceData> sprites, List<Texture2D> textures)
    {
        if (sprites.Count == 0)
        {
            return;
        }

        uniformBuffers.Cycle();
        using (uniformBuffers.Current.Map(out var data))
        {
            data[0] = new SpriteUniformData()
            {
                View = settings.View,
                Projection = settings.Projection,
            };
        }

        instanceBuffers.Cycle();
        using (instanceBuffers.Current.Map(out var data))
        {
            sprites.AsSpan().CopyTo(data);
        }

        commandBuffer.BindPipeline(pipeline.Pipeline);
        commandBuffer.BindVertexBuffer(instanceBuffers.Current);

        pipeline.TexturesVariable.SetTextures(textures.AsSpan());
        pipeline.UniformsVariable.SetBuffer(uniformBuffers.Current);
        commandBuffer.BindPipelineLayout(PipelineBindPoint.Graphics, pipeline.Pipeline.Layout);

        commandBuffer.Draw(new DrawDesc(4, sprites.Count));
    }

    private void ReleaseResources()
    {
        disposables.Dispose();
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;

        ReleaseResources();
        GC.SuppressFinalize(this);
    }

    ~SpriteBatchPass()
    {
        ReleaseResources();
    }

    private record struct PipelineCacheKey(Format ColorFormat, Format DepthFormat);

    private class PipelineCacheState : ShaderPipelineCacheState
    {
        public readonly ShaderPipelineVariable UniformsVariable;
        public readonly ShaderPipelineVariable TexturesVariable;

        public PipelineCacheState(ShaderPipeline pipeline) : base(pipeline)
        {
            UniformsVariable = pipeline.Layout.GetVariable("Uniforms");
            TexturesVariable = pipeline.Layout.GetVariable("Textures");
        }
    }
}