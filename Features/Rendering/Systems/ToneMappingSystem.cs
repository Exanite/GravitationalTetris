using System;
using System.Collections.Generic;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Timing;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class ToneMappingSystem : GameSystem, ISetupSystem, IRenderSystem, ITeardownSystem
{
    private Buffer<ToneMapUniformData> uniformBuffer = null!;
    private ReloadableHandle<IPipelineState> pipeline = null!;
    private IShaderResourceBinding shaderResourceBinding = null!;
    private IShaderResourceVariable? textureVariable;

    private readonly ITextureView[] renderTargets = new ITextureView[1];

    private readonly RenderingContext renderingContext;
    private readonly IResourceManager resourceManager;
    private readonly RenderingResourcesSystem renderingResourcesSystem;
    private readonly ITime time;

    public ToneMappingSystem(RenderingContext renderingContext, IResourceManager resourceManager, RenderingResourcesSystem renderingResourcesSystem, ITime time)
    {
        this.renderingContext = renderingContext;
        this.resourceManager = resourceManager;
        this.renderingResourcesSystem = renderingResourcesSystem;
        this.time = time;
    }

    public void Setup()
    {
        var renderDevice = renderingContext.RenderDevice;

        var vShader = resourceManager.GetResource(RenderingMod.ScreenVertexModule);
        var pShader = resourceManager.GetResource(RenderingMod.ToneMapFragmentModule);

        uniformBuffer = new Buffer<ToneMapUniformData>(renderingContext, new BufferDesc()
        {
            Usage = Usage.Dynamic,
            BindFlags = BindFlags.UniformBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
        }, 1);

        pipeline = new ReloadableHandle<IPipelineState>((List<IHandle> dependencies, out IPipelineState resource, out Action<IPipelineState> unloadAction) =>
        {
            dependencies.Add(vShader);
            dependencies.Add(pShader);

            resource = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo()
            {
                GraphicsPipeline = new GraphicsPipelineDesc()
                {
                    PrimitiveTopology = PrimitiveTopology.TriangleStrip,

                    NumRenderTargets = 1,
                    RTVFormats = [CommonTextureFormats.HdrTextureFormat],

                    RasterizerDesc = new RasterizerStateDesc() { CullMode = CullMode.Back, FrontCounterClockwise = true},
                    DepthStencilDesc = new DepthStencilStateDesc() { DepthEnable = false },
                },

                Vs = vShader.Value.Handle,
                Ps = pShader.Value.Handle,
            });

            pipeline.Value.GetStaticVariableByName(ShaderType.Pixel, "Uniforms")?.Set(uniformBuffer.Handle, SetShaderResourceFlags.None);

            shaderResourceBinding = pipeline.Value.CreateShaderResourceBinding(true);
            textureVariable = shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture");

            unloadAction = resource =>
            {
                resource.Dispose();
                shaderResourceBinding.Dispose();
                shaderResourceBinding = null!;
                textureVariable = null!;
            };
        });
    }

    public void Render()
    {
        var deviceContext = renderingContext.DeviceContext;

        renderTargets[0] = renderingResourcesSystem.WorldColor.RenderTarget;
        deviceContext.SetRenderTargets(renderTargets, renderingResourcesSystem.WorldDepth.DepthStencil, ResourceStateTransitionMode.Transition);

        using (uniformBuffer.Map(MapType.Write, MapFlags.Discard, out var uniformData))
        {
            uniformData[0].Time = time.Time;
        }

        textureVariable?.Set(renderingResourcesSystem.WorldColor.RenderTarget, SetShaderResourceFlags.AllowOverwrite);

        deviceContext.SetPipelineState(pipeline.Value);
        deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
        deviceContext.Draw(new DrawAttribs()
        {
            NumVertices = 4,
            Flags = DrawFlags.VerifyAll,
        });
    }

    public void Teardown()
    {
        uniformBuffer.Dispose();
        pipeline.Dispose();
    }
}
