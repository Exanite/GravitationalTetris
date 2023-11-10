using System.Numerics;

namespace Exanite.Extraction.Features.Tiles.Components;

public struct TileChunkComponent
{
    public Tile[] Tiles;

    public Vector2 Position;
    public int Width;
    public int Height;
}
