using Myriad.ECS;
using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.GravitationalTetris.Features.Physics.Components;

public struct ComponentRigidbody : IComponent
{
    public Body Body;

    public ComponentRigidbody(Body body)
    {
        Body = body;
    }
}
