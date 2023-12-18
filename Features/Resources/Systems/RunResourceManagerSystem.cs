using Exanite.Ecs.Systems;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Resources.Systems;

public class RunResourceManagerSystem : IUpdateSystem
{
    private readonly ResourceManager resourceManager;

    public RunResourceManagerSystem(ResourceManager resourceManager)
    {
        this.resourceManager = resourceManager;
    }

    public void Update()
    {
        resourceManager.Update();
    }
}
