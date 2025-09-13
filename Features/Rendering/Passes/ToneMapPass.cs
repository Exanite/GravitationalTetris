using System;
using Exanite.Core.Runtime;
using Exanite.Engine.Framework;
using Exanite.Engine.Graphics;
using Exanite.ResourceManagement;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Rendering.Passes;

public class ToneMapPass : ITrackedDisposable
{
    public bool IsDisposed { get; private set; }

    private const Format ColorTargetFormat = Format.R32G32B32A32Sfloat;

    private readonly Reloadable<ShaderPipeline> pipeline;
    private ShaderPipelineLayout pipelineLayout = null!;
    private ShaderPipelineVariable textureVariable = null!;

    private readonly Lifetime lifetime = new();

    public ToneMapPass(GraphicsContext graphicsContext, IResourceManager resourceManager)
    {
        var vertexModule = resourceManager.GetResource(EngineResources.Rendering.ScreenTriVertexModule);
        var fragmentModule = resourceManager.GetResource(GravitationalTetrisResources.ToneMapFragmentModule);

        var sampler = new TextureSampler(graphicsContext, new TextureSamplerDesc(Filter.Linear)).DisposeWith(lifetime);

        pipeline = new Reloadable<ShaderPipeline>((dependencies, out resource, out changedAction) =>
        {
            dependencies.Add(vertexModule);
            dependencies.Add(fragmentModule);

            resource = new ShaderPipeline(graphicsContext, new ShaderPipelineDesc()
            {
                ShaderModules = [vertexModule.Value, fragmentModule.Value],

                Topology = PrimitiveTopology.TriangleList,

                ColorAttachmentFormats = [ColorTargetFormat],
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

            changedAction = (previous, current) =>
            {
                previous?.Dispose();

                if (current != null)
                {
                    pipelineLayout = current.Layout;
                    pipelineLayout.GetVariable("TextureSampler").SetSampler(sampler);

                    textureVariable = pipelineLayout.GetVariable("Texture");
                }
                else
                {
                    pipelineLayout = null!;
                    textureVariable = null!;
                }
            };
        }).DisposeWith(lifetime);
    }

    public void Render(GraphicsCommandBuffer commandBuffer, Texture2D colorSource, Texture2D colorTarget)
    {
        commandBuffer.AddTransition(colorSource, ResourceState.ShaderRead);
        commandBuffer.AddTransition(colorTarget, ResourceState.Attachment);

        using (commandBuffer.BeginRenderPass([colorTarget]))
        {
            commandBuffer.BindPipeline(pipeline.Value);

            textureVariable.SetTexture(colorSource);
            commandBuffer.BindPipelineLayout(PipelineBindPoint.Graphics, pipelineLayout);

            commandBuffer.Draw(3);
        }
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

    ~ToneMapPass()
    {
        Dispose();
    }
}
