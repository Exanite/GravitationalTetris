using System.Numerics;
using System.Runtime.InteropServices;

namespace Exanite.GravitationalTetris.Features.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct SpriteUniformData
{
    public Matrix4x4 World;
    public Matrix4x4 View;
    public Matrix4x4 Projection;

    public Vector4 Color;
}
