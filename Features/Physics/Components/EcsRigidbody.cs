using Exanite.Myriad.Ecs;
using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.GravitationalTetris.Features.Physics.Components;

public struct EcsRigidbody : IComponent
{
    public Body Body;

    public EcsRigidbody(Body body)
    {
        Body = body;
    }
}
