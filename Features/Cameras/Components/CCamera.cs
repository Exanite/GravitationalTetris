using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Cameras.Components;

public struct CCamera : IComponent
{
    public float VerticalHeight;

    public CCamera(float verticalHeight)
    {
        VerticalHeight = verticalHeight;
    }
}
