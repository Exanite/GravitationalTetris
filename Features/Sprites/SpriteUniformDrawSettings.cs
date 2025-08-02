using System.Numerics;
using Exanite.Engine.Graphics;

namespace Exanite.GravitationalTetris.Features.Sprites;

public record struct SpriteUniformDrawSettings
{
    public required GraphicsCommandBuffer CommandBuffer;
    public required Texture2D ColorTarget;
    public required Texture2D DepthTarget;

    public required Matrix4x4 View;
    public required Matrix4x4 Projection;
}
