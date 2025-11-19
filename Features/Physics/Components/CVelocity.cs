using System.Numerics;
using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Physics.Components;

public struct CVelocity : IComponent
{
    public Vector2 Velocity;

    public CVelocity() : this(Vector2.Zero) {}

    public CVelocity(Vector2 velocity)
    {
        Velocity = velocity;
    }
}
