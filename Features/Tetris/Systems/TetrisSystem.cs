using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Ecs.Systems;
using Exanite.Engine.Inputs;
using Exanite.Engine.Rendering;
using Exanite.Engine.Time;
using Exanite.GravitationalTetris.Features.Lifecycles.Components;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Players;
using Exanite.GravitationalTetris.Features.Players.Components;
using Exanite.GravitationalTetris.Features.Players.Systems;
using Exanite.GravitationalTetris.Features.Resources;
using Exanite.GravitationalTetris.Features.Sprites.Components;
using Exanite.GravitationalTetris.Features.Tetris.Components;
using Exanite.GravitationalTetris.Features.Tiles;
using Exanite.GravitationalTetris.Features.Tiles.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Exanite.ResourceManagement;
using nkast.Aether.Physics2D.Dynamics;
using SDL2;

namespace Exanite.GravitationalTetris.Features.Tetris.Systems;

public partial class TetrisSystem : EcsSystem, IInitializeSystem, IUpdateSystem
{
    public static readonly string ScoresFilePath = Path.Join(GameDirectories.PersistentDataDirectory, "Scores.txt");

    public float SpeedMultiplier = 1;
    public float ScoreMultiplier => SpeedMultiplier * 2;

    public float DifficultyIncreaseCooldown = 15;
    public float DifficultyIncreaseTimer = 0;
    public float DifficultyIncreaseSpeedIncrement = 0.5f;

    public float Score;
    public float PreviousScore;
    public List<float> HighScores = new();

    public float ScorePerSecond = 20f;
    public float ScorePerTile = 100f;

    private readonly float blockVerticalSpeed = 0.5f;
    private readonly float blockHorizontalSpeed = 2f;
    private EntityReference currentShapeRoot;

    private readonly List<TetrisShapeDefinition> shapes = new();

    private readonly ResourceManager resourceManager;
    private readonly Random random;
    private readonly SimulationTime time;
    private readonly Input input;
    private readonly GameTilemapData tilemap;
    private readonly PlayerControllerSystem playerControllerSystem;

    public TetrisSystem(ResourceManager resourceManager, Random random, SimulationTime time, Input input, PlayerControllerSystem playerControllerSystem, GameTilemapData tilemap)
    {
        this.resourceManager = resourceManager;
        this.random = random;
        this.time = time;
        this.input = input;
        this.playerControllerSystem = playerControllerSystem;
        this.tilemap = tilemap;
    }

    public void Initialize()
    {
        if (File.Exists(ScoresFilePath))
        {
            using (var streamReader = File.OpenText(ScoresFilePath))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine()!;
                    if (float.TryParse(line, out var score))
                    {
                        HighScores.Add(score);
                    }
                }
            }

            HighScores.Sort((a, b) => -a.CompareTo(b));
        }

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
        DifficultyIncreaseTimer += time.DeltaTime;
        if (DifficultyIncreaseTimer > DifficultyIncreaseCooldown)
        {
            SpeedMultiplier += DifficultyIncreaseSpeedIncrement;
            DifficultyIncreaseTimer = 0;
        }

        Score += ScorePerSecond * ScoreMultiplier * time.DeltaTime;

        if (currentShapeRoot.IsAlive() && currentShapeRoot.Entity.Has<TetrisRootComponent>() && input.GetKeyDown(SDL.SDL_Scancode.SDL_SCANCODE_Q))
        {
            ref var tetrisRootComponent = ref currentShapeRoot.Entity.Get<TetrisRootComponent>();
            tetrisRootComponent.Rotation = (TetrisRotation)(((int)tetrisRootComponent.Rotation + 1) % 4);
        }

        if (currentShapeRoot.IsAlive() && currentShapeRoot.Entity.Has<TetrisRootComponent>() && input.GetKeyDown(SDL.SDL_Scancode.SDL_SCANCODE_E))
        {
            ref var tetrisRootComponent = ref currentShapeRoot.Entity.Get<TetrisRootComponent>();
            tetrisRootComponent.Rotation = (TetrisRotation)(((int)tetrisRootComponent.Rotation + 1) % 4);
        }

        if (!currentShapeRoot.IsAlive() || input.GetKeyDown(SDL.SDL_Scancode.SDL_SCANCODE_SPACE) || World.CountEntities(ShouldShouldPlaceTetris_QueryDescription) > 0)
        {
            PlaceBlocksQuery(World);

            var shape = shapes[random.Next(0, shapes.Count)];

            var currentShapeRootEntity = World.Create(
                new TetrisRootComponent
                {
                    Definition = shape,
                    Rotation = (TetrisRotation)random.Next(0, 4),
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
                            Texture = shape.Texture,
                        });
                }
            }
        }

        UpdateRootPositionsQuery(World);
        UpdateRootBlockPositionsQuery(World);

        UpdateBlockPositionsQuery(World);

        MovePlayerOutOfTileQuery(World);

        ResetIfPlayerOutOfBoundsQuery(World);
        for (var x = 0; x < tilemap.Tiles.GetLength(0); x++)
        {
            if (tilemap.Tiles[x, tilemap.Tiles.GetLength(1) - 1].IsWall)
            {
                ResetGameQuery(World);
            }
        }
    }

    [Query]
    [All<ShouldPlaceTetrisEventComponent>]
    private void ShouldShouldPlaceTetris() {}

    [Query]
    [All<PlayerComponent>]
    private void UpdateRootPositions(ref TransformComponent playerTransform)
    {
        UpdateRootPositions_1Query(World, ref playerTransform);
    }

    [Query]
    private void UpdateRootPositions_1([Data] ref TransformComponent playerTransform, ref TetrisRootComponent root, ref TransformComponent transform)
    {
        var minX = 0;
        var maxX = 9;

        switch (root.Rotation)
        {
            case TetrisRotation.R0:
            {
                minX = minX + root.Definition.PivotX;
                maxX = maxX + root.Definition.PivotX - root.Definition.Shape.GetLength(0) + 1;

                break;
            }
            case TetrisRotation.R90:
            {
                minX = minX - root.Definition.PivotY + root.Definition.Shape.GetLength(1) - 1;
                maxX = maxX - root.Definition.PivotY;

                break;
            }
            case TetrisRotation.R180:
            {
                minX = minX - root.Definition.PivotX + root.Definition.Shape.GetLength(0) - 1;
                maxX = maxX - root.Definition.PivotX;

                break;
            }
            case TetrisRotation.R270:
            {
                minX = minX + root.Definition.PivotY;
                maxX = maxX + root.Definition.PivotY - root.Definition.Shape.GetLength(1) + 1;

                break;
            }
        }

        var distanceToPlayerX = playerTransform.Position.X - transform.Position.X;
        var distanceToTravel = Math.Sign(distanceToPlayerX) * blockHorizontalSpeed * time.DeltaTime;
        distanceToTravel = Math.Clamp(distanceToTravel, -Math.Abs(distanceToPlayerX), Math.Abs(distanceToPlayerX));

        transform.Position.Y -= blockVerticalSpeed * SpeedMultiplier * time.DeltaTime;
        transform.Position.X += distanceToTravel;

        transform.Position.X = Math.Clamp(transform.Position.X, minX, maxX);
    }

    [Query]
    private void UpdateRootBlockPositions(Entity entity, ref TetrisRootComponent root, ref TransformComponent transform)
    {
        // Update logical world position of blocks
        var actualY = (int)MathF.Ceiling(transform.Position.Y);
        var predictedX = (int)MathF.Round(transform.Position.X);
        var predictedY = actualY;

        root.BlockPositions.Clear();

        for (var x = 0; x < root.Definition.Shape.GetLength(0); x++)
        {
            for (var y = 0; y < root.Definition.Shape.GetLength(1); y++)
            {
                if (!root.Definition.Shape[x, y])
                {
                    continue;
                }

                var position = new TetrisVector2Int(x - root.Definition.PivotX, y - root.Definition.PivotY);
                for (var i = 0; i < (int)root.Rotation; i++)
                {
                    position = new TetrisVector2Int(-position.Y, position.X);
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
                root.PredictedBlockPositions[i] = new TetrisVector2Int(root.PredictedBlockPositions[i].X, root.PredictedBlockPositions[i].Y - 1);
                predictedY--;
            }
        }

        if (actualY == predictedY)
        {
            entity.Add(new ShouldPlaceTetrisEventComponent());
        }

        root.PredictedBlockPositions.RemoveAll(position => position.Y >= tilemap.Tiles.GetLength(1));
    }

    [Query]
    private void UpdateBlockPositions(ref TetrisBlockComponent block, ref TransformComponent transform)
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
        transform.Position = Vector2.Transform(localPosition, Matrix4x4.CreateRotationZ(float.Pi / 2 * (int)root.Rotation) * Matrix4x4.CreateTranslation(rootTransform.Position.X, rootTransform.Position.Y, 0));
    }

    [Query]
    private void PlaceBlocks(Entity entity, ref TetrisRootComponent root)
    {
        foreach (var (x, y) in root.PredictedBlockPositions)
        {
            ref var tile = ref tilemap.Tiles[x, y];
            tile.IsWall = true;
            tile.Texture = root.Definition.Texture;
        }

        World.Create(new UpdateTilemapCollidersEventComponent());

        RemoveAllTetrisBlocksQuery(World);
        RemoveMatchingBlockTiles(ref root);
    }

    private static readonly TetrisVector2Int[] Directions =
    {
        new(1, 0),
        new(-1, 0),
        new(0, 1),
        new(0, -1),
    };

    private bool[,]? IsBlockRooted;

    private void RemoveMatchingBlockTiles(ref TetrisRootComponent root)
    {
        foreach (var position in root.PredictedBlockPositions)
        {
            foreach (var direction in Directions)
            {
                var targetPosition = new TetrisVector2Int(position.X + direction.X, position.Y + direction.Y);
                if (IsMatchingTileAndNotPartOfSelf(targetPosition, ref root))
                {
                    RecursiveRemove(targetPosition, root.Definition.Texture);
                    World.Create(new UpdateTilemapCollidersEventComponent());

                    while (TryApplyBlockGravity()) {}

                    return;
                }
            }
        }

        bool IsMatchingTileAndNotPartOfSelf(TetrisVector2Int position, ref TetrisRootComponent root)
        {
            if (root.PredictedBlockPositions.Contains(position))
            {
                return false;
            }

            if (position.X < 0
                || position.Y < 0
                || position.X >= tilemap.Tiles.GetLength(0)
                || position.Y >= tilemap.Tiles.GetLength(1))
            {
                return false;
            }

            if (tilemap.Tiles[position.X, position.Y].Texture != root.Definition.Texture)
            {
                return false;
            }

            return true;
        }

        void RecursiveRemove(TetrisVector2Int position, IResourceHandle<Texture2D> texture)
        {
            if (position.X < 0
                || position.Y < 0
                || position.X >= tilemap.Tiles.GetLength(0)
                || position.Y >= tilemap.Tiles.GetLength(1))
            {
                return;
            }

            if (tilemap.Tiles[position.X, position.Y].Texture != texture)
            {
                return;
            }

            Score += ScorePerTile * ScoreMultiplier;

            tilemap.Tiles[position.X, position.Y] = default;

            foreach (var direction in Directions)
            {
                var targetPosition = new TetrisVector2Int(position.X + direction.X, position.Y + direction.Y);
                RecursiveRemove(targetPosition, texture);
            }
        }

        // Blocks can be floating after blocks are removed
        // This is different from normal tetris because tetris removes an entire row and everything always goes down by one row
        // This first marks all first row blocks and all of the connected blocks as rooted
        // Un-rooted blocks are then moved one position down
        // If there were any movements, this will return true, meaning that this function should be ran again
        bool TryApplyBlockGravity()
        {
            if (IsBlockRooted == null || IsBlockRooted.GetLength(0) != tilemap.Tiles.GetLength(0) || IsBlockRooted.GetLength(1) != tilemap.Tiles.GetLength(1))
            {
                IsBlockRooted = new bool[tilemap.Tiles.GetLength(0), tilemap.Tiles.GetLength(1)];
            }

            Array.Clear(IsBlockRooted);

            // Mark first row
            for (var x = 0; x < tilemap.Tiles.GetLength(0); x++)
            {
                if (tilemap.Tiles[x, 0].IsWall)
                {
                    RecursiveMarkRooted(new TetrisVector2Int(x, 0));
                }
            }

            // Move un-rooted blocks downwards
            var wereAnyBlocksMoved = false;
            for (var x = 0; x < tilemap.Tiles.GetLength(0); x++)
            {
                for (var y = 1; y < tilemap.Tiles.GetLength(1); y++)
                {
                    if (IsBlockRooted[x, y] || !tilemap.Tiles[x, y].IsWall)
                    {
                        continue;
                    }

                    tilemap.Tiles[x, y - 1] = tilemap.Tiles[x, y];
                    tilemap.Tiles[x, y] = default;

                    wereAnyBlocksMoved = true;
                }
            }

            return wereAnyBlocksMoved;
        }

        void RecursiveMarkRooted(TetrisVector2Int position)
        {
            if (position.X < 0
                || position.Y < 0
                || position.X >= tilemap.Tiles.GetLength(0)
                || position.Y >= tilemap.Tiles.GetLength(1))
            {
                return;
            }

            if (IsBlockRooted[position.X, position.Y])
            {
                return;
            }

            if (!tilemap.Tiles[position.X, position.Y].IsWall)
            {
                return;
            }

            IsBlockRooted[position.X, position.Y] = true;

            foreach (var direction in Directions)
            {
                var targetPosition = new TetrisVector2Int(position.X + direction.X, position.Y + direction.Y);
                RecursiveMarkRooted(targetPosition);
            }
        }
    }

    [Query]
    [All<PlayerComponent>]
    private void MovePlayerOutOfTile(ref TransformComponent transform)
    {
        var position = new TetrisVector2Int((int)MathF.Round(transform.Position.X), (int)MathF.Round(transform.Position.Y));
        if (position.X < 0
            || position.Y < 0
            || position.X >= tilemap.Tiles.GetLength(0)
            || position.Y >= tilemap.Tiles.GetLength(1))
        {
            return;
        }

        if (tilemap.Tiles[position.X, position.Y].IsWall)
        {
            var safeY = position.Y;
            while (safeY < tilemap.Tiles.GetLength(1) && tilemap.Tiles[position.X, safeY].IsWall)
            {
                safeY++;
            }

            transform.Position.Y = safeY;
        }
    }

    [Query]
    [All<PlayerComponent>]
    private void ResetIfPlayerOutOfBounds(ref TransformComponent transform)
    {
        if (!(transform.Position.Y < -1f) && !(transform.Position.Y > 20.5f))
        {
            return;
        }

        ResetGameQuery(World);
    }

    [Query]
    [All<PlayerComponent>]
    private void ResetGame(ref TransformComponent playerTransform, ref VelocityComponent velocity)
    {
        playerTransform.Position = new Vector2(4f, 0);
        velocity.Velocity = Vector2.Zero;
        playerControllerSystem.SetIsGravityDown(true);

        RemoveAllTetrisBlocksQuery(World);

        for (var x = 0; x < tilemap.Tiles.GetLength(0); x++)
        {
            for (var y = 0; y < tilemap.Tiles.GetLength(1); y++)
            {
                tilemap.Tiles[x, y] = default;
            }
        }

        HighScores.Add(Score);
        HighScores.Sort((a, b) => -a.CompareTo(b));

        PreviousScore = Score;
        Score = 0;
        SpeedMultiplier = 1;
        DifficultyIncreaseTimer = 0;

        World.Create(new UpdateTilemapCollidersEventComponent());

        Directory.CreateDirectory(GameDirectories.PersistentDataDirectory);
        using (var stream = new FileStream(ScoresFilePath, FileMode.Append))
        using (var streamWriter = new StreamWriter(stream))
        {
            streamWriter.WriteLine(PreviousScore);
        }

        HighScores.Sort((a, b) => -a.CompareTo(b));
    }

    [Query]
    [Any<TetrisRootComponent, TetrisBlockComponent>]
    private void RemoveAllTetrisBlocks(Entity entity)
    {
        if (!entity.Has<DestroyedComponent>())
        {
            entity.Add(new DestroyedComponent());
        }
    }
}
