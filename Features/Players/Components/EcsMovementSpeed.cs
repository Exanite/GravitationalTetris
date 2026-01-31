using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Players.Components;

public struct EcsMovementSpeed : IComponent
{
    public float MovementSpeed;

    public EcsMovementSpeed(float movementSpeed)
    {
        MovementSpeed = movementSpeed;
    }
}
