using System.Numerics;
using Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Cameras.Components;

public struct ComponentCameraProjection : IComponent
{
    public Matrix4x4 View;
    public Matrix4x4 Projection;

    public ComponentCameraProjection()
    {
        View = Matrix4x4.Identity;
        Projection = Matrix4x4.Identity;
    }
}
