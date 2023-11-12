using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.ResourceManagement;
using Exanite.WarGames.Features.Lifecycles.Components;
using Exanite.WarGames.Features.Physics.Components;
using Exanite.WarGames.Features.Players;
using Exanite.WarGames.Features.Players.Components;
using Exanite.WarGames.Features.Players.Systems;
using Exanite.WarGames.Features.Resources;
using Exanite.WarGames.Features.Sprites.Components;
using Exanite.WarGames.Features.Tiles.Components;
using Exanite.WarGames.Features.Time;
using Exanite.WarGames.Features.Transforms.Components;
using Exanite.WarGames.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.WarGames.Features.Tiles.Systems;

public enum Rotation
{
    R0 = 0,
    R90 = 1,
    R180 = 2,
    R270 = 3,
}

public record TetrisShapeDefinition
{
    public required bool[,] Shape;
    public required IResourceHandle<Texture2D> Texture;
    public required int PivotX;
    public required int PivotY;
}

public record struct Vector2Int(int X, int Y);

public struct TetrisBlockComponent
{
    public required EntityReference Root;

    public required TetrisShapeDefinition Definition;
    public required int LocalX;
    public required int LocalY;
}

public struct TetrisRootComponent
{
    public required TetrisShapeDefinition Definition;
    public required Rotation Rotation;

    public readonly List<Vector2Int> BlockPositions;
    public readonly List<Vector2Int> PredictedBlockPositions;

    public TetrisRootComponent()
    {
        BlockPositions = new List<Vector2Int>();
        PredictedBlockPositions = new List<Vector2Int>();
    }
}

public partial class TetrisSystem : EcsSystem, ICallbackSystem, IUpdateSystem
{
    private readonly float blockVerticalSpeed = 0.5f;
    private readonly float blockHorizontalSpeed = 2f;
    private EntityReference currentShapeRoot;

    private readonly List<TetrisShapeDefinition> shapes = new();

    private readonly ResourceManager resourceManager;
    private readonly Random random;
    private readonly GameTimeData time;
    private readonly GameInputData input;
    private readonly GameTilemapData tilemap;
    private readonly PlayerControllerSystem playerControllerSystem; // Todo Don't do this

    public TetrisSystem(ResourceManager resourceManager, Random random, GameTimeData time, GameInputData input, PlayerControllerSystem playerControllerSystem, GameTilemapData tilemap)
    {
        this.resourceManager = resourceManager;
        this.random = random;
        this.time = time;
        this.input = input;
        this.playerControllerSystem = playerControllerSystem;
        this.tilemap = tilemap;
    }

    public void RegisterCallbacks()
    {
        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new bool[,]
            {
                { true, true, true, true },
            },

            PivotX = 0,
            PivotY = 1,

            Texture = resourceManager.GetResource(BaseMod.TileCyan),
        });

        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new bool[,]
            {
                { true, false, false },
                { true, true, true },
            },

            PivotX = 1,
            PivotY = 1,

            Texture = resourceManager.GetResource(BaseMod.TileBlue),
        });

        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new bool[,]
            {
                { false, false, true },
                { true, true, true },
            },

            PivotX = 1,
            PivotY = 1,

            Texture = resourceManager.GetResource(BaseMod.TileOrange),
        });

        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new bool[,]
            {
                { true, true },
                { true, true },
            },

            PivotX = 0,
            PivotY = 0,

            Texture = resourceManager.GetResource(BaseMod.TileYellow),
        });

        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new bool[,]
            {
                { false, true, true },
                { true, true, false },
            },

            PivotX = 1,
            PivotY = 1,

            Texture = resourceManager.GetResource(BaseMod.TileGreen),
        });

        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new bool[,]
            {
                { false, true, false },
                { true, true, true },
            },

            PivotX = 1,
            PivotY = 1,

            Texture = resourceManager.GetResource(BaseMod.TilePurple),
        });

        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new bool[,]
            {
                { true, true, false },
                { false, true, true },
            },

            PivotX = 1,
            PivotY = 1,

            Texture = resourceManager.GetResource(BaseMod.TileRed),
        });
    }

    public void Update()
    {
        if (currentShapeRoot.IsAlive() && currentShapeRoot.Entity.Has<TetrisRootComponent>() && input.Current.Keyboard.IsKeyDown(Keys.Q) && !input.Previous.Keyboard.IsKeyDown(Keys.Q))
        {
            ref var tetrisRootComponent = ref currentShapeRoot.Entity.Get<TetrisRootComponent>();
            tetrisRootComponent.Rotation = (Rotation)(((int)tetrisRootComponent.Rotation + 1) % 4);
        }

        if (currentShapeRoot.IsAlive() && currentShapeRoot.Entity.Has<TetrisRootComponent>() && input.Current.Keyboard.IsKeyDown(Keys.E) && !input.Previous.Keyboard.IsKeyDown(Keys.E))
        {
            ref var tetrisRootComponent = ref currentShapeRoot.Entity.Get<TetrisRootComponent>();
            tetrisRootComponent.Rotation = (Rotation)(((int)tetrisRootComponent.Rotation + 1) % 4);
        }

        if (!currentShapeRoot.IsAlive() || (input.Current.Keyboard.IsKeyDown(Keys.Space) && !input.Previous.Keyboard.IsKeyDown(Keys.Space)))
        {
            PlaceBlocksQuery(World);

            var shape = shapes[random.Next(0, shapes.Count)];

            var currentShapeRootEntity = World.Create(
                new TetrisRootComponent
                {
                    Definition = shape,
                    Rotation = (Rotation)random.Next(0, 4),
                },
                new TransformComponent
                {
                    Position = new Vector2(5, 20),
                });

            currentShapeRoot = currentShapeRootEntity.Reference();

            for (var x = 0; x < shape.Shape.GetLength(0); x++)
            {
                for (var y = 0; y < shape.Shape.GetLength(1); y++)
                {
                    if (!shape.Shape[x, y])
                    {
                        continue;
                    }

                    var body = new Body();
                    body.BodyType = BodyType.Kinematic;
                    body.FixedRotation = true;

                    var fixture = body.CreateRectangle(1, 1, 1, Vector2.Zero);
                    fixture.Restitution = 0;

                    World.Create(
                        new TetrisBlockComponent
                        {
                            Root = currentShapeRoot,

                            Definition = shape,
                            LocalX = x - shape.PivotX,
                            LocalY = y - shape.PivotY,
                        },
                        new TransformComponent(),
                        new RigidbodyComponent(body),
                        new SpriteComponent
                        {
                            Resource = shape.Texture,
                        });
                }
            }
        }

        UpdateRootPositionsQuery(World);
        UpdateRootBlockPositionsQuery(World);

        UpdateBlockPositionsQuery(World);

        ResetIfPlayerOutOfBoundsQuery(World);
    }

    [Query]
    [All<PlayerComponent>]
    public void UpdateRootPositions(ref TransformComponent playerTransform)
    {
        UpdateRootPositions_1Query(World, ref playerTransform);
    }

    [Query]
    public void UpdateRootPositions_1([Data] ref TransformComponent playerTransform, ref TetrisRootComponent root, ref TransformComponent transform)
    {
        var minX = 0;
        var maxX = 9;

        switch (root.Rotation)
        {
            case Rotation.R0:
            {
                minX = minX + root.Definition.PivotX;
                maxX = maxX + root.Definition.PivotX - root.Definition.Shape.GetLength(0) + 1;

                break;
            }
            case Rotation.R90:
            {
                minX = minX - root.Definition.PivotY + root.Definition.Shape.GetLength(1) - 1;
                maxX = maxX - root.Definition.PivotY;

                break;
            }
            case Rotation.R180:
            {
                minX = minX - root.Definition.PivotX + root.Definition.Shape.GetLength(0) - 1;
                maxX = maxX - root.Definition.PivotX;

                break;
            }
            case Rotation.R270:
            {
                minX = minX + root.Definition.PivotY;
                maxX = maxX + root.Definition.PivotY - root.Definition.Shape.GetLength(1) + 1;

                break;
            }
        }

        var distanceToPlayerX = playerTransform.Position.X - transform.Position.X;
        var distanceToTravel = Math.Sign(distanceToPlayerX) * blockHorizontalSpeed * time.DeltaTime;
        distanceToTravel = Math.Clamp(distanceToTravel, -Math.Abs(distanceToPlayerX), Math.Abs(distanceToPlayerX));

        transform.Position.Y -= blockVerticalSpeed * time.DeltaTime;
        transform.Position.X += distanceToTravel;

        transform.Position.X = Math.Clamp(transform.Position.X, minX, maxX);
    }

    [Query]
    private void UpdateRootBlockPositions(ref TetrisRootComponent root, ref TransformComponent transform)
    {
        // Update logical world position of blocks
        var predictedX = (int)MathF.Round(transform.Position.X);
        var predictedY = (int)MathF.Ceiling(transform.Position.Y);

        root.BlockPositions.Clear();

        for (var x = 0; x < root.Definition.Shape.GetLength(0); x++)
        {
            for (var y = 0; y < root.Definition.Shape.GetLength(1); y++)
            {
                if (!root.Definition.Shape[x, y])
                {
                    continue;
                }

                var position = new Vector2Int(x - root.Definition.PivotX, y - root.Definition.PivotY);
                for (var i = 0; i < (int)root.Rotation; i++)
                {
                    position = new Vector2Int(-position.Y, position.X);
                }

                position.X += predictedX;
                position.Y += predictedY;

                root.BlockPositions.Add(position);
            }
        }

        // Update predicted logical world position of blocks
        root.PredictedBlockPositions.Clear();
        root.PredictedBlockPositions.AddRange(root.BlockPositions);

        while (true)
        {
            var hasFoundPredictedPosition = false;
            for (var i = 0; i < root.PredictedBlockPositions.Count; i++)
            {
                var (x, y) = root.PredictedBlockPositions[i];
                y--;

                if (y >= tilemap.Tiles.GetLength(1))
                {
                    continue;
                }

                if (y < 0 || tilemap.Tiles[x, y].IsWall)
                {
                    hasFoundPredictedPosition = true;

                    break;
                }
            }

            if (hasFoundPredictedPosition)
            {
                break;
            }

            for (var i = 0; i < root.PredictedBlockPositions.Count; i++)
            {
                root.PredictedBlockPositions[i] = new Vector2Int(root.PredictedBlockPositions[i].X, root.PredictedBlockPositions[i].Y - 1);
                predictedY--;
            }
        }

        root.PredictedBlockPositions.RemoveAll(position => position.Y >= tilemap.Tiles.GetLength(1));
    }

    [Query]
    public void UpdateBlockPositions(ref TetrisBlockComponent block, ref TransformComponent transform)
    {
        if (!block.Root.IsAlive())
        {
            return;
        }

        var rootEntity = block.Root.Entity;
        if (!rootEntity.Has<TetrisRootComponent>() || !rootEntity.Has<TransformComponent>())
        {
            return;
        }

        ref var root = ref rootEntity.Get<TetrisRootComponent>();
        ref var rootTransform = ref rootEntity.Get<TransformComponent>();

        var localPosition = new Vector2(block.LocalX, block.LocalY);
        transform.Position = Vector2.Transform(localPosition, Matrix.CreateRotationZ(float.Pi / 2 * (int)root.Rotation) * Matrix.CreateTranslation(rootTransform.Position.X, rootTransform.Position.Y, 0));
    }

    [Query]
    public void PlaceBlocks(Entity entity, ref TetrisRootComponent root)
    {
        foreach (var (x, y) in root.PredictedBlockPositions)
        {
            ref var tile = ref tilemap.Tiles[x, y];
            tile.IsWall = true;
            tile.Texture = root.Definition.Texture;
        }

        World.Create(new UpdateTilemapCollidersEventComponent());

        RemoveAllTetrisBlocksQuery(World);
    }

    [Query]
    [All<PlayerComponent>]
    public void ResetIfPlayerOutOfBounds(ref TransformComponent playerTransform, ref VelocityComponent velocity)
    {
        if (!(playerTransform.Position.Y < -1.5f) && !(playerTransform.Position.Y > 20.5f))
        {
            return;
        }

        ResetGameQuery(World);
    }

    [Query]
    [All<PlayerComponent>]
    public void ResetGame(ref TransformComponent playerTransform, ref VelocityComponent velocity)
    {
        playerTransform.Position = new Vector2(4f, 0);
        velocity.Velocity = Vector2.Zero;
        playerControllerSystem.SetIsGravityDown(true);

        RemoveAllTetrisBlocksQuery(World);
    }

    [Query]
    [Any<TetrisRootComponent, TetrisBlockComponent>]
    public void RemoveAllTetrisBlocks(Entity entity)
    {
        entity.Add(new DestroyedComponent());
    }
}
