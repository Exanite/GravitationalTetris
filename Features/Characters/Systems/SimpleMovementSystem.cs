using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Extraction.Features.Characters.Components;
using Exanite.Extraction.Features.Physics.Components;
using Exanite.Extraction.Systems;

namespace Exanite.Extraction.Features.Characters.Systems;

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
