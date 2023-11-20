using Arch.Core;

namespace Exanite.WarGames.Features.Tetris.Components;

public struct TetrisBlockComponent
{
    public required EntityReference Root;

    public required TetrisShapeDefinition Definition;
    public required int LocalX;
    public required int LocalY;
}
