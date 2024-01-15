using System;
using System.Numerics;
using Exanite.Core.Utilities;
using Exanite.Ecs.Systems;
using Exanite.Engine.Inputs;
using Exanite.Engine.Time;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Players.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Flecs.NET.Core;
using SDL2;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris.Features.Players.Systems;

public class PlayerControllerSystem : EcsSystem, ISetupSystem, IUpdateSystem
{
    private bool isGravityDown = true;

    private Query updateMovementQuery;
    private Query zeroVelocityYQuery;
    private Query setPlayerRotationQuery;
    private Query clampPlayerVelocityQuery;

    private readonly PhysicsWorld physicsWorld;
    private readonly Input input;
    private readonly SimulationTime time;

    public PlayerControllerSystem(PhysicsWorld physicsWorld, Input input, SimulationTime time)
    {
        this.physicsWorld = physicsWorld;
        this.input = input;
        this.time = time;
    }

    public void Setup()
    {
        updateMovementQuery = World.Query(World.FilterBuilder<PlayerComponent, VelocityComponent, PlayerMovement, MovementSpeedComponent>());
        zeroVelocityYQuery = World.Query(World.FilterBuilder<PlayerComponent, VelocityComponent>());
        setPlayerRotationQuery = World.Query(World.FilterBuilder<PlayerComponent, TransformComponent>());
        clampPlayerVelocityQuery = World.Query(World.FilterBuilder<PlayerComponent, VelocityComponent>());
    }

    public void Update()
    {
        updateMovementQuery.Each<VelocityComponent, PlayerMovement, MovementSpeedComponent>(UpdateMovement);

        if (input.GetKeyDown(SDL.SDL_Scancode.SDL_SCANCODE_SPACE))
        {
            SetIsGravityDown(!isGravityDown);
        }

        clampPlayerVelocityQuery.Each<VelocityComponent>(ClampPlayerVelocity);
    }

    public void SetIsGravityDown(bool isGravityDown)
    {
        this.isGravityDown = isGravityDown;

        var gravity = physicsWorld.Gravity;
        gravity.Y = Math.Abs(gravity.Y) * (isGravityDown ? 1 : -1);
        physicsWorld.Gravity = gravity;

        zeroVelocityYQuery.Each<VelocityComponent>(ZeroVelocityY);
        setPlayerRotationQuery.Each<TransformComponent>(SetPlayerRotation);
    }

    private void UpdateMovement(ref VelocityComponent velocity, ref PlayerMovement movement, ref MovementSpeedComponent movementSpeed)
    {
        var movementInput = Vector2.Zero;
        movementInput.X -= input.GetKey(SDL.SDL_Scancode.SDL_SCANCODE_A) ? 1 : 0;
        movementInput.X += input.GetKey(SDL.SDL_Scancode.SDL_SCANCODE_D) ? 1 : 0;
        movementInput.Y -= input.GetKey(SDL.SDL_Scancode.SDL_SCANCODE_W) ? 1 : 0;
        movementInput.Y += input.GetKey(SDL.SDL_Scancode.SDL_SCANCODE_S) ? 1 : 0;
        movementInput = movementInput.AsNormalizedSafe();

        velocity.Velocity.X = MathUtility.SmoothDamp(velocity.Velocity.X, movementInput.X * movementSpeed.MovementSpeed, movement.SmoothTime, time.DeltaTime, ref movement.SmoothVelocity.X);
    }

    private void ZeroVelocityY(ref VelocityComponent velocity)
    {
        velocity.Velocity.Y = 0;
    }

    private void SetPlayerRotation(ref TransformComponent transform)
    {
        transform.Rotation = isGravityDown ? 0 : float.Pi;
    }

    private void ClampPlayerVelocity(ref VelocityComponent velocity)
    {
        velocity.Velocity.Y = Math.Clamp(velocity.Velocity.Y, -4, 4);
    }
}
