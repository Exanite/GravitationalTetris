using System.Numerics;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class WorldRenderTextureSystem : ISetupSystem, IRenderSystem, ITeardownSystem
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

    public WorldRenderTextureSystem(RendererContext rendererContext)
    {
        this.rendererContext = rendererContext;
    }

    public void Setup()
    {
        CreateRenderTextures();
    }

    public void Render()
    {
        var deviceContext = rendererContext.DeviceContext;

        ResizeRenderTextures();

        renderTargets[0] = worldColorRenderTarget;

        deviceContext.SetRenderTargets(renderTargets, worldDepthDepthStencil, ResourceStateTransitionMode.Transition);
        deviceContext.ClearRenderTarget(worldColorRenderTarget, Vector4.Zero, ResourceStateTransitionMode.Transition);
        deviceContext.ClearDepthStencil(worldDepthDepthStencil, ClearDepthStencilFlags.Depth | ClearDepthStencilFlags.Stencil, 1, 0, ResourceStateTransitionMode.Transition);
    }

    private void ResizeRenderTextures()
    {
        var swapChain = rendererContext.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        if (previousWidth != swapChainDesc.Width || previousHeight != swapChain.GetDesc().Height)
        {
            worldColor.Dispose();
            worldDepth.Dispose();
            CreateRenderTextures();
        }
    }

    private void CreateRenderTextures()
    {
        var renderDevice = rendererContext.RenderDevice;
        var swapChain = rendererContext.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        worldColor = renderDevice.CreateTexture(
            new TextureDesc
            {
                Name = "World Color Render Texture",
                Type = ResourceDimension.Tex2d,
                Width = swapChainDesc.Width,
                Height = swapChainDesc.Height,
                Format = TextureFormat.RGBA32_Float,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                Usage = Usage.Default,
            });

        worldColorRenderTarget = worldColor.GetDefaultView(TextureViewType.RenderTarget);
        worldDepthShaderResource = worldColor.GetDefaultView(TextureViewType.ShaderResource);

        worldDepth = renderDevice.CreateTexture(
            new TextureDesc
            {
                Name = "World Depth Render Texture",
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
