using System.Numerics;
using Exanite.Engine.Graphics;

namespace Exanite.GravitationalTetris.Features.Sprites;

public struct SpriteDrawOptions
{
    public required Texture2D Texture;

    public required Matrix4x4 World;

    public Vector4 Color;

    public Vector2 Offset;
    public Vector2 Size;

    public SpriteDrawOptions()
    {
        Color = Vector4.One;

        Offset = Vector2.Zero;
        Size = Vector2.One;
    }
}
