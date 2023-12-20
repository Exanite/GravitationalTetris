using System.Numerics;

namespace Exanite.GravitationalTetris.Features.Cameras.Components;

public struct CameraProjectionComponent
{
    public Matrix4x4 View;
    public Matrix4x4 Projection;

    public CameraProjectionComponent()
    {
        View = Matrix4x4.Identity;
        Projection = Matrix4x4.Identity;
    }
}
