using System.Numerics;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering.Systems;

namespace Exanite.GravitationalTetris.Features.Tetris.Systems;

public class SetClearColorSystem : IStartSystem
{
    private readonly ClearRenderTargetRenderSystem clearRenderTargetRenderSystem;

    public SetClearColorSystem(ClearRenderTargetRenderSystem clearRenderTargetRenderSystem)
    {
        this.clearRenderTargetRenderSystem = clearRenderTargetRenderSystem;
    }

    public void Start()
    {
        clearRenderTargetRenderSystem.ClearColor = Vector4.Zero;
    }
}
