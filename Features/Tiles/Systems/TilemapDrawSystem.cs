using System;
using System.Collections.Generic;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Core.Utilities;
using Exanite.ResourceManagement;
using Exanite.WarGames.Features.Cameras.Components;
using Exanite.WarGames.Features.Resources;
using Exanite.WarGames.Features.Sprites;
using Exanite.WarGames.Features.Time;
using Exanite.WarGames.Features.Transforms.Components;
using Exanite.WarGames.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.WarGames.Features.Tiles.Systems;

public partial class TilemapDrawSystem : EcsSystem, IDrawSystem, ICallbackSystem
{
    private readonly GameSpriteBatch gameSpriteBatch;
    private readonly GameTilemapData tilemap;
    private readonly ResourceManager resourceManager;
    private readonly GameTimeData time;

    private IResourceHandle<Texture2D> emptyTileTexture = null!;
    private IResourceHandle<Texture2D> placeholderTileTexture = null!;

    public TilemapDrawSystem(GameSpriteBatch gameSpriteBatch, GameTilemapData tilemap, ResourceManager resourceManager, GameTimeData time)
    {
        this.gameSpriteBatch = gameSpriteBatch;
        this.tilemap = tilemap;
        this.resourceManager = resourceManager;
        this.time = time;
    }

    public void RegisterCallbacks()
    {
        emptyTileTexture = resourceManager.GetResource(BaseMod.TileNone);
        placeholderTileTexture = resourceManager.GetResource(BaseMod.TilePlaceholder);
    }

    public void Draw()
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
    private void DrawPlaceholders(ref TetrisRootComponent root, ref TransformComponent transform)
    {
        DrawPlaceholders_1Query(World, ref root, ref transform);
    }

    public record struct Position(int X, int Y);

    private readonly List<Position> blockPositions = new();

    [Query]
    [All<CameraComponent>]
    private void DrawPlaceholders_1([Data] ref TetrisRootComponent tetrisRoot, [Data] ref TransformComponent tetrisTransform, ref CameraProjectionComponent cameraProjection)
    {
        var spriteBatch = gameSpriteBatch.SpriteBatch;

        var predictedX = (int)MathF.Round(tetrisTransform.Position.X);
        var predictedY = (int)MathF.Ceiling(tetrisTransform.Position.Y);

        blockPositions.Clear();
        for (var x = 0; x < tetrisRoot.Definition.Shape.GetLength(0); x++)
        {
            for (var y = 0; y < tetrisRoot.Definition.Shape.GetLength(1); y++)
            {
                if (!tetrisRoot.Definition.Shape[x, y])
                {
                    continue;
                }

                var position = new Position(x - tetrisRoot.Definition.PivotX, y - tetrisRoot.Definition.PivotY);
                for (var i = 0; i < (int)tetrisRoot.Rotation; i++)
                {
                    position = new Position(-position.Y, position.X);
                }

                position.X += predictedX;
                position.Y += predictedY;

                blockPositions.Add(position);
            }
        }

        // Calculate predictedY
        while (true)
        {
            // tetrisRoot.Definition

            break;
        }

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
            foreach (var blockPosition in blockPositions)
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
