using System;
using System.Collections.Generic;
using Exanite.Core.Pooling;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Engine.Graphics;
using Exanite.ResourceManagement;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Sprites;

public class SpriteBatcher : IDisposable
{
    private const int MaxSpritesPerBatch = 1024;
    private const int MaxTexturesPerBatch = 64;

    private readonly ShaderPipelineCache<PipelineCacheKey, PipelineCacheState> pipelines;

    private readonly Pool<Batch> batchPool;

    private readonly CycledBuffer<SpriteUniformData> uniformBuffers;
    private readonly CycledBuffer<SpriteInstanceData> instanceBuffers;

    private readonly DisposableCollection disposables = new();

    public SpriteBatcher(GraphicsContext graphicsContext, ResourceManager resourceManager)
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

        batchPool = new Pool<Batch>(
            create: () => new Batch(this),
            onAcquire: batch =>
            {
                batch.Clear();
            },
            onRelease: batch =>
            {
                batch.Clear();
            },
            onDestroy: batch =>
            {
                batch.Clear();
            }
        );

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

    public void Dispose()
    {
        disposables.Dispose();
    }

    public Batch Acquire(SpriteUniformDrawSettings settings)
    {
        var batch = batchPool.Acquire();
        batch.Settings = settings;

        return batch;
    }

    public void Release(Batch batch)
    {
        AssertUtility.IsTrue(batch.SpriteBatcher == this, $"{nameof(Batch)} was released to the wrong {nameof(SpriteBatcher)}");
        batchPool.Release(batch);
    }

    public void Submit(Batch batch)
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
                    Submit(batch.Settings, sprites, textures);
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
                Submit(batch.Settings, sprites, textures);
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
        Submit(batch.Settings, sprites, textures);
        sprites.Clear();
        textures.Clear();

        batchPool.Release(batch);
    }

    private void Submit(SpriteUniformDrawSettings settings, List<SpriteInstanceData> sprites, List<Texture2D> textures)
    {
        if (sprites.Count == 0)
        {
            return;
        }

        var commandBuffer = settings.CommandBuffer;
        var colorTarget = settings.ColorTarget;
        var depthTarget = settings.DepthTarget;
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
    }

    public class Batch : IDisposable
    {
        internal readonly SpriteBatcher SpriteBatcher;

        /// <summary>
        /// Draw settings that are the same for each submitted sprite.
        /// </summary>
        public SpriteUniformDrawSettings Settings;

        internal List<SpriteInstanceDrawSettings> Sprites { get; } = new();

        internal Batch(SpriteBatcher spriteBatcher)
        {
            this.SpriteBatcher = spriteBatcher;
        }

        /// <summary>
        /// Draws an individual sprite with the specified settings.
        /// </summary>
        public void Draw(SpriteInstanceDrawSettings settings)
        {
            Sprites.Add(settings);
        }

        /// <summary>
        /// Clears all submitted sprites. Does not reset <see cref="Settings"/>.
        /// </summary>
        public void Clear()
        {
            Sprites.Clear();
        }

        /// <summary>
        /// Submits the batch. This can be called multiple times.
        /// </summary>
        public void Submit()
        {
            SpriteBatcher.Submit(this);
        }

        /// <summary>
        /// Submits, clears, and releases the batch.
        /// </summary>
        public void Dispose()
        {
            Submit();
            Clear();
            SpriteBatcher.Release(this);
        }
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
