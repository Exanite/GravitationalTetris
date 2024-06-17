using System.Numerics;
using Myriad.ECS;

namespace Exanite.GravitationalTetris.Features.Players.Components;

public struct PlayerMovement : IComponent
{
    public required float SmoothTime;
    public Vector2 SmoothVelocity;
}
