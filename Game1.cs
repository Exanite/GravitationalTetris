using System;
using System.Numerics;
using Autofac;
using Exanite.Ecs.Systems;
using Exanite.Engine;
using Exanite.Engine.Avalonia;
using Exanite.Engine.Avalonia.Systems;
using Exanite.Engine.GameLoops;
using Exanite.Engine.Inputs;
using Exanite.Engine.Inputs.Systems;
using Exanite.Engine.Lifecycles.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Rendering.Systems;
using Exanite.Engine.Threading;
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
using Exanite.GravitationalTetris.Features.UserInterface;
using Exanite.Logging;
using Exanite.ResourceManagement;
using EcsWorld = Arch.Core.World;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris;

public class Game1 : Game
{
    protected override ContainerBuilder CreateContainer()
    {
        var builder = new ContainerBuilder();

        // Game
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
        builder.RegisterType<RendererContext>().SingleInstance();

        // Input
        builder.RegisterType<Input>().SingleInstance();

        // Shared data
        builder.RegisterType<GameTilemapData>().SingleInstance();

        // Physics
        builder.RegisterType<PhysicsWorld>().SingleInstance();

        // ECS world
        builder.Register(_ => EcsWorld.Create()).SingleInstance();

        // Game loop
        builder.RegisterType<EcsGameLoop>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterModule(CreateSystemSchedulerConfig());

        // Modules
        builder.RegisterModule<AvaloniaModule<App>>();
        builder.RegisterModule<ResourcesModule>();
        builder.RegisterModule<ThreadingModule>();
        builder.RegisterModule<TimeModule>();

        return builder;
    }

    private SystemScheduler.Config CreateSystemSchedulerConfig()
    {
        var config = new SystemScheduler.Config();

        config.Register<WindowSystem>();

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
        config.Register<ClearSwapChainSystem>((_, system) => system.ClearColor = Vector4.Zero);
        {
            config.Register<CameraProjectionSystem>();
            config.Register<SpriteBatchSystem>();

            // World RT
            config.Register<WorldRenderTextureSystem>();

            config.Register<TilemapRenderSystem>(); // Todo This system causes Vulkan validation errors
            config.Register<SpriteRenderSystem>(); // Todo This system causes Vulkan validation errors

            // Main RT
            config.Register<BloomSystem>(); // Todo This system causes Vulkan validation errors

            config.Register<UseSwapChainAsRenderTargetSystem>();

            config.Register<ToneMappingSystem>(); // Todo This system causes Vulkan validation errors

            config.Register<RenderWorldToMainSystem>();

            config.Register<AvaloniaRenderSystem>();
            config.Register<AvaloniaCopyTextureSystem>((container, system) =>
            {
                var renderSystem = container.Resolve<AvaloniaRenderSystem>();
                var resourceManager = container.Resolve<IResourceManager>();

                system.GetSourceTexture = () =>
                {
                    return renderSystem.TopLevel.Impl.Surface.Texture;
                };

                system.ScreenShader = resourceManager.GetResource(RenderingMod.ScreenShader);
                system.PassthroughShader = resourceManager.GetResource(RenderingMod.PassthroughShader);
            });

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
