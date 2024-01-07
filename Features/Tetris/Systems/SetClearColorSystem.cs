using System.Numerics;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering.Systems;

namespace Exanite.GravitationalTetris.Features.Tetris.Systems;

public class SetClearColorSystem : IStartSystem
{
    private readonly ClearMainRenderTargetSystem clearMainRenderTargetSystem;

    public SetClearColorSystem(ClearMainRenderTargetSystem clearMainRenderTargetSystem)
    {
        this.clearMainRenderTargetSystem = clearMainRenderTargetSystem;
    }

    public void Start()
    {
        clearMainRenderTargetSystem.ClearColor = Vector4.Zero;
    }
}
