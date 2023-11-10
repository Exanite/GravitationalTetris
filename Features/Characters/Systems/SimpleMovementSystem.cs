using Arch.System;
using Exanite.WarGames.Features.Characters.Components;
using Exanite.WarGames.Features.Physics.Components;
using Exanite.WarGames.Systems;

namespace Exanite.WarGames.Features.Characters.Systems;

public partial class SimpleMovementSystem : EcsSystem, IUpdateSystem
{
    public void Update()
    {
        UpdateQuery(World);
    }

    [Query]
    [All<SimpleMovementComponent>]
    private void Update(ref VelocityComponent velocity, ref MovementSpeedComponent movementSpeed, ref MovementDirectionComponent movement)
    {
        velocity.Velocity = movement.Direction * movementSpeed.MovementSpeed;
    }
}
