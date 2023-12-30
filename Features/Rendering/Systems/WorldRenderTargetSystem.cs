using System.Numerics;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;

namespace Exanite.GravitationalTetris.Features.Lighting.Systems;

public class WorldRenderTargetSystem : ISetupSystem, IRenderSystem, ITeardownSystem
{
    private uint previousWidth;
    private uint previousHeight;

    public ITexture worldColor = null!;
    public ITextureView worldColorRenderTarget = null!;
    public ITextureView worldColorShaderResource = null!;

    public ITexture worldDepth = null!;
    public ITextureView worldDepthDepthStencil = null!;
    public ITextureView worldDepthShaderResource = null!;

    private readonly ITextureView[] renderTargets = new ITextureView[1];

    private readonly RendererContext rendererContext;

    public WorldRenderTargetSystem(RendererContext rendererContext)
    {
        this.rendererContext = rendererContext;
    }

    public void Setup()
    {
        CreateRenderTargets();
    }

    public void Render()
    {
        var deviceContext = rendererContext.DeviceContext;

        ResizeRenderTargets();

        renderTargets[0] = worldColorRenderTarget;

        deviceContext.SetRenderTargets(renderTargets, worldDepthDepthStencil, ResourceStateTransitionMode.Transition);
        deviceContext.ClearRenderTarget(worldColorRenderTarget, Vector4.Zero, ResourceStateTransitionMode.Transition);
        deviceContext.ClearDepthStencil(worldDepthDepthStencil, ClearDepthStencilFlags.Depth | ClearDepthStencilFlags.Stencil, 1, 0, ResourceStateTransitionMode.Transition);
    }

    private void ResizeRenderTargets()
    {
        var swapChain = rendererContext.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        if (previousWidth != swapChainDesc.Width || previousHeight != swapChain.GetDesc().Height)
        {
            worldColor.Dispose();
            worldDepth.Dispose();
            CreateRenderTargets();
        }
    }

    private void CreateRenderTargets()
    {
        var swapChain = rendererContext.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        worldColor = rendererContext.RenderDevice.CreateTexture(
            new TextureDesc
            {
                Name = "World Color Render Target",
                Type = ResourceDimension.Tex2d,
                Width = swapChainDesc.Width,
                Height = swapChainDesc.Height,
                Format = TextureFormat.RGBA32_Float,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                Usage = Usage.Default,
            });

        worldColorRenderTarget = worldColor.GetDefaultView(TextureViewType.RenderTarget);
        worldDepthShaderResource = worldColor.GetDefaultView(TextureViewType.ShaderResource);

        worldDepth = rendererContext.RenderDevice.CreateTexture(
            new TextureDesc
            {
                Name = "World Depth Render Target",
                Type = ResourceDimension.Tex2d,
                Width = swapChainDesc.Width,
                Height = swapChainDesc.Height,
                Format = TextureFormat.D32_Float,
                BindFlags = BindFlags.ShaderResource | BindFlags.DepthStencil,
                Usage = Usage.Default,
            });

        worldDepthDepthStencil = worldDepth.GetDefaultView(TextureViewType.DepthStencil);
        worldDepthShaderResource = worldDepth.GetDefaultView(TextureViewType.ShaderResource);

        previousWidth = swapChainDesc.Width;
        previousHeight = swapChainDesc.Height;
    }

    public void Teardown()
    {
        worldColor.Dispose();
        worldDepth.Dispose();
    }
}
