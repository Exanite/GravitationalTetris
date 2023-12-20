using System.Numerics;

namespace Exanite.GravitationalTetris.Features.Physics.Components;

public struct VelocityComponent
{
    public Vector2 Velocity;

    public VelocityComponent(Vector2 velocity)
    {
        Velocity = velocity;
    }
}
