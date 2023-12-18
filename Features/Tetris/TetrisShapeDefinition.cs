using Exanite.ResourceManagement;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.GravitationalTetris.Features.Tetris;

public record TetrisShapeDefinition
{
    public required bool[,] Shape;
    public required IResourceHandle<Texture2D> Texture;
    public required int PivotX;
    public required int PivotY;
}
