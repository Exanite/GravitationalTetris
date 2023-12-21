using System;
using System.Numerics;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Core.Utilities;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Time;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Rendering;
using Exanite.GravitationalTetris.Features.Resources;
using Exanite.GravitationalTetris.Features.Sprites.Systems;
using Exanite.GravitationalTetris.Features.Tetris.Components;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Tiles.Systems;

public partial class TilemapRenderSystem : EcsSystem, IRenderSystem, IInitializeSystem
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
        for (var x = 0; x < tilemap.Tiles.GetLength(0); x++)
        {
            for (var y = 0; y < tilemap.Tiles.GetLength(1); y++)
            {
                ref var tile = ref tilemap.Tiles[x, y];
                var texture = (tile.Texture ?? emptyTileTexture).Value;

                var world = Matrix4x4.CreateTranslation(x, y, 0);
                var view = cameraProjection.View;
                var projection = cameraProjection.Projection;

                spriteBatchSystem.DrawSprite(texture, new SpriteUniformData
                {
                    World = world,
                    View = view,
                    Projection = projection,
                });
            }
        }
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
        foreach (var blockPosition in tetrisRoot.PredictedBlockPositions)
        {
            var texture = placeholderTileTexture.Value;

            var maxAlpha = 0.8f;
            var minAlpha = 0.1f;
            var alpha = MathUtility.Remap(EaseInOutCubic(time.Time / 1.5f), 0, 1, minAlpha, maxAlpha);

            var world = Matrix4x4.CreateTranslation(blockPosition.X, blockPosition.Y, 0);
            var view = cameraProjection.View;
            var projection = cameraProjection.Projection;

            spriteBatchSystem.DrawSprite(texture, new SpriteUniformData
            {
                World = world,
                View = view,
                Projection = projection,
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
