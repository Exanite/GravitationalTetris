using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Cameras.Components;

public struct ComponentCamera : IComponent
{
    public float VerticalHeight;

    public ComponentCamera(float verticalHeight)
    {
        VerticalHeight = verticalHeight;
    }
}
