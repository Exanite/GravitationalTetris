using System;
using System.Numerics;
using Autofac;
using Exanite.Engine.Avalonia.Systems;
using Exanite.Engine.Ecs.Scheduling;
using Exanite.Engine.Framework;
using Exanite.Engine.Inputs.Systems;
using Exanite.Engine.Lifecycles.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Rendering.Systems;
using Exanite.Engine.Timing.Systems;
using Exanite.Engine.Windowing;
using Exanite.Engine.Windowing.Systems;
using Exanite.GravitationalTetris.Features;
using Exanite.GravitationalTetris.Features.Audio.Systems;
using Exanite.GravitationalTetris.Features.Cameras.Systems;
using Exanite.GravitationalTetris.Features.Physics.Systems;
using Exanite.GravitationalTetris.Features.Players.Systems;
using Exanite.GravitationalTetris.Features.Rendering.Systems;
using Exanite.GravitationalTetris.Features.Sprites.Systems;
using Exanite.GravitationalTetris.Features.Tetris.Systems;
using Exanite.GravitationalTetris.Features.Tiles;
using Exanite.GravitationalTetris.Features.Tiles.Systems;
using Exanite.ResourceManagement;
using Myriad.ECS.Worlds;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris;

public class Game1 : EngineGame
{
    public Game1(EngineSettings settings) : base(settings) {}

    protected override void Register(ContainerBuilder builder)
    {
        base.Register(builder);

        // Game
        builder.RegisterInstance(this).SingleInstance();

        // Misc
        builder.RegisterType<Random>().InstancePerDependency();

        // Windowing
        builder.Register(ctx =>
        {
            var resourceManager = ctx.Resolve<IResourceManager>();

            return new WindowSettings()
            {
                Name = "Gravitational Tetris",
                Icon = resourceManager.GetResource(BaseMod.WindowIcon).Value,
            };
        }).SingleInstance();

        builder.RegisterType<Window>().SingleInstance();

        // Rendering
        builder.RegisterType<RendererContext>().SingleInstance();
        builder.Register(_ => new RendererContextSettings
            {
                EnableValidation = false, // Todo Enable
            })
            .SingleInstance();
        builder.RegisterType<SwapChain>().SingleInstance();

        // Shared data
        builder.RegisterType<GameTilemapData>().SingleInstance();

        // Physics
        builder.RegisterType<PhysicsWorld>().SingleInstance();

        // ECS world
        builder.Register(_ => new WorldBuilder().Build()).SingleInstance();

        // Resources
        builder.RegisterFolderFileSystem("GravitationalTetris", "/Base/", "Base");
        builder.RegisterFolderFileSystem("GravitationalTetris", "/Winter/", "Winter/Content");
        builder.RegisterFolderFileSystem("GravitationalTetris", "/Base/", "Winter/Overrides/Base");
        builder.RegisterFolderFileSystem("GravitationalTetris", "/Rendering/", "Rendering");
    }

    protected override void ConfigureSystemScheduler(SystemSchedulerConfig config)
    {
        base.ConfigureSystemScheduler(config);

        config.Register<WindowSystem>();

        config.Register<SimpleTimeSystem>();
        config.Register<InputSystem>();
        config.Register<InputActionSystem>();

        config.Register<CreateEntitiesSystem>();

        config.Register<PhysicsSimulationSystem>();

        config.Register<PlayerControllerSystem>();
        config.Register<TetrisSystem>();
        config.Register<TilemapColliderSystem>();

        config.Register<FmodAudioSystem>();

        config.Register<ResizeSwapChainSystem>();
        config.Register<ClearSwapChainSystem>().OnInstantiated((_, system) => system.ClearColor = Vector4.Zero);
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

            config.Register<SimpleAvaloniaSystem>();
            config.Register<AvaloniaCopyTextureSystem>().OnInstantiated((container, system) =>
            {
                var renderSystem = container.Resolve<SimpleAvaloniaSystem>();
                var resourceManager = container.Resolve<IResourceManager>();

                system.GetSourceTexture = () =>
                {
                    return renderSystem.Instance.Texture;
                };

                system.VShader = resourceManager.GetResource(RenderingMod.ScreenShader);
                system.PShader = resourceManager.GetResource(RenderingMod.PassthroughShader);
            });

            config.Register<TetrisUiSystem>();
        }
        config.Register<PresentSwapChainSystem>();

        config.Register<RemoveDestroyedSystem>();
    }
}
