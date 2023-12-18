using Exanite.Core.Properties;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Resources;

public static class ResourceUtilities
{
    public static IResourceHandle<TResource> GetResource<TResource>(this ResourceManager resourceManager, PropertyDefinition<TResource> resourceDefinition)
    {
        return resourceManager.GetResource<TResource>(resourceDefinition.Key);
    }
}
