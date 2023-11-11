using Exanite.ResourceManagement;

namespace Exanite.WarGames.Features.Tiles;

public class GameTilemapData
{
    public readonly Tile[,] Tiles;

    private readonly ResourceManager resourceManager;

    public GameTilemapData(ResourceManager resourceManager)
    {
        this.resourceManager = resourceManager;

        Tiles = new Tile[10, 20];
    }
}
