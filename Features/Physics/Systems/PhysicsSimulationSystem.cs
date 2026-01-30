using System.Numerics;
using Exanite.Core.Runtime;
using Exanite.Engine.Ecs.Queries;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Lifecycles.Components;
using Exanite.Engine.Timing;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Exanite.Myriad.Ecs;
using World = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris.Features.Physics.Systems;

public partial class PhysicsSimulationSystem : EngineSystem, IStartSystem, IFrameUpdateSystem, IFrameCleanupSystem
{
    private readonly World physicsWorld;
    private readonly Time time;

    public PhysicsSimulationSystem(World physicsWorld, Time time)
    {
        this.physicsWorld = physicsWorld;
        this.time = time;
    }

    public void Start()
    {
        physicsWorld.Gravity = Vector2.UnitY * 4f;
    }

    public void FrameUpdate()
    {
        AddRigidbodiesQuery();

        SyncTransformsToPhysicsQuery();
        SyncVelocitiesToPhysicsQuery();
        {
            SimulatePhysicsWorld();
        }
        SyncTransformsFromPhysicsQuery();
        SyncVelocitiesFromPhysicsQuery();
    }

    public void FrameCleanup()
    {
        RemoveRigidbodiesQuery();
    }

    private void SimulatePhysicsWorld()
    {
        physicsWorld.Step(time.DeltaTime);
    }

    [Query]
    private void AddRigidbodies(Entity entity, ref CRigidbody rigidbody)
    {
        if (rigidbody.Body.World == null)
        {
            physicsWorld.Add(rigidbody.Body);
            rigidbody.Body.Tag = new Box<Entity>(entity);
        }
    }

    [Query]
    private void SyncTransformsToPhysics(ref CTransform transform, ref CRigidbody rigidbody)
    {
        rigidbody.Body.Position = transform.Position;
        rigidbody.Body.Rotation = transform.Rotation;
    }

    [Query]
    private void SyncVelocitiesToPhysics(ref CVelocity velocity, ref CRigidbody rigidbody)
    {
        rigidbody.Body.LinearVelocity = velocity.Velocity;
    }

    [Query]
    private void SyncAngularVelocitiesToPhysics(ref CAngularVelocity angularVelocity, ref CRigidbody rigidbody)
    {
        rigidbody.Body.AngularVelocity = angularVelocity.AngularVelocity;
    }

    [Query]
    private void SyncTransformsFromPhysics(ref CTransform transform, ref CRigidbody rigidbody)
    {
        transform.Position = rigidbody.Body.Position;
        transform.Rotation = rigidbody.Body.Rotation;
    }

    [Query]
    private void SyncVelocitiesFromPhysics(ref CVelocity velocity, ref CRigidbody rigidbody)
    {
        velocity.Velocity = rigidbody.Body.LinearVelocity;
    }

    [Query]
    private void SyncAngularVelocitiesFromPhysics(ref CAngularVelocity angularVelocity, ref CRigidbody rigidbody)
    {
        angularVelocity.AngularVelocity = rigidbody.Body.AngularVelocity;
    }

    [Query]
    [QueryInclude<CDestroyed>]
    private void RemoveRigidbodies(ref CRigidbody rigidbody)
    {
        var body = rigidbody.Body;
        if (body.World != null)
        {
            body.World.Remove(body);
        }
    }
}
