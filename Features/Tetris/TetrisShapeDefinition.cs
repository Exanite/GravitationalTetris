using Exanite.Engine.OldRendering;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Tetris;

public record TetrisShapeDefinition
{
    public required bool[,] Shape;
    public required IResourceHandle<Texture2D> DefaultTexture;
    public required IResourceHandle<Texture2D> SnowTexture;
    public required int PivotX;
    public required int PivotY;
}
