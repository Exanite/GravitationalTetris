using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Players.Components;

public struct EcsMovementSpeed : IEcsComponent
{
    public float MovementSpeed;

    public EcsMovementSpeed(float movementSpeed)
    {
        MovementSpeed = movementSpeed;
    }
}
