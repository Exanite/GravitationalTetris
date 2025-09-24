using System;
using Autofac;
using Exanite.Engine.Ecs.Scheduling;
using Exanite.Engine.Framework;
using Exanite.Engine.Modding;
using Exanite.Engine.Resources.Systems;
using Exanite.Engine.Timing.Systems;
using Exanite.Engine.Windowing;
using Exanite.GravitationalTetris.Features;
using Exanite.GravitationalTetris.Features.Audio.Modules;
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
    protected override void OnDeclareRelations(GameModuleRelations relations)
    {
        base.OnDeclareRelations(relations);

        relations.RegisterSubmodule<AudioModule>();
    }

    protected override void OnConfigureContainer(ContainerBuilder builder)
    {
        base.OnConfigureContainer(builder);

        // Misc
        builder.RegisterType<Random>().InstancePerDependency();

        // Windowing
        builder.Register(ctx =>
        {
            var resourceManager = ctx.Resolve<IResourceManager>();

            return new WindowSettings()
            {
                Name = "Gravitational Tetris",
                Icon = resourceManager.GetResource(GravitationalTetrisResources.WindowIcon).Value,
            };
        }).SingleInstance();

        // Shared data
        builder.RegisterType<GameTilemapData>().SingleInstance();

        // Physics
        builder.RegisterType<PhysicsWorld>().SingleInstance();

        // Resources
        builder.RegisterFolderFileSystem("GravitationalTetris", "/", "");
    }

    protected override void OnConfigureSystems(SystemScheduler scheduler)
    {
        base.OnConfigureSystems(scheduler);

        scheduler.DefaultGroup.RegisterSystem<FpsCounterLogSystem>();
        scheduler.DefaultGroup.RegisterSystem<ResourceHotReloadSystem>();

        scheduler.DefaultGroup.RegisterSystem<CreateEntitiesSystem>();

        scheduler.DefaultGroup.RegisterSystem<PhysicsSimulationSystem>();

        scheduler.DefaultGroup.RegisterSystem<PlayerControllerSystem>();
        scheduler.DefaultGroup.RegisterSystem<TetrisSystem>();
        scheduler.DefaultGroup.RegisterSystem<TilemapColliderSystem>();

        scheduler.DefaultGroup.RegisterSystem<AudioSystem>();

        // Rendering
        {
            // Update data
            scheduler.DefaultGroup.RegisterSystem<CameraProjectionSystem>();
            scheduler.DefaultGroup.RegisterSystem<TetrisUiSystem>();

            // Render
            scheduler.DefaultGroup.RegisterSystem<RendererSystem>();
        }
    }
}
