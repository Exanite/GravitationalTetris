using Microsoft.Xna.Framework;

namespace Exanite.Extraction.Features.Characters.Components;

public struct SmoothDampMovementComponent
{
    public float SmoothTime;
    public Vector2 SmoothVelocity;

    public SmoothDampMovementComponent(float smoothTime)
    {
        SmoothTime = smoothTime;
    }
}
