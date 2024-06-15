using Exanite.Engine.Ecs.Systems;
using World = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris.Features.Physics.Systems;

public class PhysicsContactSystem : ISetupSystem
{
    private readonly World physicsWorld;

    public PhysicsContactSystem(World physicsWorld)
    {
        this.physicsWorld = physicsWorld;
    }

    public void Setup()
    {
        // physicsWorld.ContactManager.BeginContact = (contact) =>
        // {
        //     var bodyA = contact.FixtureA.Body;
        //     var bodyB = contact.FixtureB.Body;
        //
        //     if (bodyA.Tag is not BoxedObject<EntityReference> refA || bodyB.Tag is not BoxedObject<EntityReference> refB)
        //     {
        //         return contact.Enabled;
        //     }
        //
        //     if (!refA.Value.Entity.IsAlive() || !refB.Value.Entity.IsAlive())
        //     {
        //         return contact.Enabled;
        //     }
        //
        //     if (refA.Value.Entity.Has<PlayerComponent>() || !refB.Value.Entity.Has<TetrisBlockComponent>())
        //     {
        //         contact.FixtureB.Body.LinearVelocity = Vector2.UnitY * 0.5f;
        //
        //         return contact.Enabled;
        //     }
        //
        //     return contact.Enabled;
        // };
    }
}
