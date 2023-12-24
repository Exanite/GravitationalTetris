using System.Numerics;
using System.Runtime.InteropServices;
using Exanite.Engine.Rendering;

namespace Exanite.GravitationalTetris.Features.Sprites;

[StructLayout(LayoutKind.Sequential)]
public struct SpriteUniformData
{
    public required Texture2D Texture;

    public required Matrix4x4 View;
    public required Matrix4x4 Projection;
}
