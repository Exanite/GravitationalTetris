using Exanite.Core.Properties;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.WarGames.Features.Resources;

public static class Base
{
    public static PropertyDefinition<Texture2D> White = new("Base:White.png");

    public static PropertyDefinition<Texture2D> Player = new("Base:Player.png");

    public static PropertyDefinition<Texture2D> Tile1 = new("Base:Tile_1.png");
    public static PropertyDefinition<Texture2D> Tile2 = new("Base:Tile_2.png");
    public static PropertyDefinition<Texture2D> Tile3 = new("Base:Tile_3.png");
    public static PropertyDefinition<Texture2D> Tile4 = new("Base:Tile_4.png");
    public static PropertyDefinition<Texture2D> Tile5 = new("Base:Tile_5.png");
}
