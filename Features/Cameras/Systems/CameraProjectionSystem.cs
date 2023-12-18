using Arch.System;
using Exanite.Ecs.Systems;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Microsoft.Xna.Framework;

namespace Exanite.GravitationalTetris.Features.Cameras.Systems;

public partial class CameraProjectionSystem : EcsSystem, IUpdateSystem
{
    public required Game Game { get; init; }

    public void Update()
    {
        UpdateQuery(World);
    }

    [Query]
    private void Update(ref CameraComponent camera, ref TransformComponent transform, ref CameraProjectionComponent cameraProjection)
    {
        var worldToLocalTranslation = Matrix.CreateTranslation(-transform.Position.X, -transform.Position.Y, 0);
        var worldToLocalRotation = Matrix.CreateRotationZ(transform.Rotation);
        cameraProjection.WorldToLocal = worldToLocalTranslation * worldToLocalRotation;

        var scaleFactor = Game.Window.ClientBounds.Height / camera.VerticalHeight;
        var localToScreenScale = Matrix.CreateScale(scaleFactor, scaleFactor, 1);
        var offsetBackOnScreen = Matrix.CreateTranslation(0, Game.Window.ClientBounds.Height, 0);
        var offsetToCenterOfScreen = Matrix.CreateTranslation(Game.Window.ClientBounds.Width / 2f, -Game.Window.ClientBounds.Height / 2f, 0);
        cameraProjection.LocalToScreen = localToScreenScale * offsetBackOnScreen * offsetToCenterOfScreen;

        cameraProjection.MetersToPixels = scaleFactor;
        cameraProjection.Rotation = -transform.Rotation;
    }
}
