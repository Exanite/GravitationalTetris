using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Tetris.Components;

public struct EcsTetrisBlock : IComponent
{
    public required Entity Root;

    public required TetrisShapeDefinition Definition;
    public required int LocalX;
    public required int LocalY;
}
