using System.Numerics;
using System.Runtime.InteropServices;

namespace Exanite.GravitationalTetris.Features.Sprites;

[StructLayout(LayoutKind.Sequential)]
public struct SpriteUniformData
{
    public required Matrix4x4 View;
    public required Matrix4x4 Projection;
}
