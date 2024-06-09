using System.Numerics;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Windowing;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class WorldRenderTextureSystem : ISetupSystem, IRenderSystem, ITeardownSystem
{
    public ITexture WorldColor = null!;
    public ITextureView WorldColorView = null!;

    public ITexture WorldDepth = null!;
    public ITextureView WorldDepthView = null!;

    private uint previousWidth;
    private uint previousHeight;

    private readonly ITextureView[] renderTargets = new ITextureView[1];

    private readonly RendererContext rendererContext;
    private readonly Window window;

    public WorldRenderTextureSystem(RendererContext rendererContext, Window window)
    {
        this.rendererContext = rendererContext;
        this.window = window;
    }

    public void Setup()
    {
        CreateRenderTextures();
    }

    public void Render()
    {
        var deviceContext = rendererContext.DeviceContext;

        ResizeRenderTextures();

        renderTargets[0] = WorldColorView;

        deviceContext.SetRenderTargets(renderTargets, WorldDepthView, ResourceStateTransitionMode.Transition);
        deviceContext.ClearRenderTarget(WorldColorView, Vector4.Zero, ResourceStateTransitionMode.Transition);
        deviceContext.ClearDepthStencil(WorldDepthView, ClearDepthStencilFlags.Depth | ClearDepthStencilFlags.Stencil, 1, 0, ResourceStateTransitionMode.Transition);
    }

    private void ResizeRenderTextures()
    {
        var swapChain = window.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        if (previousWidth != swapChainDesc.Width || previousHeight != swapChainDesc.Height)
        {
            WorldColor.Dispose();
            WorldDepth.Dispose();
            CreateRenderTextures();
        }
    }

    private void CreateRenderTextures()
    {
        var renderDevice = rendererContext.RenderDevice;
        var swapChain = window.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        WorldColor = renderDevice.CreateTexture(
            new TextureDesc
            {
                Name = "World Color Render Texture",
                Type = ResourceDimension.Tex2d,
                Width = swapChainDesc.Width,
                Height = swapChainDesc.Height,
                Format = CommonTextureFormats.HdrTextureFormat,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                Usage = Usage.Default,
            });

        WorldColorView = WorldColor.GetDefaultView(TextureViewType.RenderTarget);

        WorldDepth = renderDevice.CreateTexture(
            new TextureDesc
            {
                Name = "World Depth Render Texture",
                Type = ResourceDimension.Tex2d,
                Width = swapChainDesc.Width,
                Height = swapChainDesc.Height,
                Format = CommonTextureFormats.DepthTextureFormat,
                BindFlags = BindFlags.ShaderResource | BindFlags.DepthStencil,
                Usage = Usage.Default,
            });

        WorldDepthView = WorldDepth.GetDefaultView(TextureViewType.DepthStencil);

        previousWidth = swapChainDesc.Width;
        previousHeight = swapChainDesc.Height;
    }

    public void Teardown()
    {
        WorldColor.Dispose();
        WorldDepth.Dispose();
    }
}
