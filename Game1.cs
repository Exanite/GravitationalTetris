using System;
using System.IO;
using System.Threading.Tasks;
using Arch.CommandBuffer;
using Autofac;
using Exanite.Ecs.Systems;
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
using Exanite.WarGames.Features.Tetris.Systems;
using Exanite.WarGames.Features.Tiles;
using Exanite.WarGames.Features.Tiles.Systems;
using Exanite.WarGames.Features.Time;
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
            systemScheduler.Draw();

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
        var schedulerConfig = CreateSystemSchedulerConfig();
        builder.RegisterInstance(schedulerConfig).SingleInstance();
        builder.RegisterModule(schedulerConfig);
        builder.RegisterType<SystemScheduler>().SingleInstance();

        return builder.Build();
    }

    private SystemScheduler.Config CreateSystemSchedulerConfig()
    {
        var config = new SystemScheduler.Config();

        config.SimpleRegisterSystem<CreateEntitiesSystem>();

        config.SimpleRegisterSystem<PhysicsSimulationSystem>();
        config.SimpleRegisterSystem<PhysicsContactSystem>();

        config.SimpleRegisterSystem<InputSystem>();
        config.SimpleRegisterSystem<PlayerControllerSystem>();
        config.SimpleRegisterSystem<TetrisSystem>();
        config.SimpleRegisterSystem<TilemapColliderSystem>();
        config.SimpleRegisterSystem<CameraProjectionSystem>();

        config.SimpleRegisterSystem<TilemapDrawSystem>();
        config.SimpleRegisterSystem<SpriteDrawSystem>();
        config.SimpleRegisterSystem<TetrisUiSystem>();

        config.SimpleRegisterSystem<RemoveDestroyedSystem>();
        config.SimpleRegisterSystem<RunResourceManagerSystem>();

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
