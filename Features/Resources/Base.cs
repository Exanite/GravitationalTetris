using Exanite.Core.Properties;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.Extraction.Features.Resources;

public static class Base
{
    public static PropertyDefinition<Texture2D> Player = new("Base:Player.png");
    public static PropertyDefinition<Texture2D> Wall = new("Base:Wall.png");
}
