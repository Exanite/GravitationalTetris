using System;
using System.Collections.Generic;
using System.Linq;
using Exanite.Core.Properties;
using Exanite.ResourceManagement;
using Exanite.WarGames.Features.Resources;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.WarGames.Features.Tiles;

public class GameTilemapData
{
    public readonly Tile[,] Tiles;

    private readonly ResourceManager resourceManager;

    private readonly List<IResourceHandle<Texture2D>> possibleTileTextures = new();

    public GameTilemapData(ResourceManager resourceManager)
    {
        this.resourceManager = resourceManager;

        Tiles = new Tile[10, 20];
    }

    public void Load()
    {
        possibleTileTextures.Clear();
        possibleTileTextures.AddRange(new List<PropertyDefinition<Texture2D>>
        {
            Base.TileBlue,
            Base.TileCyan,
            Base.TileGreen,
            Base.TileOrange,
            Base.TilePurple,
            Base.TileRed,
            Base.TileYellow,
        }.Select(def => resourceManager.GetResource(def)));

        // var random = new Random();
        // for (var x = 0; x < Tiles.GetLength(0); x++)
        // {
        //     for (var y = 0; y < Tiles.GetLength(1); y++)
        //     {
        //         ref var tile = ref Tiles[x, y];
        //
        //         tile.IsWall = random.NextSingle() > 0.5f;
        //         if (tile.IsWall)
        //         {
        //             tile.Texture = possibleTileTextures[random.Next(0, possibleTileTextures.Count)];
        //         }
        //     }
        // }
    }
}
