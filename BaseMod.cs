using Exanite.Core.Properties;
using Exanite.Engine.OldRendering;
using Exanite.Engine.Windowing;

namespace Exanite.GravitationalTetris;

public static class BaseMod
{
    public static PropertyDefinition<WindowIcon> WindowIcon = new("/Base/Icon.png");

    public static PropertyDefinition<Shader> SpriteVShader = new("/Base/Sprite.v.hlsl");
    public static PropertyDefinition<Shader> SpritePShader = new("/Base/Sprite.p.hlsl");

    public static PropertyDefinition<Texture2D> White = new("/Base/White.png");

    public static PropertyDefinition<Texture2D> Player = new("/Base/Player.png");

    public static PropertyDefinition<Texture2D> TileNone = new("/Base/TileNone.png");
    public static PropertyDefinition<Texture2D> TilePlaceholder = new("/Base/TilePlaceholder.png");

    public static PropertyDefinition<Texture2D> TileBlue = new("/Base/TileBlue.png");
    public static PropertyDefinition<Texture2D> TileCyan = new("/Base/TileCyan.png");
    public static PropertyDefinition<Texture2D> TileGreen = new("/Base/TileGreen.png");
    public static PropertyDefinition<Texture2D> TileOrange = new("/Base/TileOrange.png");
    public static PropertyDefinition<Texture2D> TilePurple = new("/Base/TilePurple.png");
    public static PropertyDefinition<Texture2D> TileRed = new("/Base/TileRed.png");
    public static PropertyDefinition<Texture2D> TileYellow = new("/Base/TileYellow.png");
}
