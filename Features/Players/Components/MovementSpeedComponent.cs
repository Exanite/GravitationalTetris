using Myriad.ECS;

namespace Exanite.GravitationalTetris.Features.Players.Components;

public struct MovementSpeedComponent : IComponent
{
    public float MovementSpeed;

    public MovementSpeedComponent(float movementSpeed)
    {
        MovementSpeed = movementSpeed;
    }
}
