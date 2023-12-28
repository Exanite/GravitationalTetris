using System;
using System.Numerics;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Core.Utilities;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Time;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Resources;
using Exanite.GravitationalTetris.Features.Sprites;
using Exanite.GravitationalTetris.Features.Sprites.Systems;
using Exanite.GravitationalTetris.Features.Tetris.Components;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Tiles.Systems;

public partial class TilemapRenderSystem : EcsSystem, IRenderSystem, ISetupSystem
{
    private readonly GameTilemapData tilemap;
    private readonly ResourceManager resourceManager;
    private readonly SimulationTime time;
    private readonly SpriteBatchSystem spriteBatchSystem;

    private IResourceHandle<Texture2D> emptyTileTexture = null!;
    private IResourceHandle<Texture2D> placeholderTileTexture = null!;

    public TilemapRenderSystem(
        GameTilemapData tilemap,
        ResourceManager resourceManager,
        SimulationTime time,
        SpriteBatchSystem spriteBatchSystem)
    {
        this.tilemap = tilemap;
        this.resourceManager = resourceManager;
        this.time = time;
        this.spriteBatchSystem = spriteBatchSystem;
    }

    public void Setup()
    {
        emptyTileTexture = resourceManager.GetResource(BaseMod.TileNone);
        placeholderTileTexture = resourceManager.GetResource(BaseMod.TilePlaceholder);
    }

    public void Render()
    {
        ForEachCameraQuery(World);
    }

    [Query]
    [All<CameraComponent>]
    private void ForEachCamera(ref CameraProjectionComponent cameraProjection)
    {
        spriteBatchSystem.Begin(new SpriteBeginDrawOptions
        {
            View = cameraProjection.View,
            Projection = cameraProjection.Projection,
        });
        {
            DrawTiles();
            DrawPlaceholdersQuery(World);
        }
        spriteBatchSystem.End();
    }

    private void DrawTiles()
    {
        for (var x = 0; x < tilemap.Tiles.GetLength(0); x++)
        {
            for (var y = 0; y < tilemap.Tiles.GetLength(1); y++)
            {
                ref var tile = ref tilemap.Tiles[x, y];

                var texture = emptyTileTexture;
                if (tile.Shape != null)
                {
                    texture = tile.Shape.DefaultTexture;

                    if (y + 1 == tilemap.Tiles.GetLength(1) || tilemap.Tiles[x, y + 1].Shape == null)
                    {
                        texture = tile.Shape.SnowTexture;
                    }
                }

                var world = Matrix4x4.CreateTranslation(x, y, 0);

                spriteBatchSystem.Draw(new SpriteDrawOptions
                {
                    Texture = texture.Value,
                    World = world,
                });
            }
        }
    }

    [Query]
    private void DrawPlaceholders(ref TetrisRootComponent tetrisRoot)
    {
        foreach (var blockPosition in tetrisRoot.PredictedBlockPositions)
        {
            var texture = placeholderTileTexture.Value;

            var maxAlpha = 0.8f;
            var minAlpha = 0.1f;
            var alpha = MathUtility.Remap(EaseInOutCubic(time.Time / 1.5f), 0, 1, minAlpha, maxAlpha);

            var world = Matrix4x4.CreateTranslation(blockPosition.X, blockPosition.Y, 0);

            spriteBatchSystem.Draw(new SpriteDrawOptions
            {
                Texture = texture,
                World = world,
                Color = new Vector4(1, 1, 1, alpha),
            });
        }
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
