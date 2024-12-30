using System.Numerics;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Windowing;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class RenderingResourcesSystem : EcsSystem, ISetupSystem, IRenderSystem, ITeardownSystem
{
    public Texture2D WorldColor = null!;
    public Texture2D WorldDepth = null!;

    private readonly RenderingContext renderingContext;
    private readonly Window window;

    public RenderingResourcesSystem(RenderingContext renderingContext, Window window)
    {
        this.renderingContext = renderingContext;
        this.window = window;
    }

    public void Setup()
    {
        WorldColor = new Texture2D(renderingContext, window.Size);
        WorldDepth = new Texture2D(renderingContext, window.Size);
    }

    public void Render()
    {
        WorldColor.ResizeIfNeeded(window.Size);
        WorldDepth.ResizeIfNeeded(window.Size);

        renderTargets[0] = WorldColor.RenderTarget;

        deviceContext.SetRenderTargets(renderTargets, WorldDepth.DepthStencil, ResourceStateTransitionMode.Transition);
        deviceContext.ClearRenderTarget(WorldColor.RenderTarget, Vector4.Zero, ResourceStateTransitionMode.Transition);
        deviceContext.ClearDepthStencil(WorldDepth.DepthStencil, ClearDepthStencilFlags.Depth, 1, 0, ResourceStateTransitionMode.Transition);
    }

    public void Teardown()
    {
        WorldColor.Dispose();
        WorldDepth.Dispose();
    }
}
