using Flecs.NET.Core;

namespace Exanite.GravitationalTetris.Features.Tetris.Components;

public struct TetrisBlockComponent
{
    public required Entity Root;

    public required TetrisShapeDefinition Definition;
    public required int LocalX;
    public required int LocalY;
}
