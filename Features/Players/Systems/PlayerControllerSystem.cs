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

public partial class PlayerControllerSystem : EngineSystem, ISetupSystem, IFrameUpdateSystem
{
    private bool isGravityDown = true;

    private IInputAction<float> movementAction = null!;

    private readonly PhysicsWorld physicsWorld;
    private readonly Input input;
    private readonly Time time;

    public PlayerControllerSystem(PhysicsWorld physicsWorld, Input input, Time time)
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
            action.AddPositive(new KeyInputAction(KeyCode.D));
            action.AddNegative(new KeyInputAction(KeyCode.A));

            return action;
        });
    }

    public void FrameUpdate()
    {
        UpdateMovementQuery();
        ClampPlayerVelocityQuery();
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

        ZeroVelocityYQuery();
        SetPlayerRotationQuery();
    }

    [Query]
    [QueryInclude<CPlayer>]
    private void UpdateMovement(ref CVelocity velocity, ref CPlayerMovement movement, ref CMovementSpeed movementSpeed)
    {
        velocity.Velocity.X = M.SmoothDamp(velocity.Velocity.X, movementAction.CurrentValue * movementSpeed.MovementSpeed, movement.SmoothTime, time.DeltaTime, ref movement.SmoothVelocity.X);
    }

    [Query]
    [QueryInclude<CPlayer>]
    private void ZeroVelocityY(ref CVelocity velocity)
    {
        velocity.Velocity.Y = 0;
    }

    [Query]
    [QueryInclude<CPlayer>]
    private void SetPlayerRotation(ref CTransform transform)
    {
        transform.Rotation = isGravityDown ? 0 : float.Pi;
    }

    [Query]
    [QueryInclude<CPlayer>]
    private void ClampPlayerVelocity(ref CVelocity velocity)
    {
        velocity.Velocity.Y = Math.Clamp(velocity.Velocity.Y, -4, 4);
    }
}
