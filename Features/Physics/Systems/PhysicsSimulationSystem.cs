using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using CommunityToolkit.HighPerformance;
using Exanite.Extraction.Features.Physics.Components;
using Exanite.Extraction.Features.Time;
using Exanite.Extraction.Features.Transforms.Components;
using Exanite.Extraction.Systems;
using Microsoft.Xna.Framework;
using World = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.Extraction.Features.Physics.Systems;

public partial class PhysicsSimulationSystem : EcsSystem, IStartSystem, IUpdateSystem
{
    private readonly World physicsWorld;
    private readonly GameTimeData time;

    public PhysicsSimulationSystem(World physicsWorld, GameTimeData time)
    {
        this.physicsWorld = physicsWorld;
        this.time = time;
    }

    public void Start()
    {
        physicsWorld.Gravity = Vector2.Zero;
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
            rigidbody.Body.Tag = (Box<EntityReference>)entity.Reference();
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
}
