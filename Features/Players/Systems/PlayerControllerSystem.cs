using System;
using System.Numerics;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Core.Utilities;
using Exanite.Ecs.Systems;
using Exanite.Engine.Inputs;
using Exanite.Engine.Time;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Players.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using SDL2;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris.Features.Players.Systems;

public partial class PlayerControllerSystem : EcsSystem, IUpdateSystem
{
    private bool isGravityDown = true;

    private readonly PhysicsWorld physicsWorld;
    private readonly Input input;
    private readonly SimulationTime time;

    public PlayerControllerSystem(PhysicsWorld physicsWorld, Input input, SimulationTime time)
    {
        this.physicsWorld = physicsWorld;
        this.input = input;
        this.time = time;
    }

    public void Update()
    {
        UpdateMovementQuery(World);

        if (input.IsPressed(SDL.SDL_Scancode.SDL_SCANCODE_SPACE))
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
        var movementInput = 0f;
        movementInput -= input.IsHeld(SDL.SDL_Scancode.SDL_SCANCODE_A) ? 1 : 0;
        movementInput += input.IsHeld(SDL.SDL_Scancode.SDL_SCANCODE_D) ? 1 : 0;

        velocity.Velocity.X = MathUtility.SmoothDamp(velocity.Velocity.X, movementInput * movementSpeed.MovementSpeed, movement.SmoothTime, time.DeltaTime, ref movement.SmoothVelocity.X);
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
