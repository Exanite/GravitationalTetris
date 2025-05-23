using System.Numerics;
using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Transforms.Components;

public struct ComponentTransform : IComponent
{
    public Vector2 Position;
    public Vector2 Size;
    public float Rotation;

    public ComponentTransform()
    {
        Size = Vector2.One;
    }
}
