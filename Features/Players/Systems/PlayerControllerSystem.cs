using System;
using Exanite.Core.Utilities;
using Exanite.Engine.Ecs.Queries;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Inputs.Actions;
using Exanite.Engine.Inputs.Systems;
using Exanite.Engine.Time;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Players.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using SDL2;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris.Features.Players.Systems;

public partial class PlayerControllerSystem : EcsSystem, ISetupSystem, IUpdateSystem
{
    private bool isGravityDown = true;

    private IInputAction<float> movementAction = null!;

    private readonly PhysicsWorld physicsWorld;
    private readonly InputActionSystem input;
    private readonly SimulationTime time;

    public PlayerControllerSystem(PhysicsWorld physicsWorld, InputActionSystem input, SimulationTime time)
    {
        this.physicsWorld = physicsWorld;
        this.input = input;
        this.time = time;
    }

    public void Setup()
    {
        movementAction = input.RegisterAction(() =>
        {
            var action = new CompositeFloatInputAction();
            action.AddPositive(new ButtonInputAction(SDL.SDL_Scancode.SDL_SCANCODE_D));
            action.AddNegative(new ButtonInputAction(SDL.SDL_Scancode.SDL_SCANCODE_A));

            return action;
        });
    }

    public void Update()
    {
        UpdateMovementQuery(World);
        ClampPlayerVelocityQuery(World);
    }

    public void FlipGravity()
    {
        SetIsGravityDown(!isGravityDown);
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
        velocity.Velocity.X = MathUtility.SmoothDamp(velocity.Velocity.X, movementAction.CurrentValue * movementSpeed.MovementSpeed, movement.SmoothTime, time.DeltaTime, ref movement.SmoothVelocity.X);
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
