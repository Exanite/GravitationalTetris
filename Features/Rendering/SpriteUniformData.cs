using System.Numerics;
using System.Runtime.InteropServices;

namespace Exanite.GravitationalTetris.Features.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct SpriteUniformData
{
    public required Matrix4x4 World;
    public required Matrix4x4 View;
    public required Matrix4x4 Projection;

    public Vector4 Color;

    public Vector2 Offset;
    public Vector2 Size;

    public SpriteUniformData()
    {
        Color = Vector4.One;

        Offset = Vector2.Zero;
        Size = Vector2.One;
    }
}
