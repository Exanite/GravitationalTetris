using System.Numerics;
using Exanite.Engine.Graphics;

namespace Exanite.GravitationalTetris.Features.Sprites;

public struct SpriteInstanceDrawSettings
{
    public required Texture2D Texture;

    /// <summary>
    /// The model matrix of the sprite. This transforms from local to world space.
    /// </summary>
    public required Matrix4x4 Model;

    /// <summary>
    /// This will be multiplied with the sprite's color.
    /// </summary>
    public Vector4 Color;

    /// <summary>
    /// Defines the offset of a rectangle in UV space that the sprite should sample from.
    /// </summary>
    public Vector2 UvOffset;

    /// <summary>
    /// Defines the size of a rectangle in UV space that the sprite should sample from.
    /// </summary>
    public Vector2 UvSize;

    public SpriteInstanceDrawSettings()
    {
        Color = Vector4.One;

        UvOffset = Vector2.Zero;
        UvSize = Vector2.One;
    }
}
