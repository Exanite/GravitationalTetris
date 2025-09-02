using System;
using System.Collections.Generic;
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

    private readonly ReloadableHandle<ShaderPipeline> pipeline;
    private ShaderPipelineLayout pipelineLayout = null!;
    private ShaderPipelineVariable textureVariable = null!;

    private readonly DisposableCollection disposables = new();

    public ToneMapPass(GraphicsContext graphicsContext, IResourceManager resourceManager)
    {
        var vertexModule = resourceManager.GetResource(EngineResources.Rendering.ScreenTriVertexModule);
        var fragmentModule = resourceManager.GetResource(GravitationalTetrisResources.ToneMapFragmentModule);

        var sampler = new TextureSampler(graphicsContext, new TextureSamplerDesc(Filter.Linear)).AddTo(disposables);

        pipeline = new ReloadableHandle<ShaderPipeline>((List<IHandle> dependencies, out ShaderPipeline resource, out ResourceChangedAction<ShaderPipeline> changedAction) =>
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
        }).AddTo(disposables);
    }

    public void Render(GraphicsCommandBuffer commandBuffer, Texture2D colorSource, Texture2D colorTarget)
    {
        commandBuffer.AddTransition(colorSource, new TransitionDesc(colorSource.State, ResourceState.FragmentShaderRead));
        commandBuffer.AddTransition(colorTarget, new TransitionDesc(colorTarget.State, ResourceState.Attachment));

        using (commandBuffer.BeginRenderPass(new RenderPassDesc([colorTarget])))
        {
            commandBuffer.BindPipeline(pipeline.Value);

            textureVariable.SetTexture(colorSource);
            commandBuffer.BindPipelineLayout(PipelineBindPoint.Graphics, pipelineLayout);

            commandBuffer.Draw(new DrawDesc(3));
        }
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

    ~ToneMapPass()
    {
        ReleaseResources();
    }
}
