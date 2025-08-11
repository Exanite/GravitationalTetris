using System.Collections.Generic;

namespace Exanite.GravitationalTetris.Features.Sprites;

public class SpriteBatch
{
    /// <summary>
    /// The submitted sprites.
    /// </summary>
    public List<SpriteInstanceDrawSettings> Sprites { get; } = new();

    /// <summary>
    /// Adds an individual sprite with the specified settings to the batch.
    /// </summary>
    public void Draw(SpriteInstanceDrawSettings settings)
    {
        Sprites.Add(settings);
    }

    /// <summary>
    /// Clears all submitted sprites.
    /// </summary>
    public void Clear()
    {
        Sprites.Clear();
    }
}