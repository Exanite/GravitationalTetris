using System;
using Exanite.Core.Utilities;
using Exanite.Engine.Ecs.Queries;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Inputs;
using Exanite.Engine.Inputs.Actions;
using Exanite.Engine.Timing;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Players.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.GravitationalTetris.Features.Players.Systems;

public partial class PlayerControllerSystem : GameSystem, ISetupSystem, IFrameUpdateSystem
{
    private bool isGravityDown = true;

    private IInputAction<float> movementAction = null!;

    private readonly PhysicsWorld physicsWorld;
    private readonly InputActionManager input;
    private readonly ITime time;

    public PlayerControllerSystem(PhysicsWorld physicsWorld, InputActionManager input, ITime time)
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
            action.AddPositive(new ButtonInputAction(KeyCode.D));
            action.AddNegative(new ButtonInputAction(KeyCode.A));

            return action;
        });
    }

    public void FrameUpdate()
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
    [Include<ComponentPlayer>]
    private void UpdateMovement(ref ComponentVelocity velocity, ref ComponentPlayerMovement movement, ref ComponentMovementSpeed movementSpeed)
    {
        velocity.Velocity.X = MathUtility.SmoothDamp(velocity.Velocity.X, movementAction.CurrentValue * movementSpeed.MovementSpeed, movement.SmoothTime, time.DeltaTime, ref movement.SmoothVelocity.X);
    }

    [Query]
    [Include<ComponentPlayer>]
    private void ZeroVelocityY(ref ComponentVelocity velocity)
    {
        velocity.Velocity.Y = 0;
    }

    [Query]
    [Include<ComponentPlayer>]
    private void SetPlayerRotation(ref ComponentTransform transform)
    {
        transform.Rotation = isGravityDown ? 0 : float.Pi;
    }

    [Query]
    [Include<ComponentPlayer>]
    private void ClampPlayerVelocity(ref ComponentVelocity velocity)
    {
        velocity.Velocity.Y = Math.Clamp(velocity.Velocity.Y, -4, 4);
    }
}
