using Exanite.Engine.Ecs.Systems;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class RenderWorldToMainSystem : GameSystem, ISetupSystem, IRenderSystem, ITeardownSystem
{
    private IPipelineState passthroughPipeline = null!;
    private IShaderResourceBinding passthroughResources = null!;
    private IShaderResourceVariable? passthroughTextureVariable;

    private readonly ITextureView[] renderTargets = new ITextureView[1];

    private readonly RenderingContext renderingContext;
    private readonly IResourceManager resourceManager;
    private readonly RenderingResourcesSystem renderingResourcesSystem;
    private readonly Swapchain swapchain;

    public RenderWorldToMainSystem(RenderingContext renderingContext, IResourceManager resourceManager, RenderingResourcesSystem renderingResourcesSystem, Swapchain swapchain)
    {
        this.renderingContext = renderingContext;
        this.resourceManager = resourceManager;
        this.renderingResourcesSystem = renderingResourcesSystem;
        this.swapchain = swapchain;
    }

    public void Setup()
    {
        var renderDevice = renderingContext.RenderDevice;

        var vShader = resourceManager.GetResource(RenderingMod.ScreenVertexModule);
        var pShaderPassthrough = resourceManager.GetResource(RenderingMod.PassthroughFragmentModule);

        passthroughPipeline = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo()
        {
            GraphicsPipeline = new GraphicsPipelineDesc()
            {
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,

                NumRenderTargets = 1,
                RTVFormats = [CommonTextureFormats.SrgbTextureFormat],

                RasterizerDesc = new RasterizerStateDesc() { CullMode = CullMode.Back, FrontCounterClockwise = true},
                DepthStencilDesc = new DepthStencilStateDesc() { DepthEnable = false },
            },

            Vs = vShader.Value.Handle,
            Ps = pShaderPassthrough.Value.Handle,
        });

        passthroughResources = passthroughPipeline.CreateShaderResourceBinding(true);
        passthroughTextureVariable = passthroughResources.GetVariableByName(ShaderType.Pixel, "Texture");
    }

    public void Render()
    {
        var deviceContext = renderingContext.DeviceContext;

        renderTargets[0] = swapchain.Handle.GetCurrentBackBufferRTV();
        deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);

        passthroughTextureVariable?.Set(renderingResourcesSystem.WorldColor.RenderTarget, SetShaderResourceFlags.AllowOverwrite);

        deviceContext.SetPipelineState(passthroughPipeline);
        deviceContext.CommitShaderResources(passthroughResources, ResourceStateTransitionMode.Transition);
        deviceContext.Draw(new DrawAttribs()
        {
            NumVertices = 4,
            Flags = DrawFlags.VerifyAll,
        });

        deviceContext.SetRenderTargets(renderTargets, swapchain.Handle.GetDepthBufferDSV(), ResourceStateTransitionMode.Transition);
    }

    public void Teardown()
    {
        passthroughPipeline.Dispose();
        passthroughResources.Dispose();
    }
}
