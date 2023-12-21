using System.Drawing;
using Exanite.Engine.Rendering;

namespace Exanite.GravitationalTetris.Features.Resources;

public record class FontSystemLoadingSettings
{
    public Texture2D? ExistingTexture { get; init; }
    public Rectangle ExistingTextureUsedSpace { get; init; }
    public string[]? AdditionalFonts { get; init; }
}
