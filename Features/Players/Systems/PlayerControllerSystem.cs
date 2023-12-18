using System;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Core.Utilities;
using Exanite.Ecs.Systems;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Players.Components;
using Exanite.GravitationalTetris.Features.Time;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris.Features.Players.Systems;

public partial class PlayerControllerSystem : EcsSystem, IUpdateSystem
{
    private bool isGravityDown = true;

    private readonly GameInputData input;
    private readonly GameTimeData time;
    private readonly PhysicsWorld physicsWorld;

    public PlayerControllerSystem(GameInputData input, GameTimeData time, PhysicsWorld physicsWorld)
    {
        this.input = input;
        this.time = time;
        this.physicsWorld = physicsWorld;
    }

    public void Update()
    {
        UpdateMovementQuery(World);

        if (input.Current.Keyboard.IsKeyDown(Keys.Space) && !input.Previous.Keyboard.IsKeyDown(Keys.Space))
        {
            SetIsGravityDown(!isGravityDown);
        }

        ClampPlayerVelocityQuery(World);
    }

    public void SetIsGravityDown(bool isGravityDown)
    {
        this.isGravityDown = isGravityDown;

        var gravity = physicsWorld.Gravity;
        gravity.Y = Math.Abs(gravity.Y) * (isGravityDown ? 1 : -1);
        physicsWorld.Gravity = gravity;

        ZeroVelocityYQuery(World);
        SetPlayerRotationQuery(World);
    }

    [Query]
    [All<PlayerComponent>]
    private void UpdateMovement(ref VelocityComponent velocity, ref PlayerMovement movement, ref MovementSpeedComponent movementSpeed)
    {
        var movementInput = Vector2.Zero;
        movementInput.X -= input.Current.Keyboard.IsKeyDown(Keys.A) ? 1 : 0;
        movementInput.X += input.Current.Keyboard.IsKeyDown(Keys.D) ? 1 : 0;
        movementInput.Y -= input.Current.Keyboard.IsKeyDown(Keys.W) ? 1 : 0;
        movementInput.Y += input.Current.Keyboard.IsKeyDown(Keys.S) ? 1 : 0;
        movementInput = movementInput.AsNormalizedSafe();

        velocity.Velocity.X = MathUtility.SmoothDamp(velocity.Velocity.X, movementInput.X * movementSpeed.MovementSpeed, movement.SmoothTime, time.DeltaTime, ref movement.SmoothVelocity.X);
    }

    [Query]
    [All<PlayerComponent>]
    private void ZeroVelocityY(ref VelocityComponent velocity)
    {
        velocity.Velocity.Y = 0;
    }

    [Query]
    [All<PlayerComponent>]
    private void SetPlayerRotation(ref TransformComponent transform)
    {
        transform.Rotation = isGravityDown ? 0 : float.Pi;
    }

    [Query]
    [All<PlayerComponent>]
    private void ClampPlayerVelocity(ref VelocityComponent velocity)
    {
        velocity.Velocity.Y = Math.Clamp(velocity.Velocity.Y, -4, 4);
    }
}
