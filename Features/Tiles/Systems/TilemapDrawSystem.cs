using Arch.System;
using Exanite.Extraction.Features.Cameras.Components;
using Exanite.Extraction.Features.Resources;
using Exanite.Extraction.Features.Sprites;
using Exanite.Extraction.Systems;
using Exanite.ResourceManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.Extraction.Features.Tiles.Systems;

public partial class TilemapDrawSystem : EcsSystem, IDrawSystem
{
    private readonly GameSpriteBatch gameSpriteBatch;
    private readonly GameTilemapData tilemap;
    private readonly ResourceManager resourceManager;

    public TilemapDrawSystem(GameSpriteBatch gameSpriteBatch, GameTilemapData tilemap, ResourceManager resourceManager)
    {
        this.gameSpriteBatch = gameSpriteBatch;
        this.tilemap = tilemap;
        this.resourceManager = resourceManager;
    }

    public void Draw()
    {
        DrawQuery(World);
    }

    [Query]
    private void Draw(ref CameraComponent camera, ref CameraProjectionComponent cameraProjection)
    {
        var sprite = resourceManager.GetResource(Base.Wall).Value;
        if (sprite == null)
        {
            return;
        }

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
                    if (tilemap.Tiles[x, y].IsWall)
                    {
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
        }
        spriteBatch.End();
    }
}
