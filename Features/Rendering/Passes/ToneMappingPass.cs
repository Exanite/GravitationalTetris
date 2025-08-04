using System;
using System.Collections.Generic;
using Exanite.Core.Runtime;
using Exanite.Engine.Graphics;
using Exanite.Engine.Timing;
using Exanite.ResourceManagement;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Rendering.Passes;

public class ToneMappingPass : ITrackedDisposable
{
    public bool IsDisposed { get; private set; }

    private const Format ColorTargetFormat = Format.R32G32B32A32Sfloat;

    private readonly CycledBuffer<ToneMapUniformData> uniformBuffer;
    private readonly ReloadableHandle<ShaderPipeline> pipeline;
    private ShaderPipelineLayout pipelineLayout = null!;
    private ShaderPipelineVariable uniformsVariable = null!;
    private ShaderPipelineVariable textureVariable = null!;

    private readonly DisposableCollection disposables = new();

    private readonly ITime time;

    public ToneMappingPass(
        GraphicsContext graphicsContext,
        IResourceManager resourceManager,
        ITime time)
    {
        this.time = time;

        var vertexModule = resourceManager.GetResource(RenderingMod.ScreenVertexModule);
        var fragmentModule = resourceManager.GetResource(RenderingMod.ToneMapFragmentModule);

        uniformBuffer = new CycledBuffer<ToneMapUniformData>(graphicsContext, new BufferDesc()
        {
            Usages = BufferUsageFlags.UniformBufferBit,
            MapType = AllocationMapType.SequentialWrite,
        }, 1).AddTo(disposables);

        pipeline = new ReloadableHandle<ShaderPipeline>((List<IHandle> dependencies, out ShaderPipeline resource, out Action<ShaderPipeline> unloadAction) =>
        {
            dependencies.Add(vertexModule);
            dependencies.Add(fragmentModule);

            resource = new ShaderPipeline(graphicsContext, new ShaderPipelineDesc()
            {
                ShaderModules = [vertexModule.Value, fragmentModule.Value],

                Topology = PrimitiveTopology.TriangleList,

                ColorAttachmentFormats = [ColorTargetFormat],
            });

            pipelineLayout = resource.Layout;
            uniformsVariable = pipelineLayout.GetVariable("Uniforms");
            textureVariable = pipelineLayout.GetVariable("Texture");

            unloadAction = resource =>
            {
                resource.Dispose();
                pipelineLayout = null!;
                uniformsVariable = null!;
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
            uniformBuffer.Cycle();
            using (uniformBuffer.Current.Map(out var data))
            {
                data[0].Time = time.Time;
            }

            uniformsVariable.SetBuffer(uniformBuffer.Current);
            textureVariable.SetTexture(colorSource);

            commandBuffer.BindPipeline(pipeline.Value);
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
