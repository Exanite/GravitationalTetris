using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Tiles;

public struct Tile
{
    public bool IsWall;
    public IResourceHandle<Texture2D>? Texture;
}
