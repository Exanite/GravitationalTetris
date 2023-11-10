using Arch.System;
using Exanite.Core.Utilities;
using Exanite.WarGames.Features.Characters.Components;
using Exanite.WarGames.Features.Physics.Components;
using Exanite.WarGames.Features.Time;
using Exanite.WarGames.Systems;

namespace Exanite.WarGames.Features.Characters.Systems;

public partial class SmoothDampMovementSystem : EcsSystem, IUpdateSystem
{
    private readonly GameTimeData time;

    public SmoothDampMovementSystem(GameTimeData time)
    {
        this.time = time;
    }

    public void Update()
    {
        UpdateQuery(World);
    }

    [Query]
    private void Update(ref VelocityComponent velocity, ref MovementSpeedComponent movementSpeed, ref SmoothDampMovementComponent movement, ref MovementDirectionComponent movementDirection)
    {
        velocity.Velocity.X = MathUtility.SmoothDamp(velocity.Velocity.X, movementDirection.Direction.X * movementSpeed.MovementSpeed, movement.SmoothTime, time.DeltaTime, ref movement.SmoothVelocity.X);
        velocity.Velocity.Y = MathUtility.SmoothDamp(velocity.Velocity.Y, movementDirection.Direction.Y * movementSpeed.MovementSpeed, movement.SmoothTime, time.DeltaTime, ref movement.SmoothVelocity.Y);
    }
}
