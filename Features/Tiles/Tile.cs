using Exanite.ResourceManagement;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.GravitationalTetris.Features.Tiles;

public struct Tile
{
    public bool IsWall;
    public IResourceHandle<Texture2D>? Texture;
}
