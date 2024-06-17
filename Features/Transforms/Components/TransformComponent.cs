using System.Numerics;
using Myriad.ECS;

namespace Exanite.GravitationalTetris.Features.Transforms.Components;

public struct TransformComponent : IComponent
{
    public Vector2 Position;
    public Vector2 Size;
    public float Rotation;

    public TransformComponent()
    {
        Size = Vector2.One;
    }
}
