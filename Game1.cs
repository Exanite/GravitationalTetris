﻿using System;
using System.Threading.Tasks;
using Arch.CommandBuffer;
using Autofac;
using Exanite.WarGames.Features;
using Exanite.WarGames.Features.Cameras.Systems;
using Exanite.WarGames.Features.Lifecycles.Systems;
using Exanite.WarGames.Features.Physics.Systems;
using Exanite.WarGames.Features.Players;
using Exanite.WarGames.Features.Players.Systems;
using Exanite.WarGames.Features.Resources;
using Exanite.WarGames.Features.Resources.Systems;
using Exanite.WarGames.Features.Sprites;
using Exanite.WarGames.Features.Sprites.Systems;
using Exanite.WarGames.Features.Tiles;
using Exanite.WarGames.Features.Tiles.Systems;
using Exanite.WarGames.Features.Time;
using Exanite.WarGames.Systems;
using Microsoft.Xna.Framework;
using EcsWorld = Arch.Core.World;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.WarGames;

public class Game1 : Game, IAsyncDisposable
{
    private readonly IContainer container;

    private readonly SystemScheduler systemScheduler;
    private readonly GameTimeData time;

    public Game1()
    {
        container = BuildContainer();
        systemScheduler = container.Resolve<SystemScheduler>();
        time = container.Resolve<GameTimeData>();
    }

    protected override void Initialize()
    {
        Content.RootDirectory = "Content";

        IsMouseVisible = true;
        Window.AllowUserResizing = true;

        systemScheduler.RegisterCallbacks();
        systemScheduler.Start();

        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        time.Time = (float)gameTime.TotalGameTime.TotalSeconds;
        time.DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        systemScheduler.Update();
        systemScheduler.Cleanup();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        time.Time = (float)gameTime.TotalGameTime.TotalSeconds;
        time.DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        GraphicsDevice.Clear(Color.Black);
        systemScheduler.Draw();

        base.Draw(gameTime);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await container.DisposeAsync();

        Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    private IContainer BuildContainer()
    {
        var builder = new ContainerBuilder();

        // Misc
        builder.RegisterType<Random>().InstancePerDependency();

        // Resources
        builder.RegisterModule<ResourcesModule>();

        // FNA
        builder.RegisterInstance(this).AsSelf().As<Game>().SingleInstance();
        builder.RegisterInstance(new GraphicsDeviceManager(this)).SingleInstance();

        // Shared data
        builder.RegisterType<GameInputData>().SingleInstance();
        builder.RegisterType<GameTimeData>().SingleInstance();
        builder.RegisterType<GameSpriteBatch>().SingleInstance();
        builder.RegisterType<GameTilemapData>().SingleInstance();

        // Physics
        builder.RegisterType<PhysicsWorld>().SingleInstance();

        // ECS world
        builder.Register(_ => EcsWorld.Create()).SingleInstance();
        builder.Register(ctx => new CommandBuffer(ctx.Resolve<EcsWorld>())).InstancePerDependency();

        // Systems
        var schedulerConfig = CreateSystemSchedulerConfig();
        builder.RegisterInstance(schedulerConfig).SingleInstance();
        builder.RegisterModule(schedulerConfig);
        builder.RegisterType<SystemScheduler>().SingleInstance();

        return builder.Build();
    }

    private SystemScheduler.Config CreateSystemSchedulerConfig()
    {
        var config = new SystemScheduler.Config();

        // Callbacks
        config.RegisterCallbackSystem<PhysicsContactSystem>();
        config.RegisterCallbackSystem<TetrisSystem>();
        config.RegisterCallbackSystem<TilemapDrawSystem>();

        // Start
        config.RegisterStartSystem<PhysicsSimulationSystem>();
        config.RegisterStartSystem<CreateEntitiesSystem>();
        config.RegisterStartSystem<TilemapColliderSystem>();

        // Update
        config.RegisterUpdateSystem<InputSystem>();
        config.RegisterUpdateSystem<PlayerControllerSystem>();
        config.RegisterUpdateSystem<TetrisSystem>();
        config.RegisterUpdateSystem<TilemapColliderSystem>();

        config.RegisterUpdateSystem<PhysicsSimulationSystem>();
        config.RegisterUpdateSystem<CameraProjectionSystem>();

        config.RegisterUpdateSystem<RunResourceManagerSystem>();

        // Cleanup
        config.RegisterCleanupSystem<PhysicsSimulationSystem>();
        config.RegisterCleanupSystem<RemoveDestroyedSystem>();

        // Draw
        config.RegisterDrawSystem<TilemapDrawSystem>();
        config.RegisterDrawSystem<SpriteDrawSystem>();

        return config;
    }
}
