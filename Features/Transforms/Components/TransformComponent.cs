using System.Numerics;

namespace Exanite.GravitationalTetris.Features.Transforms.Components;

public struct TransformComponent
{
    public Vector2 Position;
    public Vector2 Size;
    public float Rotation;

    public TransformComponent()
    {
        Size = Vector2.One;
    }
}
