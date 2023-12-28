using System.Numerics;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;

namespace Exanite.GravitationalTetris.Features.Lighting.Systems;

public class WorldRenderTargetSystem : ISetupSystem, IRenderSystem
{
    private uint previousWidth;
    private uint previousHeight;

    private ITexture renderTarget = null!;
    private ITextureView renderTargetRtView = null!;
    private ITextureView renderTargetSrView = null!;

    private readonly RendererContext rendererContext;

    public WorldRenderTargetSystem(RendererContext rendererContext)
    {
        this.rendererContext = rendererContext;
    }

    public void Setup()
    {
        CreateRenderTarget();
    }

    public void Render()
    {
        var deviceContext = rendererContext.DeviceContext;

        ResizeRenderTarget();

        deviceContext.SetRenderTargets(new ITextureView[] { renderTargetRtView }, null, ResourceStateTransitionMode.Transition);
        deviceContext.ClearRenderTarget(renderTargetRtView, Vector4.Zero, ResourceStateTransitionMode.Transition);
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
