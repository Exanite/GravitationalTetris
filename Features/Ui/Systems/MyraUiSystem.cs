using Exanite.Ecs.Systems;
using Exanite.ResourceManagement;
using Myra;
using Myra.Platform;

namespace Exanite.GravitationalTetris.Features.Ui.Systems;

public class MyraUiSystem : IInitializeSystem
{
    private readonly ResourceManager resourceManager;
    private IMyraPlatform platform;

    public MyraUiSystem(ResourceManager resourceManager, IMyraPlatform platform)
    {
        this.resourceManager = resourceManager;
        this.platform = platform;
    }

    public void Initialize()
    {
        var assetManager = new AssetManager(resourceManager);

        MyraEnvironment.Platform = platform;
        MyraEnvironment.DefaultAssetManager = assetManager;
        DefaultAssets.AssetManager = assetManager;
    }

    private class AssetManager : IAssetManager
    {
        private readonly ResourceManager resourceManager;

        public AssetManager(ResourceManager resourceManager)
        {
            this.resourceManager = resourceManager;
        }

        public T GetAsset<T>(string assetKey)
        {
            return resourceManager.GetResource<T>(assetKey).Value;
        }

        public void Dispose()
        {
            // Do nothing
        }
    }
}
