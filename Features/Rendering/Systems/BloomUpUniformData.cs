using System.Numerics;
using System.Runtime.InteropServices;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

[StructLayout(LayoutKind.Sequential)]
public struct BloomUpUniformData
{
    public Vector2 FilterStep;
    public float Alpha;
}
