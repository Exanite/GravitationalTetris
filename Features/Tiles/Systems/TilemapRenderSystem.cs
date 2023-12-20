using System;
using System.Numerics;
using Arch.System;
using Arch.System.SourceGenerator;
using Diligent;
using Exanite.Core.Utilities;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Time;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Rendering;
using Exanite.GravitationalTetris.Features.Resources;
using Exanite.GravitationalTetris.Features.Tetris.Components;
using Exanite.ResourceManagement;
using ValueType = Diligent.ValueType;

namespace Exanite.GravitationalTetris.Features.Tiles.Systems;

public partial class TilemapRenderSystem : EcsSystem, IRenderSystem, IInitializeSystem
{
    private readonly RendererContext rendererContext;
    private readonly GameTilemapData tilemap;
    private readonly ResourceManager resourceManager;
    private readonly SimulationTime time;
    private readonly RenderingResourcesSystem renderingResourcesSystem;

    private IResourceHandle<Texture2D> emptyTileTexture = null!;
    private IResourceHandle<Texture2D> placeholderTileTexture = null!;

    public TilemapRenderSystem(
        RendererContext rendererContext,
        GameTilemapData tilemap,
        ResourceManager resourceManager,
        SimulationTime time,
        RenderingResourcesSystem renderingResourcesSystem)
    {
        this.rendererContext = rendererContext;
        this.tilemap = tilemap;
        this.resourceManager = resourceManager;
        this.time = time;
        this.renderingResourcesSystem = renderingResourcesSystem;
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
        var deviceContext = rendererContext.DeviceContext;
        var shaderResourceBinding = renderingResourcesSystem.ShaderResourceBinding;
        var uniformBuffer = renderingResourcesSystem.UniformBuffer;
        var pipeline = renderingResourcesSystem.Pipeline;
        var mesh = renderingResourcesSystem.Mesh;

        for (var x = 0; x < tilemap.Tiles.GetLength(0); x++)
        {
            for (var y = 0; y < tilemap.Tiles.GetLength(1); y++)
            {
                ref var tile = ref tilemap.Tiles[x, y];
                var texture = (tile.Texture ?? emptyTileTexture).Value;

                shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture").Set(texture.View, SetShaderResourceFlags.AllowOverwrite);

                var world = Matrix4x4.CreateTranslation(x, y, -1);
                var view = cameraProjection.View;
                var projection = cameraProjection.Projection;

                var worldViewProjection = world * view * projection;

                var mapUniformBuffer = deviceContext.MapBuffer<Matrix4x4>(uniformBuffer, MapType.Write, MapFlags.Discard);
                mapUniformBuffer[0] = worldViewProjection;
                deviceContext.UnmapBuffer(uniformBuffer, MapType.Write);

                deviceContext.SetPipelineState(pipeline);
                deviceContext.SetVertexBuffers(0, new[] { mesh.VertexBuffer }, new[] { 0ul }, ResourceStateTransitionMode.Transition);
                deviceContext.SetIndexBuffer(mesh.IndexBuffer, 0, ResourceStateTransitionMode.Transition);
                deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
                deviceContext.DrawIndexed(new DrawIndexedAttribs
                {
                    IndexType = ValueType.UInt32,
                    NumIndices = 36,
                    Flags = DrawFlags.VerifyAll,
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
        var deviceContext = rendererContext.DeviceContext;
        var shaderResourceBinding = renderingResourcesSystem.ShaderResourceBinding;
        var uniformBuffer = renderingResourcesSystem.UniformBuffer;
        var pipeline = renderingResourcesSystem.Pipeline;
        var mesh = renderingResourcesSystem.Mesh;

        foreach (var blockPosition in tetrisRoot.PredictedBlockPositions)
        {
            var texture = placeholderTileTexture.Value;

            var maxAlpha = 0.8f;
            var minAlpha = 0.1f;
            var alpha = MathUtility.Remap(EaseInOutCubic(time.Time / 1.5f), 0, 1, minAlpha, maxAlpha);

            shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture").Set(texture.View, SetShaderResourceFlags.AllowOverwrite);

            var world = Matrix4x4.CreateTranslation(blockPosition.X, blockPosition.Y, -0.5f);
            var view = cameraProjection.View;
            var projection = cameraProjection.Projection;

            var worldViewProjection = world * view * projection;

            var mapUniformBuffer = deviceContext.MapBuffer<Matrix4x4>(uniformBuffer, MapType.Write, MapFlags.Discard);
            mapUniformBuffer[0] = worldViewProjection;
            deviceContext.UnmapBuffer(uniformBuffer, MapType.Write);

            deviceContext.SetPipelineState(pipeline);
            deviceContext.SetVertexBuffers(0, new[] { mesh.VertexBuffer }, new[] { 0ul }, ResourceStateTransitionMode.Transition);
            deviceContext.SetIndexBuffer(mesh.IndexBuffer, 0, ResourceStateTransitionMode.Transition);
            deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
            deviceContext.DrawIndexed(new DrawIndexedAttribs
            {
                IndexType = ValueType.UInt32,
                NumIndices = 36,
                Flags = DrawFlags.VerifyAll,
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
