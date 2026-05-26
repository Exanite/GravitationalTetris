using System.Numerics;
using Exanite.Ecs;

namespace Exanite.GravitationalTetris.Features.Players.Components;

public struct EcsPlayerMovement : IEcsComponent
{
    public required float SmoothTime;
    public Vector2 SmoothVelocity;
}
