using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.ResourceManagement;
using Exanite.WarGames.Features.Physics.Components;
using Exanite.WarGames.Features.Players;
using Exanite.WarGames.Features.Players.Components;
using Exanite.WarGames.Features.Resources;
using Exanite.WarGames.Features.Sprites.Components;
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
}

public partial class TetrisSystem : EcsSystem, ICallbackSystem, IUpdateSystem
{
    private float blockVerticalSpeed = 0.5f;
    private float blockHorizontalSpeed = 2f;
    private EntityReference currentShapeRoot;

    private readonly List<TetrisShapeDefinition> shapes = new();

    private readonly ResourceManager resourceManager;
    private readonly Random random;
    private readonly GameTimeData time;
    private readonly GameInputData input;

    public TetrisSystem(ResourceManager resourceManager, Random random, GameTimeData time, GameInputData input)
    {
        this.resourceManager = resourceManager;
        this.random = random;
        this.time = time;
        this.input = input;
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

            Texture = resourceManager.GetResource(Base.TileCyan),
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

            Texture = resourceManager.GetResource(Base.TileBlue),
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

            Texture = resourceManager.GetResource(Base.TileOrange),
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

            Texture = resourceManager.GetResource(Base.TileYellow),
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

            Texture = resourceManager.GetResource(Base.TileGreen),
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

            Texture = resourceManager.GetResource(Base.TilePurple),
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

            Texture = resourceManager.GetResource(Base.TileRed),
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
            var shape = shapes[random.Next(0, shapes.Count)];

            var currentShapeRootEntity = World.Create(
                new TetrisRootComponent()
                {
                    Definition = shape,
                    Rotation = (Rotation)random.Next(0, 4),
                },
                new TransformComponent()
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
        UpdateBlockPositionsQuery(World);
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
        var distanceToPlayerX = playerTransform.Position.X - transform.Position.X;
        var distanceToTravel = Math.Sign(distanceToPlayerX) * blockHorizontalSpeed * time.DeltaTime;
        distanceToTravel = Math.Clamp(distanceToTravel, -Math.Abs(distanceToPlayerX), Math.Abs(distanceToPlayerX));

        transform.Position.Y -= blockVerticalSpeed * time.DeltaTime;
        transform.Position.X += distanceToTravel;
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
}
