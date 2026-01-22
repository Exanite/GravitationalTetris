using System;
using System.Collections.Generic;
using System.Numerics;
using Exanite.Core.Numerics;
using Exanite.Core.Runtime;
using Exanite.Engine;
using Exanite.Engine.Graphics;
using Exanite.ResourceManagement;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Rendering.Passes;

public class BloomPass : ITrackedDisposable
{
    public bool IsDisposed { get; private set; }

    private Reloadable<ShaderPipeline> downPipeline;
    private ShaderPipelineLayout downPipelineLayout = null!;
    private ShaderPipelineVariable downUniformsVariable = null!;
    private ShaderPipelineVariable downTextureVariable = null!;

    private Reloadable<ShaderPipeline> upPipeline;
    private ShaderPipelineLayout upPipelineLayout = null!;
    private ShaderPipelineVariable upUniformsVariable = null!;
    private ShaderPipelineVariable upTextureVariable = null!;

    private Vector2Int currentSize;
    private readonly List<Texture2D> renderTextures = new();

    private readonly Lifetime lifetime = new();

    private readonly GraphicsContext graphicsContext;

    private float referenceResolutionHeight = 1080;
    private int maxIterationCount = 6;

    public float ReferenceResolutionHeight
    {
        get => referenceResolutionHeight;
        set
        {
            if (referenceResolutionHeight.Equals(value))
            {
                return;
            }

            referenceResolutionHeight = value;
            RecreateRenderTextures(currentSize);
        }
    }

    public int MaxIterationCount
    {
        get => maxIterationCount;
        set
        {
            if (maxIterationCount == value)
            {
                return;
            }

            maxIterationCount = value;
            RecreateRenderTextures(currentSize);
        }
    }

    public float BloomIntensity { get; set; } = 0.05f;

    public BloomPass(GraphicsContext graphicsContext, IResourceManager resourceManager)
    {
        this.graphicsContext = graphicsContext;

        var vertexModule = resourceManager.GetResource(EngineResources.ExaniteEngine.Rendering.ScreenTriVertexModule);
        var downFragmentModule = resourceManager.GetResource(GravitationalTetrisResources.BloomDownFragmentModule);
        var upFragmentModule = resourceManager.GetResource(GravitationalTetrisResources.BloomUpFragmentModule);

        var sampler = new TextureSampler("Bloom", graphicsContext, new TextureSamplerDesc(Filter.Linear)).DisposeWith(lifetime);

        downPipeline = new Reloadable<ShaderPipeline>((dependencies, out resource, out changedAction) =>
        {
            dependencies.Add(vertexModule);
            dependencies.Add(downFragmentModule);

            resource = new ShaderPipeline("Bloom", graphicsContext, new ShaderPipelineDesc()
            {
                ShaderModules = [vertexModule.Value, downFragmentModule.Value],

                Topology = PrimitiveTopology.TriangleList,

                ColorAttachmentFormats = [Format.R32G32B32A32Sfloat],
                ColorAttachmentBlends = [ShaderPipelineBlendDesc.Opaque],
            });

            changedAction = (previous, current) =>
            {
                previous?.Dispose();

                if (current != null)
                {
                    downPipelineLayout = current.Layout;
                    downPipelineLayout.GetVariable("TextureSampler").SetSampler(sampler);

                    downUniformsVariable = downPipelineLayout.GetVariable("Uniforms");
                    downTextureVariable = downPipelineLayout.GetVariable("Texture");
                }
                else
                {
                    downPipelineLayout = null!;
                    downUniformsVariable = null!;
                    downTextureVariable = null!;
                }
            };
        }).DisposeWith(lifetime);

        upPipeline = new Reloadable<ShaderPipeline>((dependencies, out resource, out changedAction) =>
        {
            dependencies.Add(vertexModule);
            dependencies.Add(upFragmentModule);

            resource = new ShaderPipeline("Bloom", graphicsContext, new ShaderPipelineDesc()
            {
                ShaderModules = [vertexModule.Value, upFragmentModule.Value],

                Topology = PrimitiveTopology.TriangleList,

                ColorAttachmentFormats = [Format.R32G32B32A32Sfloat],
                ColorAttachmentBlends = [ShaderPipelineBlendDesc.Additive],
            });

            changedAction = (previous, current) =>
            {
                previous?.Dispose();

                if (current != null)
                {
                    upPipelineLayout = current.Layout;
                    upPipelineLayout.GetVariable("TextureSampler").SetSampler(sampler);

                    upUniformsVariable = upPipelineLayout.GetVariable("Uniforms");
                    upTextureVariable = upPipelineLayout.GetVariable("Texture");
                }
                else
                {
                    upPipelineLayout = null!;
                    upUniformsVariable = null!;
                    upTextureVariable = null!;
                }
            };
        }).DisposeWith(lifetime);
    }

    public void Render(GraphicsCommandBuffer commandBuffer, Texture2D colorSourceAndTarget)
    {
        ResizeRenderTextures(colorSourceAndTarget.Size);

        if (renderTextures.Count != 0)
        {
            // Down sample
            for (var i = 0; i < renderTextures.Count; i++)
            {
                var previousTarget = i > 0 ? renderTextures[i - 1] : colorSourceAndTarget;
                var currentTarget = renderTextures[i];

                commandBuffer.AddTransition(currentTarget, ResourceState.Attachment);
                commandBuffer.AddTransition(previousTarget, ResourceState.ShaderRead);

                using (commandBuffer.BeginRenderPass("BloomDown", [currentTarget]))
                {
                    commandBuffer.ClearColorAttachment(Vector4.Zero);

                    commandBuffer.BindPipeline(downPipeline.Value);

                    BufferBindingInfo downUniformBuffer;
                    using (commandBuffer.AllocateTempUniformBuffer<BloomDownUniformData>(out var data, out downUniformBuffer))
                    {
                        data[0].FilterStep = Vector2.One / currentTarget.Size;
                    }

                    downTextureVariable.SetTexture(previousTarget);
                    downUniformsVariable.SetBuffer(downUniformBuffer);
                    commandBuffer.BindPipelineLayout(PipelineBindPoint.Graphics, downPipelineLayout);

                    commandBuffer.Draw(3);
                }
            }

            // Up sample
            var aspectRatio = (float)colorSourceAndTarget.Size.X / colorSourceAndTarget.Size.Y;
            var step = 0.005f;
            var upFilterStep = new Vector2(step / aspectRatio, step);

            BufferBindingInfo upUniformBuffer;
            using (commandBuffer.AllocateTempUniformBuffer<BloomUpUniformData>(out var data, out upUniformBuffer))
            {
                data[0] = new BloomUpUniformData
                {
                    FilterStep = upFilterStep,
                    Alpha = 1,
                };
            }

            for (var i = renderTextures.Count - 2; i >= 0; i--)
            {
                var previousTarget = renderTextures[i + 1];
                var currentTarget = renderTextures[i];

                commandBuffer.AddTransition(currentTarget, ResourceState.Attachment);
                commandBuffer.AddTransition(previousTarget, ResourceState.ShaderRead);

                using (commandBuffer.BeginRenderPass("BloomUp", [currentTarget]))
                {
                    commandBuffer.BindPipeline(upPipeline.Value);

                    upTextureVariable.SetTexture(previousTarget);
                    upUniformsVariable.SetBuffer(upUniformBuffer);
                    commandBuffer.BindPipelineLayout(PipelineBindPoint.Graphics, upPipelineLayout);

                    commandBuffer.Draw(3);
                }
            }

            // Composite bloom with source
            commandBuffer.AddTransition(colorSourceAndTarget, ResourceState.Attachment);
            commandBuffer.AddTransition(renderTextures[0], ResourceState.ShaderRead);

            using (commandBuffer.BeginRenderPass("BloomComposite", [colorSourceAndTarget]))
            {
                commandBuffer.BindPipeline(upPipeline.Value);

                using (commandBuffer.AllocateTempUniformBuffer<BloomUpUniformData>(out var data, out upUniformBuffer))
                {
                    data[0] = new BloomUpUniformData
                    {
                        FilterStep = upFilterStep,
                        Alpha = BloomIntensity,
                    };
                }

                upTextureVariable.SetTexture(renderTextures[0]);
                upUniformsVariable.SetBuffer(upUniformBuffer);
                commandBuffer.BindPipelineLayout(PipelineBindPoint.Graphics, upPipelineLayout);

                commandBuffer.Draw(3);
            }
        }
    }

    private void ResizeRenderTextures(Vector2Int size)
    {
        if (currentSize == size)
        {
            return;
        }

        RecreateRenderTextures(size);
    }

    private void RecreateRenderTextures(Vector2Int size)
    {
        foreach (var texture in renderTextures)
        {
            texture.Dispose();
        }
        renderTextures.Clear();

        var aspectRatio = (float)size.X / size.Y;

        // Use constant height to make bloom effect render the same regardless of resolution
        var width = ReferenceResolutionHeight * aspectRatio;
        var height = ReferenceResolutionHeight;

        // Dispose existing textures
        foreach (var texture in renderTextures)
        {
            texture.Dispose();
        }
        renderTextures.Clear();

        // Recreate
        for (var i = 0; i < MaxIterationCount; i++)
        {
            var iWidth = (int)width;
            var iHeight = (int)height;

            if (iWidth == 0 || iHeight == 0)
            {
                return;
            }

            renderTextures.Add(new Texture2D($"Bloom {i}", graphicsContext, new TextureDesc2D()
            {
                Size = new Vector2Int(iWidth, iHeight),
                Format = Format.R32G32B32A32Sfloat,
                Usages = ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.SampledBit,
            }, new TextureViewDesc()
            {
                Aspects = ImageAspectFlags.ColorBit,
            }));

            width /= 2;
            height /= 2;
        }

        // Save size
        currentSize = size;
    }

    private void ReleaseResources()
    {
        lifetime.Dispose();

        foreach (var texture in renderTextures)
        {
            texture.Dispose();
        }
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

    ~BloomPass()
    {
        Dispose();
    }
}
