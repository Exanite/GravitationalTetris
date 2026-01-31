using System.Numerics;
using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Cameras.Components;

public struct EcsCameraProjection : IComponent
{
    public Matrix4x4 View;
    public Matrix4x4 Projection;

    public EcsCameraProjection()
    {
        View = Matrix4x4.Identity;
        Projection = Matrix4x4.Identity;
    }
}
