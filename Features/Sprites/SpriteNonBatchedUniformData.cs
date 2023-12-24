using System.Numerics;
using System.Runtime.InteropServices;

namespace Exanite.GravitationalTetris.Features.Sprites;

[StructLayout(LayoutKind.Sequential)]
public struct SpriteNonBatchedUniformData
{
    public required Matrix4x4 World;
    public required Matrix4x4 View;
    public required Matrix4x4 Projection;

    public Vector4 Color;

    public Vector2 Offset;
    public Vector2 Size;

    public SpriteNonBatchedUniformData()
    {
        Color = Vector4.One;

        Offset = Vector2.Zero;
        Size = Vector2.One;
    }
}
