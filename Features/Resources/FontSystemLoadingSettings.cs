using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.GravitationalTetris.Features.Resources;

public record class FontSystemLoadingSettings
{
    public Texture2D? ExistingTexture { get; init; }
    public Rectangle ExistingTextureUsedSpace { get; init; }
    public string[]? AdditionalFonts { get; init; }
}
