using Myriad.ECS;

namespace Exanite.GravitationalTetris.Features.Players.Components;

public struct ComponentMovementSpeed : IComponent
{
    public float MovementSpeed;

    public ComponentMovementSpeed(float movementSpeed)
    {
        MovementSpeed = movementSpeed;
    }
}
