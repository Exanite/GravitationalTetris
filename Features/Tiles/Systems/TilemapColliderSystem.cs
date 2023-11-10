using System.Collections.Generic;
using Exanite.Extraction.Features.Physics.Components;
using Exanite.Extraction.Features.Transforms.Components;
using Exanite.Extraction.Systems;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.Extraction.Features.Tiles.Systems;

public class TilemapColliderSystem : EcsSystem, IStartSystem
{
    private readonly GameTilemapData tilemap;

    public TilemapColliderSystem(GameTilemapData tilemap)
    {
        this.tilemap = tilemap;
    }

    public void Start()
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

                    var transformationMatrix = Matrix.CreateTranslation(x - 0.5f, y - 0.5f, 0);
                    vertices.Transform(ref transformationMatrix);

                    polygons.Add(vertices);
                }
            }
        }

        var body = new Body();
        body.BodyType = BodyType.Static;
        body.CreateCompoundPolygon(polygons, 1);

        World.Create(new TransformComponent(), new RigidbodyComponent(body));
    }
}
