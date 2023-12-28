using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;

namespace Exanite.GravitationalTetris.Features.Lighting.Systems;

public class LightingSystem : ISetupSystem, IRenderSystem
{
    private uint previousWidth;
    private uint previousHeight;

    private ITexture renderTarget = null!;

    private readonly RendererContext rendererContext;

    public LightingSystem(RendererContext rendererContext)
    {
        this.rendererContext = rendererContext;
    }

    public void Setup()
    {
        CreateRenderTarget();
    }

    public void Render()
    {
        ResizeRenderTarget();
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
                Usage = Usage.Immutable,
            });

        previousWidth = swapChainDesc.Width;
        previousHeight = swapChainDesc.Height;
    }
}
