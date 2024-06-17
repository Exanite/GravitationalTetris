using System.Numerics;
using Exanite.Core.HighPerformance;
using Exanite.Engine.Ecs.Queries;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Lifecycles.Components;
using Exanite.Engine.Time;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Myriad.ECS;
using World = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris.Features.Physics.Systems;

public partial class PhysicsSimulationSystem : EcsSystem, IStartSystem, IUpdateSystem, ICleanupSystem
{
    private readonly World physicsWorld;
    private readonly SimulationTime time;

    public PhysicsSimulationSystem(World physicsWorld, SimulationTime time)
    {
        this.physicsWorld = physicsWorld;
        this.time = time;
    }

    public void Start()
    {
        physicsWorld.Gravity = Vector2.UnitY * 4f;
    }

    public void Update()
    {
        AddRigidbodiesQuery(World);

        SyncTransformsToPhysicsQuery(World);
        SyncVelocitiesToPhysicsQuery(World);
        {
            SimulatePhysicsWorld();
        }
        SyncTransformsFromPhysicsQuery(World);
        SyncVelocitiesFromPhysicsQuery(World);
    }

    public void Cleanup()
    {
        RemoveRigidbodiesQuery(World);
    }

    private void SimulatePhysicsWorld()
    {
        physicsWorld.Step(time.DeltaTime);
    }

    [Query]
    private void AddRigidbodies(Entity entity, ref RigidbodyComponent rigidbody)
    {
        if (rigidbody.Body.World == null)
        {
            physicsWorld.Add(rigidbody.Body);
            rigidbody.Body.Tag = new BoxedValue<Entity>(entity);
        }
    }

    [Query]
    private void SyncTransformsToPhysics(ref TransformComponent transform, ref RigidbodyComponent rigidbody)
    {
        rigidbody.Body.Position = transform.Position;
        rigidbody.Body.Rotation = transform.Rotation;
    }

    [Query]
    private void SyncVelocitiesToPhysics(ref VelocityComponent velocity, ref RigidbodyComponent rigidbody)
    {
        rigidbody.Body.LinearVelocity = velocity.Velocity;
    }

    [Query]
    private void SyncAngularVelocitiesToPhysics(ref AngularVelocityComponent angularVelocity, ref RigidbodyComponent rigidbody)
    {
        rigidbody.Body.AngularVelocity = angularVelocity.AngularVelocity;
    }

    [Query]
    private void SyncTransformsFromPhysics(ref TransformComponent transform, ref RigidbodyComponent rigidbody)
    {
        transform.Position = rigidbody.Body.Position;
        transform.Rotation = rigidbody.Body.Rotation;
    }

    [Query]
    private void SyncVelocitiesFromPhysics(ref VelocityComponent velocity, ref RigidbodyComponent rigidbody)
    {
        velocity.Velocity = rigidbody.Body.LinearVelocity;
    }

    [Query]
    private void SyncAngularVelocitiesFromPhysics(ref AngularVelocityComponent angularVelocity, ref RigidbodyComponent rigidbody)
    {
        angularVelocity.AngularVelocity = rigidbody.Body.AngularVelocity;
    }

    [Query]
    [Include<DestroyedComponent>]
    private void RemoveRigidbodies(ref RigidbodyComponent rigidbody)
    {
        var body = rigidbody.Body;
        if (body.World != null)
        {
            body.World.Remove(body);
        }
    }
}
