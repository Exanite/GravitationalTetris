using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Core.Utilities;
using Exanite.WarGames.Features.Characters.Components;
using Exanite.WarGames.Features.Physics.Components;
using Exanite.WarGames.Features.Players.Components;
using Exanite.WarGames.Features.Time;
using Exanite.WarGames.Features.Transforms.Components;
using Exanite.WarGames.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Exanite.WarGames.Features.Players.Systems;

public partial class PlayerControllerSystem : EcsSystem, IUpdateSystem
{
    private readonly GameInputData input;
    private readonly GameTimeData time;

    public PlayerControllerSystem(GameInputData input, GameTimeData time)
    {
        this.input = input;
        this.time = time;
    }

    public void Update()
    {
        UpdateMovementQuery(World);
    }

    [Query]
    [All<PlayerComponent>]
    private void UpdateMovement(ref VelocityComponent velocity, ref PlayerMovement movement, ref TransformComponent transform, ref MovementSpeedComponent movementSpeed)
    {
        var movementInput = Vector2.Zero;
        movementInput.X -= input.Current.Keyboard.IsKeyDown(Keys.A) ? 1 : 0;
        movementInput.X += input.Current.Keyboard.IsKeyDown(Keys.D) ? 1 : 0;
        movementInput.Y -= input.Current.Keyboard.IsKeyDown(Keys.W) ? 1 : 0;
        movementInput.Y += input.Current.Keyboard.IsKeyDown(Keys.S) ? 1 : 0;
        movementInput = movementInput.AsNormalizedSafe();

        velocity.Velocity.X = MathUtility.SmoothDamp(velocity.Velocity.X, movementInput.X * movementSpeed.MovementSpeed, movement.SmoothTime, time.DeltaTime, ref movement.SmoothVelocity.X);
    }
}
