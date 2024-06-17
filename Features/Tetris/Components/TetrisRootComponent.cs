using System.Collections.Generic;
using Myriad.ECS;

namespace Exanite.GravitationalTetris.Features.Tetris.Components;

public struct TetrisRootComponent : IComponent
{
    public required TetrisShapeDefinition Shape;
    public required TetrisRotation Rotation;

    public readonly List<TetrisVector2Int> BlockPositions;
    public readonly List<TetrisVector2Int> PredictedBlockPositions;

    public TetrisRootComponent()
    {
        BlockPositions = new List<TetrisVector2Int>();
        PredictedBlockPositions = new List<TetrisVector2Int>();
    }
}
