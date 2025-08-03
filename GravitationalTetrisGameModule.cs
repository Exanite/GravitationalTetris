using System;
using Autofac;
using Exanite.Engine.Avalonia.Systems;
using Exanite.Engine.Ecs.Scheduling;
using Exanite.Engine.Framework;
using Exanite.Engine.Graphics;
using Exanite.Engine.Graphics.Systems;
using Exanite.Engine.Modding;
using Exanite.Engine.Windowing;
using Exanite.GravitationalTetris.Features;
using Exanite.GravitationalTetris.Features.Audio.Systems;
using Exanite.GravitationalTetris.Features.Cameras.Systems;
using Exanite.GravitationalTetris.Features.Physics.Systems;
using Exanite.GravitationalTetris.Features.Players.Systems;
using Exanite.GravitationalTetris.Features.Rendering.Systems;
using Exanite.GravitationalTetris.Features.Tetris.Systems;
using Exanite.GravitationalTetris.Features.Tiles;
using Exanite.GravitationalTetris.Features.Tiles.Systems;
using Exanite.ResourceManagement;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris;

public class GravitationalTetrisGameModule : GameModule
{
    protected override void OnConfigureContainer(ContainerBuilder builder)
    {
        base.OnConfigureContainer(builder);

        // Misc
        builder.RegisterType<Random>().InstancePerDependency();

        // Windowing
        builder.RegisterType<Window>().SingleInstance();
        builder.RegisterType<Swapchain>().AsSelf().As<IGraphicsCommandBufferProvider>().SingleInstance();
        builder.Register(_ => new SwapchainDesc()).SingleInstance();
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

    protected override void OnConfigureSystems(SystemScheduler scheduler)
    {
        base.OnConfigureSystems(scheduler);

        scheduler.DefaultGroup.RegisterSystem<CreateEntitiesSystem>();

        scheduler.DefaultGroup.RegisterSystem<PhysicsSimulationSystem>();

        scheduler.DefaultGroup.RegisterSystem<PlayerControllerSystem>();
        scheduler.DefaultGroup.RegisterSystem<TetrisSystem>();
        scheduler.DefaultGroup.RegisterSystem<TilemapColliderSystem>();

        scheduler.DefaultGroup.RegisterSystem<FmodAudioSystem>();

        {
            scheduler.DefaultGroup.RegisterSystem<AcquireSwapchainSystem>();

            // Update data
            scheduler.DefaultGroup.RegisterSystem<CameraProjectionSystem>();

            // Render
            scheduler.DefaultGroup.RegisterSystem<RendererSystem>();

            // World RT

            // scheduler.DefaultGroup.RegisterSystem<TilemapRenderSystem>();
            // scheduler.DefaultGroup.RegisterSystem<SpriteRenderSystem>();

            // Main RT
            // scheduler.DefaultGroup.RegisterSystem<BloomSystem>();
            // scheduler.DefaultGroup.RegisterSystem<ToneMappingSystem>();

            // scheduler.DefaultGroup.RegisterSystem<RenderWorldToMainSystem>();

            scheduler.DefaultGroup.RegisterSystem<AvaloniaDisplaySystem>();
            scheduler.DefaultGroup.RegisterSystem<CopyColorTextureSystem>().OnInstantiated((container, system) =>
            {
                var uiSystem = container.Resolve<TetrisUiSystem>();
                var swapchain = container.Resolve<Swapchain>();

                system.GetColorSource = () => uiSystem.UiRoot.Texture;
                system.GetColorTarget = () => swapchain.Texture;
            });

            scheduler.DefaultGroup.RegisterSystem<TetrisUiSystem>();

            scheduler.DefaultGroup.RegisterSystem<PresentSwapchainSystem>();
        }
    }
}
