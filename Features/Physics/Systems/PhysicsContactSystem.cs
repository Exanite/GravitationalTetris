using Exanite.Ecs.Systems;
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
        //     if (bodyA.Tag is not BoxedValue<Entity> refA || bodyB.Tag is not BoxedValue<Entity> refB)
        //     {
        //         return contact.Enabled;
        //     }
        //
        //     if (!refA.Value.IsAlive() || !refB.Value.IsAlive())
        //     {
        //         return contact.Enabled;
        //     }
        //
        //     if (refA.Value.Has<PlayerComponent>() || !refB.Value.Has<TetrisBlockComponent>())
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
