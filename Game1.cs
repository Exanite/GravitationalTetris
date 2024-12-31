using System;
using Autofac;
using Exanite.Engine.Avalonia.Systems;
using Exanite.Engine.Ecs.Scheduling;
using Exanite.Engine.Framework;
using Exanite.Engine.Inputs.Systems;
using Exanite.Engine.Lifecycles.Systems;
using Exanite.Engine.Rendering;
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
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris;

public class Game1 : EngineGame
{
    public Game1(EngineSettings settings) : base(settings) {}

    protected override void Register(ContainerBuilder builder)
    {
        base.Register(builder);

        // Misc
        builder.RegisterType<Random>().InstancePerDependency();

        // Windowing
        builder.RegisterType<Window>().SingleInstance();
        builder.RegisterType<Swapchain>().SingleInstance();
        builder.Register(ctx =>
        {
            var resourceManager = ctx.Resolve<IResourceManager>();

            return new WindowSettings()
            {
                Name = "Gravitational Tetris",
                Icon = resourceManager.GetResource(BaseMod.WindowIcon).Value,
            };
        }).SingleInstance();

        // Shared data
        builder.RegisterType<GameTilemapData>().SingleInstance();

        // Physics
        builder.RegisterType<PhysicsWorld>().SingleInstance();

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

        {
            // Rendering resources
            config.Register<RenderingResourcesSystem>();

            // Update data
            config.Register<CameraProjectionSystem>();
            config.Register<SpriteBatchSystem>();

            // World RT
            config.Register<TilemapRenderSystem>();
            config.Register<SpriteRenderSystem>();

            // Main RT
            config.Register<BloomSystem>();
            config.Register<ToneMappingSystem>();

            config.Register<RenderWorldToMainSystem>();

            config.Register<SimpleAvaloniaSystem>();
            config.Register<AvaloniaCopyTextureSystem>().OnInstantiated((container, system) =>
            {
                var renderSystem = container.Resolve<SimpleAvaloniaSystem>();
                var resourceManager = container.Resolve<IResourceManager>();

                system.GetColorSource = () =>
                {
                    return renderSystem.Instance.Texture;
                };

                system.VertexModule = resourceManager.GetResource(RenderingMod.ScreenVertexModule);
                system.FragmentModule = resourceManager.GetResource(RenderingMod.PassthroughFragmentModule);
            });

            config.Register<TetrisUiSystem>();

            config.Register<PresentSwapchainSystem>();
        }

        config.Register<RemoveDestroyedSystem>();
    }
}
