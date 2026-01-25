using System;
using Autofac;
using Exanite.Engine.Audio.Modules;
using Exanite.Engine.Audio.Systems;
using Exanite.Engine.Ecs.Scheduling;
using Exanite.Engine.Framework;
using Exanite.Engine.Resources;
using Exanite.Engine.Resources.Systems;
using Exanite.Engine.Timing.Systems;
using Exanite.Engine.Windowing.Modules;
using Exanite.GravitationalTetris.Features;
using Exanite.GravitationalTetris.Features.Cameras.Systems;
using Exanite.GravitationalTetris.Features.Physics.Systems;
using Exanite.GravitationalTetris.Features.Players.Systems;
using Exanite.GravitationalTetris.Features.Rendering.Systems;
using Exanite.GravitationalTetris.Features.Tetris.Systems;
using Exanite.GravitationalTetris.Features.Tiles;
using Exanite.GravitationalTetris.Features.Tiles.Systems;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris;

public class GravitationalTetrisGameModule : EngineModule
{
    protected override void OnDeclareRelations(EngineModuleRelations relations)
    {
        base.OnDeclareRelations(relations);

        relations.Submodule<AudioModule>();
    }

    protected override void OnConfigureContainer(ContainerBuilder builder)
    {
        base.OnConfigureContainer(builder);

        builder.RegisterInstance(new ResourceClassGeneratorSettings("Exanite.GravitationalTetris")).SingleInstance();
        builder.ConfigureDefaultWindow((window, _) =>
        {
            window.Name = "Gravitational Tetris";
        });

        builder.RegisterType<Random>().InstancePerDependency();
        builder.RegisterType<GameTilemapData>().SingleInstance();
        builder.RegisterType<PhysicsWorld>().SingleInstance();
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
