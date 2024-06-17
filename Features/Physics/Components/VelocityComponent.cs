using System.Numerics;
using Myriad.ECS;

namespace Exanite.GravitationalTetris.Features.Physics.Components;

public struct VelocityComponent : IComponent
{
    public Vector2 Velocity;

    public VelocityComponent(Vector2 velocity)
    {
        Velocity = velocity;
    }
}
