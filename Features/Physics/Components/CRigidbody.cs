using Exanite.Myriad.Ecs;
using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.GravitationalTetris.Features.Physics.Components;

public struct CRigidbody : IComponent
{
    public Body Body;

    public CRigidbody(Body body)
    {
        Body = body;
    }
}
