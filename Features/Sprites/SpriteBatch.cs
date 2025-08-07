using System.Collections.Generic;

namespace Exanite.GravitationalTetris.Features.Sprites;

public class SpriteBatch
{
    /// <summary>
    /// Draw settings that are the same for each submitted sprite.
    /// </summary>
    public SpriteUniformDrawSettings UniformSettings { get; set; }

    /// <summary>
    /// The submitted sprites.
    /// </summary>
    public List<SpriteInstanceDrawSettings> Sprites { get; } = new();

    /// <summary>
    /// Draws an individual sprite with the specified settings.
    /// </summary>
    public void Draw(SpriteInstanceDrawSettings settings)
    {
        Sprites.Add(settings);
    }

    /// <summary>
    /// Clears all submitted sprites. Does not reset <see cref="UniformSettings"/>.
    /// </summary>
    public void Clear()
    {
        Sprites.Clear();
    }
}