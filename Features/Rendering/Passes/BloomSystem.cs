using System;
using System.Collections.Generic;
using System.Numerics;
using Exanite.Core.Numerics;
using Exanite.Core.Runtime;
using Exanite.Engine.Framework;
using Exanite.Engine.Graphics;
using Exanite.ResourceManagement;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Rendering.Passes;

public class BloomPass : ITrackedDisposable
{
    public bool IsDisposed { get; private set; }

    private const float ReferenceResolutionHeight = 1080;
    private const int MaxIterationCount = 6;
    private const float BloomIntensity = 0.05f;

    private CycledBuffer<BloomDownUniformData> downUniformBuffer;
    private ReloadableHandle<ShaderPipeline> downPipeline;
    private ShaderPipelineLayout downPipelineLayout = null!;
    private ShaderPipelineVariable downUniformsVariable = null!;
    private ShaderPipelineVariable downTextureVariable = null!;

    private CycledBuffer<BloomUpUniformData> upUniformBuffer;
    private ReloadableHandle<ShaderPipeline> upPipeline;
    private ShaderPipelineLayout upPipelineLayout = null!;
    private ShaderPipelineVariable upUniformsVariable = null!;
    private ShaderPipelineVariable upTextureVariable = null!;

    private Vector2Int currentSize;
    private readonly List<Texture2D> renderTextures = new();

    private readonly DisposableCollection disposables = new();

    private readonly GraphicsContext graphicsContext;

    public BloomPass(
        GraphicsContext graphicsContext,
        IResourceManager resourceManager)
    {
        this.graphicsContext = graphicsContext;
        
        var vertexModule = resourceManager.GetResource(EngineResources.ScreenTriVertexModule);
        var downFragmentModule = resourceManager.GetResource(RenderingMod.BloomDownFragmentModule);
        var upFragmentModule = resourceManager.GetResource(RenderingMod.BloomUpFragmentModule);

        {
            downUniformBuffer = new CycledBuffer<BloomDownUniformData>(graphicsContext, new BufferDesc()
            {
                Usages = BufferUsageFlags.UniformBufferBit,
                MapType = AllocationMapType.SequentialWrite,
            }, 1).AddTo(disposables);

            downPipeline = new ReloadableHandle<ShaderPipeline>((List<IHandle> dependencies, out ShaderPipeline resource, out Action<ShaderPipeline> action) =>
            {
                dependencies.Add(vertexModule);
                dependencies.Add(downFragmentModule);

                resource = new ShaderPipeline(graphicsContext, new ShaderPipelineDesc()
                {
                    ShaderModules = [vertexModule.Value, downFragmentModule.Value],

                    Topology = PrimitiveTopology.TriangleList,

                    ColorAttachmentFormats = [Format.R32G32B32A32Sfloat],
                    ColorAttachmentBlends =
                    [
                        new ShaderPipelineColorAttachmentBlendDesc()
                        {
                            ColorBlendOp = BlendOp.Add,
                            SrcColorBlendFactor = BlendFactor.One,
                            DstColorBlendFactor = BlendFactor.Zero,

                            AlphaBlendOp = BlendOp.Add,
                            SrcAlphaBlendFactor = BlendFactor.One,
                            DstAlphaBlendFactor = BlendFactor.Zero,
                        },
                    ],
                });

                downPipelineLayout = resource.Layout;
                downUniformsVariable = downPipelineLayout.GetVariable("Uniforms");
                downTextureVariable = downPipelineLayout.GetVariable("Texture");

                action = resource =>
                {
                    resource.Dispose();
                };
            }).AddTo(disposables);
        }

        {
            upUniformBuffer = new CycledBuffer<BloomUpUniformData>(graphicsContext, new BufferDesc()
            {
                Usages = BufferUsageFlags.UniformBufferBit,
                MapType = AllocationMapType.SequentialWrite,
            }, 1).AddTo(disposables);

            upPipeline = new ReloadableHandle<ShaderPipeline>((List<IHandle> dependencies, out ShaderPipeline resource, out Action<ShaderPipeline> action) =>
            {
                dependencies.Add(vertexModule);
                dependencies.Add(upFragmentModule);

                resource = new ShaderPipeline(graphicsContext, new ShaderPipelineDesc()
                {
                    ShaderModules = [vertexModule.Value, upFragmentModule.Value],

                    Topology = PrimitiveTopology.TriangleList,

                    ColorAttachmentFormats = [Format.R32G32B32A32Sfloat],
                    ColorAttachmentBlends =
                    [
                        new ShaderPipelineColorAttachmentBlendDesc()
                        {
                            ColorBlendOp = BlendOp.Add,
                            SrcColorBlendFactor = BlendFactor.One,
                            DstColorBlendFactor = BlendFactor.Zero,

                            AlphaBlendOp = BlendOp.Add,
                            SrcAlphaBlendFactor = BlendFactor.One,
                            DstAlphaBlendFactor = BlendFactor.Zero,
                        },
                    ],
                });

                upPipelineLayout = resource.Layout;
                upUniformsVariable = downPipelineLayout.GetVariable("Uniforms");
                upTextureVariable = downPipelineLayout.GetVariable("Texture");

                action = resource =>
                {
                    resource.Dispose();
                };
            }).AddTo(disposables);
        }
    }

    public void Render(GraphicsCommandBuffer commandBuffer, Texture2D colorSourceAndTarget)
    {
        ResizeRenderTextures(colorSourceAndTarget.Desc.Size);

        if (renderTextures.Count != 0)
        {
            // Down sample
            deviceContext.SetPipelineState(downPipeline);
            for (var i = 0; i < renderTextures.Count; i++)
            {
                var previousRenderTarget = i > 0 ? renderTextures[i - 1].RenderTarget : GetSourceRenderTexture().RenderTarget;
                var currentRenderTarget = renderTextures[i].RenderTarget;
                var currentTexture = renderTextures[i];

                downTextureVariable?.Set(previousRenderTarget, SetShaderResourceFlags.AllowOverwrite);
                renderTargets[0] = currentRenderTarget;

                using (downUniformBuffer.Map(MapType.Write, MapFlags.Discard, out var downUniformData))
                {
                    var textureDesc = currentTexture.Handle.GetDesc();
                    downUniformData[0].FilterStep = Vector2.One / textureDesc.GetSize();
                }

                deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);
                deviceContext.CommitShaderResources(downPipelineLayout, ResourceStateTransitionMode.Transition);
                deviceContext.Draw(new DrawAttribs()
                {
                    NumVertices = 4,
                    Flags = DrawFlags.VerifyAll,
                });
            }

            var aspectRatio = window.AspectRatio;
            var step = 0.005f;
            var localUpUniformData = new BloomUpUniformData
            {
                FilterStep = new Vector2(step / aspectRatio, step),
                Alpha = 1,
            };

            using (upUniformBuffer.Map(MapType.Write, MapFlags.Discard, out var upUniformData))
            {
                upUniformData[0] = localUpUniformData;
            }

            // Up sample
            deviceContext.SetPipelineState(upPipeline);
            for (var i = renderTextures.Count - 2; i >= 0; i--)
            {
                var previousRenderTarget = renderTextures[i + 1].RenderTarget;
                var currentRenderTarget = renderTextures[i].RenderTarget;

                upTextureVariable?.Set(previousRenderTarget, SetShaderResourceFlags.AllowOverwrite);
                renderTargets[0] = currentRenderTarget;

                deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);
                deviceContext.CommitShaderResources(upPipelineLayout, ResourceStateTransitionMode.Transition);
                deviceContext.Draw(new DrawAttribs()
                {
                    NumVertices = 4,
                    Flags = DrawFlags.VerifyAll,
                });
            }

            // Draw bloom to world RT
            renderTargets[0] = GetSourceRenderTexture().RenderTarget;
            deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);

            deviceContext.SetPipelineState(upPipeline);

            localUpUniformData.Alpha = BloomIntensity;
            using (upUniformBuffer.Map(MapType.Write, MapFlags.Discard, out var upUniformData))
            {
                upUniformData[0] = localUpUniformData;
            }

            upTextureVariable?.Set(renderTextures[0].RenderTarget, SetShaderResourceFlags.AllowOverwrite);
            deviceContext.CommitShaderResources(upPipelineLayout, ResourceStateTransitionMode.Transition);
            deviceContext.Draw(new DrawAttribs()
            {
                NumVertices = 4,
                Flags = DrawFlags.VerifyAll,
            });
        }
    }

    private void ResizeRenderTextures(Vector2Int size)
    {
        if (currentSize != size)
        {
            return;
        }

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

            renderTextures.Add(new Texture2D(graphicsContext, new TextureDesc2D()
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
        disposables.Dispose();

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
        ReleaseResources();
    }
}
