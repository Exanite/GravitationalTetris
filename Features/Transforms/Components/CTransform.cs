using System.Numerics;
using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Transforms.Components;

public struct CTransform : IComponent
{
    public Vector2 Position;
    public Vector2 Size;
    public float Rotation;

    public CTransform()
    {
        Size = Vector2.One;
    }
}
