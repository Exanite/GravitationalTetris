using System.Numerics;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.GravitationalTetris.Features.Sprites;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Lighting.Systems;

public class LightingSystem : ISetupSystem, IRenderSystem
{
    private uint previousWidth;
    private uint previousHeight;

    private ITexture renderTarget = null!;
    private ITextureView renderTargetRtView = null!;
    private ITextureView renderTargetSrView = null!;

    private IPipelineState pipeline = null!;

    private readonly RendererContext rendererContext;
    private readonly IResourceManager resourceManager;

    public LightingSystem(RendererContext rendererContext, IResourceManager resourceManager)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
    }

    public void Setup()
    {
        CreateRenderTarget();

        var renderDevice = rendererContext.RenderDevice;
        var swapChain = rendererContext.SwapChain;

        var vShader = resourceManager.GetResource<Shader>("Lighting:Light.v.hlsl");
        var pShader = resourceManager.GetResource<Shader>("Lighting:Light.p.hlsl");

        pipeline = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo
        {
            PSODesc = new PipelineStateDesc
            {
                Name = "Lighting Shader Pipeline",
            },

            GraphicsPipeline = new GraphicsPipelineDesc
            {
                InputLayout = SpriteInstanceData.Layout,
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,

                NumRenderTargets = 1,
                RTVFormats = new[] { swapChain.GetDesc().ColorBufferFormat },

                RasterizerDesc = new RasterizerStateDesc { CullMode = CullMode.None },
                DepthStencilDesc = new DepthStencilStateDesc { DepthEnable = false },
            },

            Vs = vShader.Value.Handle,
            Ps = pShader.Value.Handle,
        });
    }

    public void Render()
    {
        ResizeRenderTarget();

        var deviceContext = rendererContext.DeviceContext;

        deviceContext.SetRenderTargets(new ITextureView[] { renderTargetRtView }, null, ResourceStateTransitionMode.Transition);
        deviceContext.ClearRenderTarget(renderTargetRtView, Vector4.Zero, ResourceStateTransitionMode.Transition);

        deviceContext.SetPipelineState(pipeline);
        deviceContext.Draw(new DrawAttribs
        {
            NumVertices = 4,
            Flags = DrawFlags.VerifyAll,
        });
    }

    private void ResizeRenderTarget()
    {
        var swapChain = rendererContext.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        if (previousWidth != swapChainDesc.Width || previousHeight != swapChain.GetDesc().Height)
        {
            renderTarget.Dispose();
            CreateRenderTarget();
        }
    }

    private void CreateRenderTarget()
    {
        var swapChain = rendererContext.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        renderTarget = rendererContext.RenderDevice.CreateTexture(
            new TextureDesc
            {
                Name = "Lighting Render Target",
                Type = ResourceDimension.Tex2d,
                Width = swapChainDesc.Width,
                Height = swapChainDesc.Height,
                Format = swapChain.GetCurrentBackBufferRTV().GetDesc().Format,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                Usage = Usage.Default,
            });

        renderTargetRtView = renderTarget.GetDefaultView(TextureViewType.RenderTarget);
        renderTargetSrView = renderTarget.GetDefaultView(TextureViewType.ShaderResource);

        previousWidth = swapChainDesc.Width;
        previousHeight = swapChainDesc.Height;
    }
}
