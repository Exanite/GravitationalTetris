using Exanite.ResourceManagement;
using Exanite.WarGames.Systems;

namespace Exanite.WarGames.Features.Resources.Systems;

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
