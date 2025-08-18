using Exanite.Core.Properties;
using Exanite.Engine.Graphics;
using Exanite.Engine.Windowing;

namespace Exanite.GravitationalTetris;

public static class GravitationalTetrisResources
{
    public static PropertyDefinition<WindowIcon> WindowIcon = new("/Exanite.GravitationalTetris/Icon.png");

    public static PropertyDefinition<ShaderModule> SpriteVertexModule = new("/Exanite.GravitationalTetris/Sprite.vertex.slang");
    public static PropertyDefinition<ShaderModule> SpriteFragmentModule = new("/Exanite.GravitationalTetris/Sprite.fragment.slang");

    public static PropertyDefinition<Texture2D> White = new("/Exanite.GravitationalTetris/White.png");

    public static PropertyDefinition<Texture2D> Player = new("/Exanite.GravitationalTetris/Player.png");

    public static PropertyDefinition<Texture2D> TileNone = new("/Exanite.GravitationalTetris/TileNone.png");
    public static PropertyDefinition<Texture2D> TilePlaceholder = new("/Exanite.GravitationalTetris/TilePlaceholder.png");

    public static PropertyDefinition<Texture2D> TileBlue = new("/Exanite.GravitationalTetris/TileBlue.png");
    public static PropertyDefinition<Texture2D> TileCyan = new("/Exanite.GravitationalTetris/TileCyan.png");
    public static PropertyDefinition<Texture2D> TileGreen = new("/Exanite.GravitationalTetris/TileGreen.png");
    public static PropertyDefinition<Texture2D> TileOrange = new("/Exanite.GravitationalTetris/TileOrange.png");
    public static PropertyDefinition<Texture2D> TilePurple = new("/Exanite.GravitationalTetris/TilePurple.png");
    public static PropertyDefinition<Texture2D> TileRed = new("/Exanite.GravitationalTetris/TileRed.png");
    public static PropertyDefinition<Texture2D> TileYellow = new("/Exanite.GravitationalTetris/TileYellow.png");

    public static PropertyDefinition<ShaderModule> BloomDownFragmentModule = new("/Exanite.GravitationalTetris/Rendering/BloomDown.fragment.slang");
    public static PropertyDefinition<ShaderModule> BloomUpFragmentModule = new("/Exanite.GravitationalTetris/Rendering/BloomUp.fragment.slang");

    public static PropertyDefinition<ShaderModule> ToneMapFragmentModule = new("/Exanite.GravitationalTetris/Rendering/ToneMap.fragment.slang");

    public static class Winter
    {
        public static PropertyDefinition<Texture2D> Player = new("/Exanite.GravitationalTetris.Winter/Player.png");

        public static PropertyDefinition<Texture2D> TileBlue = new("/Exanite.GravitationalTetris.Winter/TileBlue.png");
        public static PropertyDefinition<Texture2D> TileCyan = new("/Exanite.GravitationalTetris.Winter/TileCyan.png");
        public static PropertyDefinition<Texture2D> TileGreen = new("/Exanite.GravitationalTetris.Winter/TileGreen.png");
        public static PropertyDefinition<Texture2D> TileOrange = new("/Exanite.GravitationalTetris.Winter/TileOrange.png");
        public static PropertyDefinition<Texture2D> TilePurple = new("/Exanite.GravitationalTetris.Winter/TilePurple.png");
        public static PropertyDefinition<Texture2D> TileRed = new("/Exanite.GravitationalTetris.Winter/TileRed.png");
        public static PropertyDefinition<Texture2D> TileYellow = new("/Exanite.GravitationalTetris.Winter/TileYellow.png");
    }
}
