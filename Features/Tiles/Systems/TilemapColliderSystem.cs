using System.Collections.Generic;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Ecs.Systems;
using Exanite.Engine.Lifecycles.Components;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Tiles.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.GravitationalTetris.Features.Tiles.Systems;

public partial class TilemapColliderSystem : EcsSystem, IStartSystem, IUpdateSystem
{
    private readonly GameTilemapData tilemap;

    public TilemapColliderSystem(GameTilemapData tilemap)
    {
        this.tilemap = tilemap;
    }

    public void Start()
    {
        World.Create(new UpdateTilemapCollidersEventComponent());
    }

    public void Update()
    {
        if (World.CountEntities(RemoveUpdateTilemapCollidersEvent_QueryDescription) > 0)
        {
            RemoveUpdateTilemapCollidersEventQuery(World);
            RemoveTilemapCollidersQuery(World);

            UpdateTilemapColliders();
        }
    }

    [Query]
    [All<UpdateTilemapCollidersEventComponent>]
    private void RemoveUpdateTilemapCollidersEvent(Entity entity)
    {
        entity.Add(new DestroyedComponent());
    }

    [Query]
    [All<TilemapColliderComponent>]
    private void RemoveTilemapColliders(Entity entity)
    {
        entity.Add(new DestroyedComponent());
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

        World.Create(new TransformComponent(), new RigidbodyComponent(body), new TilemapColliderComponent());
    }
}
