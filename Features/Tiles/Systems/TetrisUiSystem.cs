using Exanite.ResourceManagement;
using Exanite.WarGames.Systems;
using Microsoft.Xna.Framework;
using Myra;

namespace Exanite.WarGames.Features.Tiles.Systems;

public class TetrisUiSystem : EcsSystem, IStartSystem, IUpdateSystem, IDrawSystem
{
    private readonly Game game;
    private readonly ResourceManager resourceManager;

    public TetrisUiSystem(Game game, ResourceManager resourceManager)
    {
        this.game = game;
        this.resourceManager = resourceManager;
    }

    public void Start()
    {
        var assetManager = new AssetManager(resourceManager);

        MyraEnvironment.Game = game;
        MyraEnvironment.DefaultAssetManager = assetManager;
        DefaultAssets.AssetManager = assetManager;
    }

    public void Update()
    {

    }

    public void Draw()
    {
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
