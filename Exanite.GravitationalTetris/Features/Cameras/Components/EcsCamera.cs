using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Cameras.Components;

public struct EcsCamera : IEcsComponent
{
    public float VerticalHeight;

    public EcsCamera(float verticalHeight)
    {
        VerticalHeight = verticalHeight;
    }
}
