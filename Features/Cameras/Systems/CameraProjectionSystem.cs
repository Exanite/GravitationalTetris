using System.Numerics;
using Arch.System;
using Exanite.Ecs.Systems;
using Exanite.Engine.Windowing;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;

namespace Exanite.GravitationalTetris.Features.Cameras.Systems;

public partial class CameraProjectionSystem : EcsSystem, IRenderSystem
{
    private readonly Window window;

    public CameraProjectionSystem(Window window)
    {
        this.window = window;
    }

    public void Render()
    {
        UpdateQuery(World);
    }

    [Query]
    private void Update(ref CameraComponent camera, ref TransformComponent transform, ref CameraProjectionComponent cameraProjection)
    {
        var swapChain = window.SwapChain;
        var aspectRatio = (float)swapChain.GetDesc().Width / swapChain.GetDesc().Height;

        cameraProjection.View = Matrix4x4.CreateRotationZ(transform.Rotation) * Matrix4x4.CreateTranslation(-transform.Position.X, -transform.Position.Y, -10);
        cameraProjection.Projection = Matrix4x4.CreateOrthographic(camera.VerticalHeight * aspectRatio, camera.VerticalHeight, 0.001f, 1000f) * Matrix4x4.CreateRotationZ(float.Pi) * Matrix4x4.CreateScale(-1, 1, 1);
    }
}
