using System.Numerics;

namespace Exanite.GravitationalTetris.Features.Sprites;

public record struct SpriteUniformDrawSettings
{
    public required Matrix4x4 View;
    public required Matrix4x4 Projection;
}
