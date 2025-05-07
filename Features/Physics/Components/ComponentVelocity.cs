using System.Numerics;
using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Physics.Components;

public struct ComponentVelocity : IComponent
{
    public Vector2 Velocity;

    public ComponentVelocity() : this(Vector2.Zero) {}

    public ComponentVelocity(Vector2 velocity)
    {
        Velocity = velocity;
    }
}
