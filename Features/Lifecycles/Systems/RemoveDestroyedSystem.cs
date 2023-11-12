using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.WarGames.Features.Lifecycles.Components;
using Exanite.WarGames.Systems;

namespace Exanite.WarGames.Features.Lifecycles.Systems;

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
