using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Extraction.Features.Lifecycles.Components;
using Exanite.Extraction.Systems;

namespace Exanite.Extraction.Features.Lifecycles.Systems;

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
