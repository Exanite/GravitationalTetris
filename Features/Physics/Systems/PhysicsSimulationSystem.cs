using System.Numerics;
using Exanite.Core.HighPerformance;
using Exanite.Ecs.Systems;
using Exanite.Engine.Lifecycles.Components;
using Exanite.Engine.Time;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Flecs.NET.Core;
using World = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris.Features.Physics.Systems;

public class PhysicsSimulationSystem : EcsSystem, ISetupSystem, IStartSystem, IUpdateSystem, ICleanupSystem
{
    private Query removeRigidbodiesQuery;

    private readonly World physicsWorld;
    private readonly SimulationTime time;

    public PhysicsSimulationSystem(World physicsWorld, SimulationTime time)
    {
        this.physicsWorld = physicsWorld;
        this.time = time;
    }

    public void Setup()
    {
        removeRigidbodiesQuery = World.Query(World.FilterBuilder<RigidbodyComponent, DestroyedComponent>());
    }

    public void Start()
    {
        physicsWorld.Gravity = Vector2.UnitY * 4f;
    }

    public void Update()
    {
        World.Each<RigidbodyComponent>(AddRigidbodies);

        World.Each<TransformComponent, RigidbodyComponent>(SyncTransformsToPhysics);
        World.Each<VelocityComponent, RigidbodyComponent>(SyncVelocitiesToPhysics);
        {
            SimulatePhysicsWorld();
        }
        World.Each<TransformComponent, RigidbodyComponent>(SyncTransformsFromPhysics);
        World.Each<VelocityComponent, RigidbodyComponent>(SyncVelocitiesFromPhysics);
    }

    public void Cleanup()
    {
        // Todo Validate that this is correct
        removeRigidbodiesQuery.Each<RigidbodyComponent>(RemoveDestroyedRigidbodies);
    }

    private void SimulatePhysicsWorld()
    {
        physicsWorld.Step(time.DeltaTime);
    }

    private void AddRigidbodies(Entity entity, ref RigidbodyComponent rigidbody)
    {
        if (rigidbody.Body.World == null)
        {
            physicsWorld.Add(rigidbody.Body);
            rigidbody.Body.Tag = new BoxedValue<Entity>(entity);
        }
    }

    private void SyncTransformsToPhysics(ref TransformComponent transform, ref RigidbodyComponent rigidbody)
    {
        rigidbody.Body.Position = transform.Position;
        rigidbody.Body.Rotation = transform.Rotation;
    }

    private void SyncVelocitiesToPhysics(ref VelocityComponent velocity, ref RigidbodyComponent rigidbody)
    {
        rigidbody.Body.LinearVelocity = velocity.Velocity;
    }

    private void SyncAngularVelocitiesToPhysics(ref AngularVelocityComponent angularVelocity, ref RigidbodyComponent rigidbody)
    {
        rigidbody.Body.AngularVelocity = angularVelocity.AngularVelocity;
    }

    private void SyncTransformsFromPhysics(ref TransformComponent transform, ref RigidbodyComponent rigidbody)
    {
        transform.Position = rigidbody.Body.Position;
        transform.Rotation = rigidbody.Body.Rotation;
    }

    private void SyncVelocitiesFromPhysics(ref VelocityComponent velocity, ref RigidbodyComponent rigidbody)
    {
        velocity.Velocity = rigidbody.Body.LinearVelocity;
    }

    private void SyncAngularVelocitiesFromPhysics(ref AngularVelocityComponent angularVelocity, ref RigidbodyComponent rigidbody)
    {
        angularVelocity.AngularVelocity = rigidbody.Body.AngularVelocity;
    }

    private void RemoveDestroyedRigidbodies(ref RigidbodyComponent rigidbody)
    {
        var body = rigidbody.Body;
        if (body.World != null)
        {
            body.World.Remove(body);
        }
    }
}
