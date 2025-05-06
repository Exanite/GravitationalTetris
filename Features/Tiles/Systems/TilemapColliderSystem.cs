using System.Collections.Generic;
using System.Numerics;
using Exanite.Engine.Ecs.Queries;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Lifecycles.Components;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Tiles.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Myriad.Ecs;
using Myriad.Ecs.CommandBuffers;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.GravitationalTetris.Features.Tiles.Systems;

public partial class TilemapColliderSystem : GameSystem, IStartSystem, IUpdateSystem
{
    private EcsCommandBuffer commandBuffer = null!;

    private readonly GameTilemapData tilemap;

    public TilemapColliderSystem(GameTilemapData tilemap)
    {
        this.tilemap = tilemap;
    }

    public void Start()
    {
        commandBuffer = new EcsCommandBuffer(World);

        commandBuffer.Create().Set(new ComponentUpdateTilemapCollidersEvent());
        commandBuffer.Playback().Dispose();
    }

    public void Update()
    {
        if (World.Count(RemoveUpdateTilemapCollidersEventQueryDescription(World)) > 0)
        {
            RemoveUpdateTilemapCollidersEventQuery(World);
            RemoveTilemapCollidersQuery(World);

            UpdateTilemapColliders();

            commandBuffer.Playback().Dispose();
        }
    }

    [Query]
    [Include<ComponentUpdateTilemapCollidersEvent>]
    private void RemoveUpdateTilemapCollidersEvent(Entity entity)
    {
        commandBuffer.Set(entity, new ComponentDestroyed());
    }

    [Query]
    [Include<ComponentTilemapCollider>]
    private void RemoveTilemapColliders(Entity entity)
    {
        commandBuffer.Set(entity, new ComponentDestroyed());
    }

    private void UpdateTilemapColliders()
    {
        var polygons = new List<Vertices>();
        for (var x = 0; x < tilemap.Tiles.GetLength(0); x++)
        {
            for (var y = 0; y < tilemap.Tiles.GetLength(1); y++)
            {
                ref var tile = ref tilemap.Tiles[x, y];

                if (tile.IsWall)
                {
                    var vertices = new Vertices();

                    vertices.Add(new Vector2(0, 0));
                    vertices.Add(new Vector2(0, 1));
                    vertices.Add(new Vector2(1, 1));
                    vertices.Add(new Vector2(1, 0));

                    var transformationMatrix = Matrix4x4.CreateTranslation(x - 0.5f, y - 0.5f, 0);
                    vertices.Transform(ref transformationMatrix);

                    polygons.Add(vertices);
                }
            }
        }

        var body = new Body();
        body.BodyType = BodyType.Static;
        body.CreateCompoundPolygon(polygons, 1);

        commandBuffer.Create()
            .Set(new ComponentTransform())
            .Set(new ComponentRigidbody(body))
            .Set(new ComponentTilemapCollider());
    }
}
