using System.Numerics;
using System.Runtime.InteropServices;

namespace Exanite.GravitationalTetris.Features.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct BloomDownUniformData
{
    public Vector2 FilterStep;
}
