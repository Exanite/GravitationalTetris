﻿using System;
using System.IO;
using System.Threading.Tasks;
using Arch.CommandBuffer;
using Autofac;
using Exanite.Ecs.Systems;
using Exanite.GravitationalTetris.Features;
using Exanite.GravitationalTetris.Features.Cameras.Systems;
using Exanite.GravitationalTetris.Features.Lifecycles.Systems;
using Exanite.GravitationalTetris.Features.Physics.Systems;
using Exanite.GravitationalTetris.Features.Players;
using Exanite.GravitationalTetris.Features.Players.Systems;
using Exanite.GravitationalTetris.Features.Resources;
using Exanite.GravitationalTetris.Features.Resources.Systems;
using Exanite.GravitationalTetris.Features.Sprites;
using Exanite.GravitationalTetris.Features.Sprites.Systems;
using Exanite.GravitationalTetris.Features.Tetris.Systems;
using Exanite.GravitationalTetris.Features.Tiles;
using Exanite.GravitationalTetris.Features.Tiles.Systems;
using Exanite.GravitationalTetris.Features.Time;
using Microsoft.Xna.Framework;
using EcsWorld = Arch.Core.World;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris;

public class Game1 : Game, IAsyncDisposable
{
    private readonly IContainer container;

    private readonly SystemScheduler systemScheduler;
    private readonly GameTimeData time;

    public Game1()
    {
        Window.Title = "Gravitational Tetris";

        container = BuildContainer();
        systemScheduler = container.Resolve<SystemScheduler>();
        time = container.Resolve<GameTimeData>();
    }

    protected override void Initialize()
    {
        Content.RootDirectory = "Content";

        IsMouseVisible = true;
        Window.AllowUserResizing = true;

        systemScheduler.Initialize();
        systemScheduler.Start();

        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        try
        {
            time.Time = (float)gameTime.TotalGameTime.TotalSeconds;
            time.DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            systemScheduler.Update();
            systemScheduler.Cleanup();

            base.Update(gameTime);
        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        try
        {
            time.Time = (float)gameTime.TotalGameTime.TotalSeconds;
            time.DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            GraphicsDevice.Clear(Color.Black);
            systemScheduler.Render();

            base.Draw(gameTime);
        }
        catch (Exception e)
        {
            HandleException(e);
        }
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
        builder.RegisterModule(CreateSystemSchedulerConfig());

        return builder.Build();
    }

    private SystemScheduler.Config CreateSystemSchedulerConfig()
    {
        var config = new SystemScheduler.Config();

        config.RegisterAllCallbacks<CreateEntitiesSystem>();

        config.RegisterAllCallbacks<PhysicsSimulationSystem>();
        config.RegisterAllCallbacks<PhysicsContactSystem>();

        config.RegisterAllCallbacks<InputSystem>();
        config.RegisterAllCallbacks<PlayerControllerSystem>();
        config.RegisterAllCallbacks<TetrisSystem>();
        config.RegisterAllCallbacks<TilemapColliderSystem>();
        config.RegisterAllCallbacks<CameraProjectionSystem>();

        config.RegisterAllCallbacks<TilemapRenderSystem>();
        config.RegisterAllCallbacks<SpriteRenderSystem>();
        config.RegisterAllCallbacks<TetrisUiSystem>();

        config.RegisterAllCallbacks<RemoveDestroyedSystem>();
        config.RegisterAllCallbacks<RunResourceManagerSystem>();

        return config;
    }

    private void HandleException(Exception e)
    {
        Directory.CreateDirectory(GameDirectories.PersistentDataDirectory);
        using (var stream = File.Open(Path.Join(GameDirectories.PersistentDataDirectory, "Game.log"), FileMode.Append))
        using (var streamWriter = new StreamWriter(stream))
        {
            streamWriter.WriteLine(e);
        }

        Console.Error.WriteLine(e);

        Environment.Exit(-1);
    }
}
