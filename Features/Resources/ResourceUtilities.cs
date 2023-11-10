using Exanite.Core.Properties;
using Exanite.ResourceManagement;

namespace Exanite.WarGames.Features.Resources;

public static class ResourceUtilities
{
    public static IResourceHandle<TResource> GetResource<TResource>(this ResourceManager resourceManager, PropertyDefinition<TResource> resourceDefinition)
    {
        return resourceManager.GetResource<TResource>(resourceDefinition.Key);
    }
}
