using System.Numerics;
using Exanite.Ecs;

namespace Exanite.GravitationalTetris.Features.Physics.Components;

public struct EcsVelocity : IEcsComponent
{
    public Vector2 Velocity;

    public EcsVelocity() : this(Vector2.Zero) {}

    public EcsVelocity(Vector2 velocity)
    {
        Velocity = velocity;
    }
}
