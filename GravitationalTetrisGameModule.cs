using System;
using Autofac;
using Exanite.Engine.Avalonia.Systems;
using Exanite.Engine.Ecs.Scheduling;
using Exanite.Engine.Framework;
using Exanite.Engine.Graphics;
using Exanite.Engine.Graphics.Systems;
using Exanite.Engine.Modding;
using Exanite.Engine.Resources.Systems;
using Exanite.Engine.Timing.Systems;
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

        scheduler.DefaultGroup.RegisterSystem<FpsCounterSystem>();
        scheduler.DefaultGroup.RegisterSystem<ResourceHotReloadSystem>();

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
            scheduler.DefaultGroup.RegisterSystem<TetrisUiSystem>();

            // Render
            scheduler.DefaultGroup.RegisterSystem<AvaloniaDisplaySystem>();
            scheduler.DefaultGroup.RegisterSystem<RendererSystem>();

            // Main RT
            // scheduler.DefaultGroup.RegisterSystem<BloomSystem>();
            // scheduler.DefaultGroup.RegisterSystem<ToneMappingSystem>();

            scheduler.DefaultGroup.RegisterSystem<PresentSwapchainSystem>();
        }
    }
}
