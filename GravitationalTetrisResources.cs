using Exanite.Engine.Graphics;
using Exanite.ResourceManagement;
using Prowl.Scribe;

namespace Exanite.GravitationalTetris;

public static class GravitationalTetrisResources
{
    public static ResourceDefinition<FontFile> Font = new("/Exanite.GravitationalTetris/FieryTurk.ttf");

    public static ResourceDefinition<ShaderModule> SpriteVertexModule = new("/Exanite.GravitationalTetris/Sprite.vertex.slang");
    public static ResourceDefinition<ShaderModule> SpriteFragmentModule = new("/Exanite.GravitationalTetris/Sprite.fragment.slang");

    public static ResourceDefinition<Texture2D> White = new("/Exanite.GravitationalTetris/White.png");

    public static ResourceDefinition<Texture2D> Player = new("/Exanite.GravitationalTetris/Player.png");

    public static ResourceDefinition<Texture2D> TileNone = new("/Exanite.GravitationalTetris/TileNone.png");
    public static ResourceDefinition<Texture2D> TilePlaceholder = new("/Exanite.GravitationalTetris/TilePlaceholder.png");

    public static ResourceDefinition<Texture2D> TileBlue = new("/Exanite.GravitationalTetris/TileBlue.png");
    public static ResourceDefinition<Texture2D> TileCyan = new("/Exanite.GravitationalTetris/TileCyan.png");
    public static ResourceDefinition<Texture2D> TileGreen = new("/Exanite.GravitationalTetris/TileGreen.png");
    public static ResourceDefinition<Texture2D> TileOrange = new("/Exanite.GravitationalTetris/TileOrange.png");
    public static ResourceDefinition<Texture2D> TilePurple = new("/Exanite.GravitationalTetris/TilePurple.png");
    public static ResourceDefinition<Texture2D> TileRed = new("/Exanite.GravitationalTetris/TileRed.png");
    public static ResourceDefinition<Texture2D> TileYellow = new("/Exanite.GravitationalTetris/TileYellow.png");

    public static ResourceDefinition<ShaderModule> BloomDownFragmentModule = new("/Exanite.GravitationalTetris/Rendering/BloomDown.fragment.slang");
    public static ResourceDefinition<ShaderModule> BloomUpFragmentModule = new("/Exanite.GravitationalTetris/Rendering/BloomUp.fragment.slang");

    public static ResourceDefinition<ShaderModule> ToneMapFragmentModule = new("/Exanite.GravitationalTetris/Rendering/ToneMap.fragment.slang");

    public static class Winter
    {
        public static ResourceDefinition<Texture2D> Player = new("/Exanite.GravitationalTetris.Winter/Player.png");

        public static ResourceDefinition<Texture2D> TileBlue = new("/Exanite.GravitationalTetris.Winter/TileBlue.png");
        public static ResourceDefinition<Texture2D> TileCyan = new("/Exanite.GravitationalTetris.Winter/TileCyan.png");
        public static ResourceDefinition<Texture2D> TileGreen = new("/Exanite.GravitationalTetris.Winter/TileGreen.png");
        public static ResourceDefinition<Texture2D> TileOrange = new("/Exanite.GravitationalTetris.Winter/TileOrange.png");
        public static ResourceDefinition<Texture2D> TilePurple = new("/Exanite.GravitationalTetris.Winter/TilePurple.png");
        public static ResourceDefinition<Texture2D> TileRed = new("/Exanite.GravitationalTetris.Winter/TileRed.png");
        public static ResourceDefinition<Texture2D> TileYellow = new("/Exanite.GravitationalTetris.Winter/TileYellow.png");
    }
}
