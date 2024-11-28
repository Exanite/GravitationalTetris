using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Exanite.Engine.Ecs.Queries;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Framework;
using Exanite.Engine.Inputs;
using Exanite.Engine.Inputs.Actions;
using Exanite.Engine.Lifecycles.Components;
using Exanite.Engine.Timing;
using Exanite.GravitationalTetris.Features.Audio.Systems;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Players.Components;
using Exanite.GravitationalTetris.Features.Players.Systems;
using Exanite.GravitationalTetris.Features.Sprites.Components;
using Exanite.GravitationalTetris.Features.Tetris.Components;
using Exanite.GravitationalTetris.Features.Tiles;
using Exanite.GravitationalTetris.Features.Tiles.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Exanite.ResourceManagement;
using Myriad.ECS;
using Myriad.ECS.Command;
using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.GravitationalTetris.Features.Tetris.Systems;

public partial class TetrisSystem : EcsSystem, ISetupSystem, IUpdateSystem
{
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
    private Entity currentShapeRoot;

    private CommandBuffer commandBuffer = null!;

    private IInputAction<bool> placeShapeAction = null!;
    private IInputAction<bool> rotateLeftAction = null!;
    private IInputAction<bool> rotateRightAction = null!;

    private readonly List<TetrisShapeDefinition> shapes = new();

    private readonly ResourceManager resourceManager;
    private readonly Random random;
    private readonly ITime time;
    private readonly InputActionManager input;
    private readonly GameTilemapData tilemap;
    private readonly PlayerControllerSystem playerControllerSystem;
    private readonly FmodAudioSystem audioSystem;
    private readonly EnginePaths paths;

    private string ScoresFilePath => Path.Join(paths.PersistentDataFolder, "Scores.txt");

    public TetrisSystem(
        ResourceManager resourceManager,
        Random random,
        ITime time,
        InputActionManager input,
        PlayerControllerSystem playerControllerSystem,
        GameTilemapData tilemap,
        FmodAudioSystem audioSystem,
        EnginePaths paths)
    {
        this.resourceManager = resourceManager;
        this.random = random;
        this.time = time;
        this.input = input;
        this.playerControllerSystem = playerControllerSystem;
        this.tilemap = tilemap;
        this.audioSystem = audioSystem;
        this.paths = paths;
    }

    public void Setup()
    {
        commandBuffer = new CommandBuffer(World);

        placeShapeAction = input.RegisterAction(new OrInputAction([
            new ButtonInputAction(KeyCode.Space),
            new ButtonInputAction(KeyCode.LeftMouse),
        ]));

        rotateLeftAction = input.RegisterAction(new OrInputAction([
            new ButtonInputAction(KeyCode.Q),
            new ButtonInputAction(KeyCode.BackwardMouse),
        ]));

        rotateRightAction = input.RegisterAction(new OrInputAction([
            new ButtonInputAction(KeyCode.E),
            new ButtonInputAction(KeyCode.ForwardMouse),
            new ButtonInputAction(KeyCode.RightMouse),
        ]));

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
            Shape = new[,]
            {
                { true, true, true, true },
            },

            PivotX = 0,
            PivotY = 1,

            DefaultTexture = resourceManager.GetResource(BaseMod.TileCyan),
            SnowTexture = resourceManager.GetResource(WinterMod.TileCyan),
        });

        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new[,]
            {
                { true, false, false },
                { true, true, true },
            },

            PivotX = 1,
            PivotY = 1,

            DefaultTexture = resourceManager.GetResource(BaseMod.TileBlue),
            SnowTexture = resourceManager.GetResource(WinterMod.TileBlue),
        });

        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new[,]
            {
                { false, false, true },
                { true, true, true },
            },

            PivotX = 1,
            PivotY = 1,

            DefaultTexture = resourceManager.GetResource(BaseMod.TileOrange),
            SnowTexture = resourceManager.GetResource(WinterMod.TileOrange),
        });

        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new[,]
            {
                { true, true },
                { true, true },
            },

            PivotX = 0,
            PivotY = 0,

            DefaultTexture = resourceManager.GetResource(BaseMod.TileYellow),
            SnowTexture = resourceManager.GetResource(WinterMod.TileYellow),
        });

        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new[,]
            {
                { false, true, true },
                { true, true, false },
            },

            PivotX = 1,
            PivotY = 1,

            DefaultTexture = resourceManager.GetResource(BaseMod.TileGreen),
            SnowTexture = resourceManager.GetResource(WinterMod.TileGreen),
        });

        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new[,]
            {
                { false, true, false },
                { true, true, true },
            },

            PivotX = 1,
            PivotY = 1,

            DefaultTexture = resourceManager.GetResource(BaseMod.TilePurple),
            SnowTexture = resourceManager.GetResource(WinterMod.TilePurple),
        });

        shapes.Add(new TetrisShapeDefinition
        {
            Shape = new[,]
            {
                { true, true, false },
                { false, true, true },
            },

            PivotX = 1,
            PivotY = 1,

            DefaultTexture = resourceManager.GetResource(BaseMod.TileRed),
            SnowTexture = resourceManager.GetResource(WinterMod.TileRed),
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

        if (currentShapeRoot.IsAlive() && currentShapeRoot.HasComponent<ComponentTetrisRoot>() && rotateLeftAction.IsPressed())
        {
            audioSystem.Play(FmodAudioSystem.RotateShape);

            ref var tetrisRootComponent = ref currentShapeRoot.GetComponentRef<ComponentTetrisRoot>();
            tetrisRootComponent.Rotation = (TetrisRotation)(((int)tetrisRootComponent.Rotation + 3) % 4);
        }

        if (currentShapeRoot.IsAlive() && currentShapeRoot.HasComponent<ComponentTetrisRoot>() && rotateRightAction.IsPressed())
        {
            audioSystem.Play(FmodAudioSystem.RotateShape);

            ref var tetrisRootComponent = ref currentShapeRoot.GetComponentRef<ComponentTetrisRoot>();
            tetrisRootComponent.Rotation = (TetrisRotation)(((int)tetrisRootComponent.Rotation + 1) % 4);
        }

        if (!currentShapeRoot.IsAlive())
        {
            PlaceShape();
        }
        else if (placeShapeAction.IsPressed() || World.Count(ShouldShouldPlaceTetrisQueryDescription(World)) > 0)
        {
            playerControllerSystem.FlipGravity();
            audioSystem.Play(FmodAudioSystem.SwitchGravity);

            PlaceShape();
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

        commandBuffer.Playback().Dispose();
    }

    private void PlaceShape()
    {
        PlaceBlocksQuery(World);

        var shape = shapes[random.Next(0, shapes.Count)];

        var currentShapeRootEntity = commandBuffer.Create()
            .Set(new ComponentTetrisRoot
            {
                Shape = shape,
                Rotation = (TetrisRotation)random.Next(0, 4),
            })
            .Set(new ComponentTransform
            {
                Position = new Vector2(5, 20),
            });

        using var resolver = commandBuffer.Playback();
        currentShapeRoot = currentShapeRootEntity.Resolve();

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

                commandBuffer.Create()
                    .Set(new ComponentTetrisBlock
                    {
                        Root = currentShapeRoot,

                        Definition = shape,
                        LocalX = x - shape.PivotX,
                        LocalY = y - shape.PivotY,
                    })
                    .Set(new ComponentTransform())
                    .Set(new ComponentRigidbody(body))
                    .Set(new ComponentSprite
                    {
                        Texture = shape.DefaultTexture,
                    });
            }
        }

        commandBuffer.Playback().Dispose();
    }

    [Query]
    [Include<ComponentShouldPlaceTetrisEvent>]
    private void ShouldShouldPlaceTetris() {}

    [Query]
    [Include<ComponentPlayer>]
    private void UpdateRootPositions(ref ComponentTransform playerTransform)
    {
        UpdateRootPositions_1Query(World, ref playerTransform);
    }

    [Query]
    private void UpdateRootPositions_1([Data] ref ComponentTransform playerTransform, ref ComponentTetrisRoot root, ref ComponentTransform transform)
    {
        var minX = 0;
        var maxX = 9;

        switch (root.Rotation)
        {
            case TetrisRotation.R0:
            {
                // ReSharper disable once UselessBinaryOperation - For better readability
                minX = minX + root.Shape.PivotX;
                maxX = maxX + root.Shape.PivotX - root.Shape.Shape.GetLength(0) + 1;

                break;
            }
            case TetrisRotation.R90:
            {
                minX = minX - root.Shape.PivotY + root.Shape.Shape.GetLength(1) - 1;
                maxX = maxX - root.Shape.PivotY;

                break;
            }
            case TetrisRotation.R180:
            {
                minX = minX - root.Shape.PivotX + root.Shape.Shape.GetLength(0) - 1;
                maxX = maxX - root.Shape.PivotX;

                break;
            }
            case TetrisRotation.R270:
            {
                // ReSharper disable once UselessBinaryOperation - For better readability
                minX = minX + root.Shape.PivotY;
                maxX = maxX + root.Shape.PivotY - root.Shape.Shape.GetLength(1) + 1;

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
    private void UpdateRootBlockPositions(Entity entity, ref ComponentTetrisRoot root, ref ComponentTransform transform)
    {
        // Update logical world position of blocks
        var actualY = (int)MathF.Ceiling(transform.Position.Y);
        var predictedX = (int)MathF.Round(transform.Position.X);
        var predictedY = actualY;

        root.BlockPositions.Clear();

        for (var x = 0; x < root.Shape.Shape.GetLength(0); x++)
        {
            for (var y = 0; y < root.Shape.Shape.GetLength(1); y++)
            {
                if (!root.Shape.Shape[x, y])
                {
                    continue;
                }

                var position = new TetrisVector2Int(x - root.Shape.PivotX, y - root.Shape.PivotY);
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
            commandBuffer.Set(entity, new ComponentShouldPlaceTetrisEvent());
        }

        root.PredictedBlockPositions.RemoveAll(position => position.Y >= tilemap.Tiles.GetLength(1));
    }

    [Query]
    private void UpdateBlockPositions(ref ComponentTetrisBlock block, ref ComponentTransform transform)
    {
        if (!block.Root.IsAlive())
        {
            return;
        }

        var rootEntity = block.Root;
        if (!rootEntity.HasComponent<ComponentTetrisRoot>() || !rootEntity.HasComponent<ComponentTransform>())
        {
            return;
        }

        ref var root = ref rootEntity.GetComponentRef<ComponentTetrisRoot>();
        ref var rootTransform = ref rootEntity.GetComponentRef<ComponentTransform>();

        var localPosition = new Vector2(block.LocalX, block.LocalY);
        transform.Position = Vector2.Transform(localPosition, Matrix4x4.CreateRotationZ(float.Pi / 2 * (int)root.Rotation) * Matrix4x4.CreateTranslation(rootTransform.Position.X, rootTransform.Position.Y, 0));
    }

    [Query]
    private void PlaceBlocks(ref ComponentTetrisRoot root)
    {
        foreach (var (x, y) in root.PredictedBlockPositions)
        {
            ref var tile = ref tilemap.Tiles[x, y];
            tile.IsWall = true;
            tile.Shape = root.Shape;
        }

        commandBuffer.Create().Set(new ComponentUpdateTilemapCollidersEvent());

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

    private bool[,]? isBlockRooted;

    private void RemoveMatchingBlockTiles(ref ComponentTetrisRoot root)
    {
        foreach (var position in root.PredictedBlockPositions)
        {
            foreach (var direction in Directions)
            {
                var targetPosition = new TetrisVector2Int(position.X + direction.X, position.Y + direction.Y);
                if (IsMatchingTileAndNotPartOfSelf(targetPosition, ref root))
                {
                    RecursiveRemove(targetPosition, root.Shape);
                    commandBuffer.Create().Set(new ComponentUpdateTilemapCollidersEvent());

                    while (TryApplyBlockGravity()) {}

                    audioSystem.Play(FmodAudioSystem.ClearTile);

                    return;
                }
            }
        }

        return;

        bool IsMatchingTileAndNotPartOfSelf(TetrisVector2Int position, ref ComponentTetrisRoot root)
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

            if (tilemap.Tiles[position.X, position.Y].Shape != root.Shape)
            {
                return false;
            }

            return true;
        }

        void RecursiveRemove(TetrisVector2Int position, TetrisShapeDefinition shape)
        {
            if (position.X < 0
                || position.Y < 0
                || position.X >= tilemap.Tiles.GetLength(0)
                || position.Y >= tilemap.Tiles.GetLength(1))
            {
                return;
            }

            if (tilemap.Tiles[position.X, position.Y].Shape != shape)
            {
                return;
            }

            Score += ScorePerTile * ScoreMultiplier;

            tilemap.Tiles[position.X, position.Y] = default;

            foreach (var direction in Directions)
            {
                var targetPosition = new TetrisVector2Int(position.X + direction.X, position.Y + direction.Y);
                RecursiveRemove(targetPosition, shape);
            }
        }

        // Blocks can be floating after blocks are removed
        // This is different from normal tetris because tetris removes an entire row and everything always goes down by one row
        // This first marks all first row blocks and all of the connected blocks as rooted
        // Un-rooted blocks are then moved one position down
        // If there were any movements, this will return true, meaning that this function should be ran again
        bool TryApplyBlockGravity()
        {
            if (isBlockRooted == null || isBlockRooted.GetLength(0) != tilemap.Tiles.GetLength(0) || isBlockRooted.GetLength(1) != tilemap.Tiles.GetLength(1))
            {
                isBlockRooted = new bool[tilemap.Tiles.GetLength(0), tilemap.Tiles.GetLength(1)];
            }

            Array.Clear(isBlockRooted);

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
                    if (isBlockRooted[x, y] || !tilemap.Tiles[x, y].IsWall)
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

            if (isBlockRooted[position.X, position.Y])
            {
                return;
            }

            if (!tilemap.Tiles[position.X, position.Y].IsWall)
            {
                return;
            }

            isBlockRooted[position.X, position.Y] = true;

            foreach (var direction in Directions)
            {
                var targetPosition = new TetrisVector2Int(position.X + direction.X, position.Y + direction.Y);
                RecursiveMarkRooted(targetPosition);
            }
        }
    }

    [Query]
    [Include<ComponentPlayer>]
    private void MovePlayerOutOfTile(ref ComponentTransform transform)
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
    [Include<ComponentPlayer>]
    private void ResetIfPlayerOutOfBounds(ref ComponentTransform transform)
    {
        if (!(transform.Position.Y < -1f) && !(transform.Position.Y > 20.5f))
        {
            return;
        }

        ResetGameQuery(World);
    }

    [Query]
    [Include<ComponentPlayer>]
    private void ResetGame(ref ComponentTransform playerTransform, ref ComponentVelocity velocity)
    {
        audioSystem.Play(FmodAudioSystem.Restart);

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

        commandBuffer.Create().Set(new ComponentUpdateTilemapCollidersEvent());

        Directory.CreateDirectory(paths.PersistentDataFolder);
        using (var stream = new FileStream(ScoresFilePath, FileMode.Append))
        using (var streamWriter = new StreamWriter(stream))
        {
            streamWriter.WriteLine(PreviousScore);
        }

        HighScores.Sort((a, b) => -a.CompareTo(b));
    }

    [Query]
    [AtLeastOneOf<ComponentTetrisRoot, ComponentTetrisBlock>]
    private void RemoveAllTetrisBlocks(Entity entity)
    {
        if (!entity.HasComponent<ComponentDestroyed>())
        {
            commandBuffer.Set(entity, new ComponentDestroyed());
        }
    }
}
