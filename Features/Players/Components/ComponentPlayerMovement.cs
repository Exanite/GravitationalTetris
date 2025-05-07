using System.Numerics;
using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Players.Components;

public struct ComponentPlayerMovement : IComponent
{
    public required float SmoothTime;
    public Vector2 SmoothVelocity;
}
