using System.Numerics;
using System.Runtime.InteropServices;

namespace Exanite.GravitationalTetris.Features.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct SpriteUniformData
{
    public required Matrix4x4 World;
    public required Matrix4x4 View;
    public required Matrix4x4 Projection;

    public required Vector4 Color;
}
