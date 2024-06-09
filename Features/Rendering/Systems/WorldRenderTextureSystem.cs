using System.Numerics;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Windowing;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class WorldRenderTextureSystem : ISetupSystem, IRenderSystem, ITeardownSystem
{
    public ColorRenderTexture2D WorldColor = null!;
    public DepthRenderTexture2D WorldDepth = null!;

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

        renderTargets[0] = WorldColor.RenderTarget;

        deviceContext.SetRenderTargets(renderTargets, WorldDepth.DepthStencil, ResourceStateTransitionMode.Transition);
        deviceContext.ClearRenderTarget(WorldColor.RenderTarget, Vector4.Zero, ResourceStateTransitionMode.Transition);
        deviceContext.ClearDepthStencil(WorldDepth.DepthStencil, ClearDepthStencilFlags.Depth | ClearDepthStencilFlags.Stencil, 1, 0, ResourceStateTransitionMode.Transition);
    }

    private void ResizeRenderTextures()
    {
        var size = window.SwapChain.GetDesc().GetSize();

        WorldColor.ResizeIfNeeded(size);
        WorldDepth.ResizeIfNeeded(size);
    }

    private void CreateRenderTextures()
    {
        var size = window.SwapChain.GetDesc().GetSize();

        WorldColor = new ColorRenderTexture2D(rendererContext, "World Color", size);
        WorldDepth = new DepthRenderTexture2D(rendererContext, "World Depth", size);
    }

    public void Teardown()
    {
        WorldColor.Dispose();
        WorldDepth.Dispose();
    }
}
