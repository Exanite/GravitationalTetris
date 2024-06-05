using System;
using Autofac;
using Exanite.Ecs.Systems;
using Exanite.Engine;
using Exanite.Engine.GameLoops;
using Exanite.Engine.Inputs;
using Exanite.Engine.Inputs.Systems;
using Exanite.Engine.Lifecycles.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Rendering.Systems;
using Exanite.Engine.Time;
using Exanite.Engine.Time.Systems;
using Exanite.Engine.Windowing;
using Exanite.Engine.Windowing.Systems;
using Exanite.GravitationalTetris.Features;
using Exanite.GravitationalTetris.Features.Audio.Systems;
using Exanite.GravitationalTetris.Features.Cameras.Systems;
using Exanite.GravitationalTetris.Features.Physics.Systems;
using Exanite.GravitationalTetris.Features.Players.Systems;
using Exanite.GravitationalTetris.Features.Rendering.Systems;
using Exanite.GravitationalTetris.Features.Resources;
using Exanite.GravitationalTetris.Features.Sprites.Systems;
using Exanite.GravitationalTetris.Features.Tetris.Systems;
using Exanite.GravitationalTetris.Features.Tiles;
using Exanite.GravitationalTetris.Features.Tiles.Systems;
using Exanite.GravitationalTetris.Features.Ui;
using Exanite.GravitationalTetris.Features.Ui.Systems;
using Exanite.Logging;
using Serilog;
using EcsWorld = Arch.Core.World;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris;

public class Game1 : Game
{
    protected override ContainerBuilder CreateContainer()
    {
        var builder = new ContainerBuilder();

        // Container
        builder.RegisterInstance(this).SingleInstance();

        // Misc
        builder.RegisterType<Random>().InstancePerDependency();

        // Logging
        builder.RegisterModule(new LoggingModule(GameDirectories.LogsDirectory));

        // Windowing
        builder.Register(_ => new WindowSettings()
        {
            Name = "Gravitational Tetris",
        }).SingleInstance();

        builder.RegisterType<Window>().SingleInstance();

        // Rendering
        builder.Register(ctx =>
            {
                var logger = ctx.Resolve<ILogger>();

                return new RendererContext(GraphicsApi.Vulkan, logger);
            })
            .SingleInstance();

        // Time
        builder.RegisterType<SimulationTime>().SingleInstance();

        // Input
        builder.RegisterType<Input>().SingleInstance();

        // UI
        builder.RegisterModule<UiModule>();

        // Shared data
        builder.RegisterType<GameTilemapData>().SingleInstance();

        // Physics
        builder.RegisterType<PhysicsWorld>().SingleInstance();

        // ECS world
        builder.Register(_ => EcsWorld.Create()).SingleInstance();

        // Game loop
        builder.RegisterType<EcsGameLoop>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterModule(CreateSystemSchedulerConfig());

        // Resources
        builder.RegisterModule<ResourcesModule>();

        return builder;
    }

    private SystemScheduler.Config CreateSystemSchedulerConfig()
    {
        var config = new SystemScheduler.Config();

        config.Register<WindowSystem>();
        config.Register<UpdateWindowSizeSystem>();

        config.Register<TimeSystem>();
        config.Register<InputSystem>();

        config.Register<CreateEntitiesSystem>();

        config.Register<PhysicsSimulationSystem>();
        config.Register<PhysicsContactSystem>();

        config.Register<PlayerControllerSystem>();
        config.Register<TetrisSystem>();
        config.Register<TilemapColliderSystem>();

        config.Register<FmodAudioSystem>();

        config.Register<ResizeSwapChainSystem>();
        config.Register<ClearMainRenderTargetSystem>();
        {
            config.Register<SetClearColorSystem>();
            config.Register<CameraProjectionSystem>();
            config.Register<SpriteBatchSystem>();

            // World RT
            config.Register<WorldRenderTextureSystem>();

            config.Register<TilemapRenderSystem>();
            config.Register<SpriteRenderSystem>();

            // Main RT
            config.Register<BloomSystem>();

            config.Register<UseMainRenderTargetSystem>();

            config.Register<ToneMappingSystem>();

            config.Register<RenderWorldToMainSystem>();

            config.Register<MyraUiSystem>();
            config.Register<TetrisUiSystem>();
        }
        config.Register<PresentSwapChainSystem>();

        config.Register<RemoveDestroyedSystem>();

        return config;
    }

    public override void Run()
    {
        var gameLoop = Container.Resolve<EcsGameLoop>();
        gameLoop.Run();
    }
}
