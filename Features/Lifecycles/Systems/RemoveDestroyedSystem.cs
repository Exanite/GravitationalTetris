using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Ecs.Systems;
using Exanite.GravitationalTetris.Features.Lifecycles.Components;

namespace Exanite.GravitationalTetris.Features.Lifecycles.Systems;

public partial class RemoveDestroyedSystem : EcsSystem, ICleanupSystem
{
    public void Cleanup()
    {
        World.Destroy(RemoveDestroyed_QueryDescription);
    }

    [Query]
    [All<DestroyedComponent>]
    private void RemoveDestroyed(Entity entity) {}
}
