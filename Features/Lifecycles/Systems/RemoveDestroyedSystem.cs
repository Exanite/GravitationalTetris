using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.WarGames.Features.Lifecycles.Components;
using Exanite.WarGames.Systems;

namespace Exanite.WarGames.Features.Lifecycles.Systems;

public partial class RemoveDestroyedSystem : EcsSystem, IUpdateSystem
{
    public void Update()
    {
        UpdateQuery(World);
    }

    [Query]
    [All<DestroyedComponent>]
    private void Update(Entity entity)
    {
        World.Destroy(entity);
    }
}
