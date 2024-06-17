using Myriad.ECS;

namespace Exanite.GravitationalTetris.Features.Cameras.Components;

public struct CameraComponent : IComponent
{
    public float VerticalHeight;

    public CameraComponent(float verticalHeight)
    {
        VerticalHeight = verticalHeight;
    }
}
