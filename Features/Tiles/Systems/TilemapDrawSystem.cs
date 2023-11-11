using Arch.System;
using Exanite.ResourceManagement;
using Exanite.WarGames.Features.Cameras.Components;
using Exanite.WarGames.Features.Resources;
using Exanite.WarGames.Features.Sprites;
using Exanite.WarGames.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.WarGames.Features.Tiles.Systems;

public partial class TilemapDrawSystem : EcsSystem, IDrawSystem, ICallbackSystem
{
    private readonly GameSpriteBatch gameSpriteBatch;
    private readonly GameTilemapData tilemap;
    private readonly ResourceManager resourceManager;

    private IResourceHandle<Texture2D> emptyTileTexture;

    public TilemapDrawSystem(GameSpriteBatch gameSpriteBatch, GameTilemapData tilemap, ResourceManager resourceManager)
    {
        this.gameSpriteBatch = gameSpriteBatch;
        this.tilemap = tilemap;
        this.resourceManager = resourceManager;
    }

    public void RegisterCallbacks()
    {
        emptyTileTexture = resourceManager.GetResource(Base.TileNone);
    }

    public void Draw()
    {
        DrawQuery(World);
    }

    [Query]
    private void Draw(ref CameraComponent camera, ref CameraProjectionComponent cameraProjection)
    {
        var spriteBatch = gameSpriteBatch.SpriteBatch;

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            cameraProjection.WorldToScreen
        );
        {
            for (var x = 0; x < tilemap.Tiles.GetLength(0); x++)
            {
                for (var y = 0; y < tilemap.Tiles.GetLength(1); y++)
                {
                    ref var tile = ref tilemap.Tiles[x, y];
                    var sprite = (tile.Texture ?? emptyTileTexture).Value;

                    var sourceRect = new Rectangle(0, 0, sprite.Width, sprite.Height);
                    var origin = new Vector2(0.5f * sprite.Width, 0.5f * sprite.Height);
                    var scale = new Vector2(1f / sprite.Width, 1f / sprite.Height);

                    spriteBatch.Draw(
                        sprite, // Texture
                        new Vector2(x, y), // Position
                        sourceRect, // Source Rect
                        Color.White, // Color
                        0, // Rotation
                        origin, // Origin
                        scale, // Scale
                        SpriteEffects.None, // Effects
                        0); // Depth
                }
            }
        }
        spriteBatch.End();
    }
}
