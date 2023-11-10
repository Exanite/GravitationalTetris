using System.IO;
using System.Linq;
using Exanite.WarGames.Features.Json;
using Exanite.WarGames.Features.Tiles.Tiled.Models;
using Newtonsoft.Json;

namespace Exanite.WarGames.Features.Tiles;

public class GameTilemapData
{
    public readonly Tile[,] Tiles;

    private readonly ProjectJsonSerializer serializer;

    public GameTilemapData(ProjectJsonSerializer serializer)
    {
        this.serializer = serializer;

        var map = LoadMap();
        var layer = map.Layers.First();
        Tiles = new Tile[layer.Width, layer.Height];

        foreach (var chunk in layer.Chunks)
        {
            for (var x = 0; x < chunk.Width; x++)
            {
                for (var y = 0; y < chunk.Height; y++)
                {
                    Tiles[chunk.X - layer.StartX + x, chunk.Y - layer.StartY + y] = new Tile()
                    {
                        IsWall = chunk.Data[y * chunk.Width + x] != 0,
                    };
                }
            }
        }
    }

    private TiledMap LoadMap()
    {
        using (var fileStream = File.Open(Path.Join("Content", "Map_0.tmj"), FileMode.Open))
        using (var streamReader = new StreamReader(fileStream))
        using (var jsonReader = new JsonTextReader(streamReader))
        {
            return serializer.Deserialize<TiledMap>(jsonReader)!;
        }
    }

    private TiledTileset LoadTileset()
    {
        using (var fileStream = File.Open(Path.Join("Content", "Map_Tileset"), FileMode.Open))
        using (var streamReader = new StreamReader(fileStream))
        using (var jsonReader = new JsonTextReader(streamReader))
        {
            return serializer.Deserialize<TiledTileset>(jsonReader)!;
        }
    }
}
