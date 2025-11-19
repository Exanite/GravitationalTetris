using System.Numerics;
using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Cameras.Components;

public struct CCameraProjection : IComponent
{
    public Matrix4x4 View;
    public Matrix4x4 Projection;

    public CCameraProjection()
    {
        View = Matrix4x4.Identity;
        Projection = Matrix4x4.Identity;
    }
}
