using System;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Core.Utilities;
using Exanite.WarGames.Features.Cameras.Components;
using Exanite.WarGames.Features.Characters.Components;
using Exanite.WarGames.Features.Players.Components;
using Exanite.WarGames.Features.Transforms.Components;
using Exanite.WarGames.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Exanite.WarGames.Features.Players.Systems;

public partial class PlayerControllerSystem : EcsSystem, IUpdateSystem
{
    private readonly GameInputData input;

    public PlayerControllerSystem(GameInputData input)
    {
        this.input = input;
    }

    public void Update()
    {
        UpdateMovementQuery(World);
    }

    [Query]
    [All<CameraComponent>]
    private void UpdateAim1(ref CameraProjectionComponent cameraProjection)
    {
        UpdateAim2Query(World, ref cameraProjection);
    }

    [Query]
    [All<PlayerComponent>]
    private void UpdateAim2([Data] ref CameraProjectionComponent cameraProjection, ref TransformComponent transform)
    {
        var mouseScreenPosition = new Vector2(input.Current.Mouse.X, input.Current.Mouse.Y);
        var mouseWorldPosition = Vector2.Transform(mouseScreenPosition, cameraProjection.ScreenToWorld);

        var aimOffset = mouseWorldPosition - transform.Position;

        transform.Rotation = MathF.Atan2(aimOffset.Y, aimOffset.X);
    }

    [Query]
    [All<PlayerComponent>]
    private void UpdateMovement(ref MovementDirectionComponent movement, ref TransformComponent transform)
    {
        var direction = Vector2.Zero;
        direction.X -= input.Current.Keyboard.IsKeyDown(Keys.A) ? 1 : 0;
        direction.X += input.Current.Keyboard.IsKeyDown(Keys.D) ? 1 : 0;
        direction.Y -= input.Current.Keyboard.IsKeyDown(Keys.W) ? 1 : 0;
        direction.Y += input.Current.Keyboard.IsKeyDown(Keys.S) ? 1 : 0;
        direction = direction.AsNormalizedSafe();

        // Penalize not moving in the same direction as the character is facing
        // Moving backwards -> 50% speed
        // Moving sideways -> 75% speed
        var speedPenalty = Vector2.Dot(direction, new Vector2(MathF.Cos(transform.Rotation), MathF.Sin(transform.Rotation)));
        speedPenalty = MathUtility.Remap(speedPenalty, -1, 1, 0.5f, 1);

        movement.Direction = direction * speedPenalty;
    }
}
