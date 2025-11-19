using System.Collections.Generic;
using Exanite.Myriad.Ecs;

namespace Exanite.GravitationalTetris.Features.Tetris.Components;

public struct CTetrisRoot : IComponent
{
    public required TetrisShapeDefinition Shape;
    public required TetrisRotation Rotation;

    public readonly List<TetrisVector2Int> BlockPositions;
    public readonly List<TetrisVector2Int> PredictedBlockPositions;

    public CTetrisRoot()
    {
        BlockPositions = new List<TetrisVector2Int>();
        PredictedBlockPositions = new List<TetrisVector2Int>();
    }
}
