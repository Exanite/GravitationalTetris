using System.Numerics;
using System.Runtime.InteropServices;

namespace Exanite.GravitationalTetris.Features.Sprites;

[StructLayout(LayoutKind.Sequential)]
public struct SpriteInstanceData
{
    public required Matrix4x4 WorldViewProjection;

    public Vector4 Color;

    public Vector2 Offset;
    public Vector2 Size;

    public SpriteInstanceData()
    {
        Color = Vector4.One;

        Offset = Vector2.Zero;
        Size = Vector2.One;
    }
}
