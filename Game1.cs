using System;
using Autofac;
using Exanite.Ecs.Systems;
using Exanite.Engine;
using Exanite.Engine.GameLoops;
using Exanite.Engine.Inputs;
using Exanite.Engine.Inputs.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Rendering.Systems;
using Exanite.Engine.Time;
using Exanite.Engine.Time.Systems;
using Exanite.Engine.Windowing;
using Exanite.Engine.Windowing.Systems;
using Exanite.GravitationalTetris.Features;
using Exanite.GravitationalTetris.Features.Audio.Systems;
using Exanite.GravitationalTetris.Features.Cameras.Systems;
using Exanite.GravitationalTetris.Features.Lifecycles.Systems;
using Exanite.GravitationalTetris.Features.Physics.Systems;
using Exanite.GravitationalTetris.Features.Players.Systems;
using Exanite.GravitationalTetris.Features.Resources;
using Exanite.GravitationalTetris.Features.Resources.Systems;
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

        // Rendering
        builder.Register(_ =>
        {
            return new RendererContextSettings
            {
                UseCombinedSamplers = false,
            };
        });

        builder.Register(_ =>
            {
                return new Window("Gravitational Tetris");
            })
            .SingleInstance();

        builder.Register(ctx =>
            {
                var window = ctx.Resolve<Window>();
                var logger = ctx.Resolve<ILogger>();
                var settings = ctx.Resolve<RendererContextSettings>();

                return new RendererContext(GraphicsApi.Vulkan, window, logger, settings);
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

        config.RegisterAllCallbacks<WindowSystem>();
        config.RegisterAllCallbacks<UpdateWindowSizeSystem>();

        config.RegisterAllCallbacks<TimeSystem>();
        config.RegisterAllCallbacks<InputSystem>();

        config.RegisterAllCallbacks<CreateEntitiesSystem>();

        config.RegisterAllCallbacks<PhysicsSimulationSystem>();
        config.RegisterAllCallbacks<PhysicsContactSystem>();

        config.RegisterAllCallbacks<PlayerControllerSystem>();
        config.RegisterAllCallbacks<TetrisSystem>();
        config.RegisterAllCallbacks<TilemapColliderSystem>();

        config.RegisterAllCallbacks<FmodAudioSystem>();

        config.RegisterAllCallbacks<ResizeSwapChainSystem>();
        config.RegisterAllCallbacks<ClearRenderTargetRenderSystem>();
        {
            config.RegisterAllCallbacks<SetClearColorSystem>();
            config.RegisterAllCallbacks<SpriteBatchSystem>();

            config.RegisterAllCallbacks<CameraProjectionSystem>();
            config.RegisterAllCallbacks<TilemapRenderSystem>();
            config.RegisterAllCallbacks<SpriteRenderSystem>();

            config.RegisterAllCallbacks<MyraUiSystem>();
            config.RegisterAllCallbacks<TetrisUiSystem>();
        }
        config.RegisterAllCallbacks<PresentSwapChainSystem>();

        config.RegisterAllCallbacks<RemoveDestroyedSystem>();
        config.RegisterAllCallbacks<RunResourceManagerSystem>();

        return config;
    }

    public override void Run()
    {
        var gameLoop = Container.Resolve<EcsGameLoop>();
        gameLoop.Run();
    }
}
