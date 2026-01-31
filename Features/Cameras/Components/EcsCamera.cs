using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Cameras.Components;

public struct EcsCamera : IComponent
{
    public float VerticalHeight;

    public EcsCamera(float verticalHeight)
    {
        VerticalHeight = verticalHeight;
    }
}
