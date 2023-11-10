using Exanite.WarGames.Systems;
using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.WarGames.Features.Physics.Systems;

public class PhysicsContactSystem : ICallbackSystem
{
    private readonly World physicsWorld;

    public PhysicsContactSystem(World physicsWorld)
    {
        this.physicsWorld = physicsWorld;
    }

    public void RegisterCallbacks()
    {
        physicsWorld.ContactManager.BeginContact = (contact) =>
        {
            return true;
        };
    }
}
