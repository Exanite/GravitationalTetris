using System.Numerics;
using System.Runtime.InteropServices;

namespace Exanite.GravitationalTetris.Features.Sprites;

[StructLayout(LayoutKind.Sequential)]
public struct SpriteInstanceData
{
    public Matrix4x4 Model;

    public Vector4 Color;

    public Vector2 UvOffset;
    public Vector2 UvSize;

    public required uint TextureIndex;

    public SpriteInstanceData()
    {
        Color = Vector4.One;

        UvOffset = Vector2.Zero;
        UvSize = Vector2.One;
    }
}
