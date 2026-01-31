using System.Numerics;
using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Transforms.Components;

public struct EcsTransform : IComponent
{
    public Vector2 Position;
    public Vector2 Size;
    public float Rotation;

    public EcsTransform()
    {
        Size = Vector2.One;
    }
}
