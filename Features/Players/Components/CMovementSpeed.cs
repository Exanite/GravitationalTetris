using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Players.Components;

public struct CMovementSpeed : IComponent
{
    public float MovementSpeed;

    public CMovementSpeed(float movementSpeed)
    {
        MovementSpeed = movementSpeed;
    }
}
