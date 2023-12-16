using System;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Core.Utilities;
using Exanite.Ecs.Systems;
using Exanite.ResourceManagement;
using Exanite.WarGames.Features.Cameras.Components;
using Exanite.WarGames.Features.Resources;
using Exanite.WarGames.Features.Sprites;
using Exanite.WarGames.Features.Tetris.Components;
using Exanite.WarGames.Features.Time;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.WarGames.Features.Tiles.Systems;

public partial class TilemapRenderSystem : EcsSystem, IRenderSystem, IInitializeSystem
{
    private readonly GameSpriteBatch gameSpriteBatch;
    private readonly GameTilemapData tilemap;
    private readonly ResourceManager resourceManager;
    private readonly GameTimeData time;

    private IResourceHandle<Texture2D> emptyTileTexture = null!;
    private IResourceHandle<Texture2D> placeholderTileTexture = null!;

    public TilemapRenderSystem(GameSpriteBatch gameSpriteBatch, GameTilemapData tilemap, ResourceManager resourceManager, GameTimeData time)
    {
        this.gameSpriteBatch = gameSpriteBatch;
        this.tilemap = tilemap;
        this.resourceManager = resourceManager;
        this.time = time;
    }

    public void Initialize()
    {
        emptyTileTexture = resourceManager.GetResource(BaseMod.TileNone);
        placeholderTileTexture = resourceManager.GetResource(BaseMod.TilePlaceholder);
    }

    public void Render()
    {
        DrawTilesQuery(World);
        DrawPlaceholdersQuery(World);
    }

    [Query]
    [All<CameraComponent>]
    private void DrawTiles(ref CameraProjectionComponent cameraProjection)
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

    [Query]
    private void DrawPlaceholders(ref TetrisRootComponent root)
    {
        DrawPlaceholders_1Query(World, ref root);
    }

    [Query]
    [All<CameraComponent>]
    private void DrawPlaceholders_1([Data] ref TetrisRootComponent tetrisRoot, ref CameraProjectionComponent cameraProjection)
    {
        var spriteBatch = gameSpriteBatch.SpriteBatch;

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.NonPremultiplied,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            cameraProjection.WorldToScreen
        );
        {
            foreach (var blockPosition in tetrisRoot.PredictedBlockPositions)
            {
                var sprite = placeholderTileTexture.Value;

                var maxAlpha = 0.8f;
                var minAlpha = 0.1f;
                var alpha = MathUtility.Remap(EaseInOutCubic(time.Time / 1.5f), 0, 1, minAlpha, maxAlpha);

                var sourceRect = new Rectangle(0, 0, sprite.Width, sprite.Height);
                var origin = new Vector2(0.5f * sprite.Width, 0.5f * sprite.Height);
                var scale = new Vector2(1f / sprite.Width, 1f / sprite.Height);
                var position = new Vector2(blockPosition.X, blockPosition.Y);

                spriteBatch.Draw(
                    sprite, // Texture
                    position, // Position
                    sourceRect, // Source Rect
                    new Color(1, 1, 1, alpha), // Color
                    0, // Rotation
                    origin, // Origin
                    scale, // Scale
                    SpriteEffects.None, // Effects
                    0); // Depth
            }
        }
        spriteBatch.End();
    }

    private float EaseInOutCubic(float t)
    {
        t = MathUtility.Wrap(t, 0, 2);
        if (t > 1)
        {
            t = 2 - t;
        }

        return (float)(t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2);
    }
}
