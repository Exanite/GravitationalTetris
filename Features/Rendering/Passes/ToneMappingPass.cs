using System;
using System.Collections.Generic;
using Exanite.Core.Runtime;
using Exanite.Engine.Framework;
using Exanite.Engine.Graphics;
using Exanite.ResourceManagement;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Rendering.Passes;

public class ToneMappingPass : ITrackedDisposable
{
    public bool IsDisposed { get; private set; }

    private const Format ColorTargetFormat = Format.R32G32B32A32Sfloat;

    private readonly ReloadableHandle<ShaderPipeline> pipeline;
    private ShaderPipelineLayout pipelineLayout = null!;
    private ShaderPipelineVariable textureVariable = null!;

    private readonly DisposableCollection disposables = new();

    public ToneMappingPass(
        GraphicsContext graphicsContext,
        IResourceManager resourceManager)
    {
        var vertexModule = resourceManager.GetResource(EngineResources.ScreenTriVertexModule);
        var fragmentModule = resourceManager.GetResource(RenderingMod.ToneMapFragmentModule);

        var sampler = new TextureSampler(graphicsContext, new TextureSamplerDesc(Filter.Linear)).AddTo(disposables);

        pipeline = new ReloadableHandle<ShaderPipeline>((List<IHandle> dependencies, out ShaderPipeline resource, out Action<ShaderPipeline> unloadAction) =>
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
                        SrcAlphaBlendFactor = BlendFactor.Zero,
                        DstAlphaBlendFactor = BlendFactor.One,
                    },
                ],
            });

            pipelineLayout = resource.Layout;

            textureVariable = pipelineLayout.GetVariable("Texture");
            pipelineLayout.GetVariable("TextureSampler").SetSampler(sampler);

            unloadAction = resource =>
            {
                resource.Dispose();
                pipelineLayout = null!;
                textureVariable = null!;
            };
        }).AddTo(disposables);
    }

    public void Render(GraphicsCommandBuffer commandBuffer, Texture2D colorSource, Texture2D colorTarget)
    {
        commandBuffer.AddBarrier(new TextureBarrier(colorSource)
        {
            SrcStages = PipelineStageFlags2.ColorAttachmentOutputBit,
            SrcAccesses = AccessFlags2.ColorAttachmentWriteBit,

            DstStages = PipelineStageFlags2.FragmentShaderBit,
            DstAccesses = AccessFlags2.ShaderReadBit,
        });

        commandBuffer.AddBarrier(new TextureBarrier(colorTarget)
        {
            SrcStages = PipelineStageFlags2.FragmentShaderBit,
            SrcAccesses = AccessFlags2.None,

            DstStages = PipelineStageFlags2.ColorAttachmentOutputBit,
            DstAccesses = AccessFlags2.ColorAttachmentWriteBit,
        });

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

    ~ToneMappingPass()
    {
        ReleaseResources();
    }
}
