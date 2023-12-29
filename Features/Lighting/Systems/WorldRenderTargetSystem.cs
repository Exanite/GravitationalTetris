using System.Numerics;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;

namespace Exanite.GravitationalTetris.Features.Lighting.Systems;

public class WorldRenderTargetSystem : ISetupSystem, IRenderSystem
{
    private uint previousWidth;
    private uint previousHeight;

    public ITexture worldColor = null!;
    public ITextureView worldColorRenderTarget = null!;
    public ITextureView worldColorShaderResource = null!;

    public ITexture worldDepth = null!;
    public ITextureView worldDepthRenderTarget = null!;
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

        deviceContext.SetRenderTargets(renderTargets, worldDepthRenderTarget, ResourceStateTransitionMode.Transition);
        deviceContext.ClearRenderTarget(worldColorRenderTarget, Vector4.Zero, ResourceStateTransitionMode.Transition);
        deviceContext.ClearDepthStencil(worldDepthRenderTarget, ClearDepthStencilFlags.Depth | ClearDepthStencilFlags.Stencil, 1, 0, ResourceStateTransitionMode.Transition);
    }

    private void ResizeRenderTargets()
    {
        var swapChain = rendererContext.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        if (previousWidth != swapChainDesc.Width || previousHeight != swapChain.GetDesc().Height)
        {
            worldColor.Dispose();
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
                Format = swapChain.GetCurrentBackBufferRTV().GetDesc().Format,
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
                Format = swapChain.GetDepthBufferDSV().GetDesc().Format,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                Usage = Usage.Default,
            });

        worldDepthRenderTarget = worldDepth.GetDefaultView(TextureViewType.RenderTarget);
        worldDepthShaderResource = worldDepth.GetDefaultView(TextureViewType.ShaderResource);

        previousWidth = swapChainDesc.Width;
        previousHeight = swapChainDesc.Height;
    }
}
