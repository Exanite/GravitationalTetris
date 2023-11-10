using Exanite.Extraction.Systems;
using Exanite.ResourceManagement;

namespace Exanite.Extraction.Features.Resources.Systems;

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
