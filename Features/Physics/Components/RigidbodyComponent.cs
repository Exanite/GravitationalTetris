using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.WarGames.Features.Physics.Components;

public struct RigidbodyComponent
{
    public Body Body;

    public RigidbodyComponent(Body body)
    {
        Body = body;
    }
}
