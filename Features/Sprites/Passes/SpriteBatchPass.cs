using System;
using System.Collections.Generic;
using Exanite.Core.Pooling;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Engine.Graphics;
using Exanite.ResourceManagement;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Sprites.Passes;

public class SpriteBatchPass : ITrackedDisposable
{
    public bool IsDisposed { get; private set; }

    private const int MaxSpritesPerBatch = 1024;
    private const int MaxTexturesPerBatch = 64;

    private readonly ShaderPipelineCache<PipelineCacheKey, PipelineCacheState> pipelines;

    private readonly Lifetime lifetime = new();

    public SpriteBatchPass(GraphicsContext graphicsContext, IResourceManager resourceManager)
    {
        var vertexModule = resourceManager.GetResource(GravitationalTetrisResources.SpriteVertexModule);
        var fragmentModule = resourceManager.GetResource(GravitationalTetrisResources.SpriteFragmentModule);

        var sampler = new TextureSampler("SpriteBatch", graphicsContext, new TextureSamplerDesc(Filter.Nearest)).DisposeWith(lifetime);

        pipelines = new ShaderPipelineCache<PipelineCacheKey, PipelineCacheState>(key =>
        {
            var pipeline = new ShaderPipeline("SpriteBatch", graphicsContext, new ShaderPipelineDesc()
            {
                ShaderModules = [vertexModule.Value, fragmentModule.Value],

                Topology = PrimitiveTopology.TriangleStrip,
                InstanceLayout = SpriteInstanceData.Layout,

                ColorAttachmentFormats = [key.ColorFormat],
                ColorAttachmentBlends = [ShaderPipelineBlendDesc.PreservingAlphaBlend],

                DepthAttachmentFormat = key.DepthFormat,
            });

            pipeline.Layout.GetVariable("TextureSampler").SetSampler(sampler);

            return new PipelineCacheState(pipeline);
        }).DisposeWith(lifetime);
    }

    public void Render(SpriteBatch batch, SpriteUniformDrawSettings uniformSettings)
    {
        var commandBuffer = uniformSettings.CommandBuffer;
        var colorTarget = uniformSettings.ColorTarget;
        var depthTarget = uniformSettings.DepthTarget;

        var pipeline = pipelines.GetPipeline(new PipelineCacheKey(colorTarget.Desc.Format, depthTarget.Desc.Format));

        commandBuffer.AddTransition(colorTarget, ResourceState.Attachment);
        commandBuffer.AddTransition(depthTarget, ResourceState.Attachment);

        using (commandBuffer.BeginRenderPass([colorTarget], depthTarget))
        {
            using var _ = ListPool<Texture2D>.Acquire(out var textures);
            using var __ = ListPool<SpriteInstanceData>.Acquire(out var sprites);

            commandBuffer.BindPipeline(pipeline.Pipeline);

            BufferBindingInfo uniformBuffer;
            using (commandBuffer.AllocateTempUniformBuffer<SpriteUniformData>(out var data, out uniformBuffer))
            {
                data[0] = new SpriteUniformData()
                {
                    View = uniformSettings.View,
                    Projection = uniformSettings.Projection,
                };
            }
            pipeline.UniformsVariable.SetBuffer(uniformBuffer);

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
                        Submit(commandBuffer, pipeline, sprites, textures);
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
                    Submit(commandBuffer, pipeline, sprites, textures);
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
            Submit(commandBuffer, pipeline, sprites, textures);
            sprites.Clear();
            textures.Clear();
        }
    }

    private void Submit(GraphicsCommandBuffer commandBuffer, PipelineCacheState pipeline, List<SpriteInstanceData> sprites, List<Texture2D> textures)
    {
        if (sprites.Count == 0)
        {
            return;
        }

        BufferBindingInfo instanceBuffer;
        using (commandBuffer.AllocateTempVertexBuffer<SpriteInstanceData>(sprites.Count, out var data, out instanceBuffer))
        {
            sprites.AsSpan().CopyTo(data);
        }
        commandBuffer.BindVertexBuffer(instanceBuffer);

        pipeline.TexturesVariable.SetTextures(textures.AsSpan());

        commandBuffer.BindPipelineLayout(PipelineBindPoint.Graphics, pipeline.Pipeline.Layout);

        commandBuffer.Draw(4, sprites.Count);
    }

    private void ReleaseResources()
    {
        lifetime.Dispose();
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
        Dispose();
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
