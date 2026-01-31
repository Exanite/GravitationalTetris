using System.Numerics;
using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Physics.Components;

public struct EcsVelocity : IComponent
{
    public Vector2 Velocity;

    public EcsVelocity() : this(Vector2.Zero) {}

    public EcsVelocity(Vector2 velocity)
    {
        Velocity = velocity;
    }
}
