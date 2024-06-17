using Myriad.ECS;

namespace Exanite.GravitationalTetris.Features.Tetris.Components;

public struct TetrisBlockComponent : IComponent
{
    public required Entity Root;

    public required TetrisShapeDefinition Definition;
    public required int LocalX;
    public required int LocalY;
}
