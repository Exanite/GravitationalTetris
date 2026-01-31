using System.Numerics;
using Exanite.Engine.Ecs.Queries;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Windowing;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;

namespace Exanite.GravitationalTetris.Features.Cameras.Systems;

public partial class CameraProjectionSystem : EngineSystem, IRenderUpdateSystem
{
    private readonly Window window;

    public CameraProjectionSystem(Window window)
    {
        this.window = window;
    }

    public void RenderUpdate()
    {
        UpdateQuery();
    }

    [Query]
    private void Update(ref EcsCamera camera, ref EcsTransform transform, ref EcsCameraProjection cameraProjection)
    {
        cameraProjection.View = Matrix4x4.CreateRotationZ(transform.Rotation) * Matrix4x4.CreateTranslation(-transform.Position.X, -transform.Position.Y, -10);
        cameraProjection.Projection = Matrix4x4.CreateOrthographic(camera.VerticalHeight * window.AspectRatio, camera.VerticalHeight, 1000f, 0.001f) * Matrix4x4.CreateRotationZ(float.Pi);
    }
}
