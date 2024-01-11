using System.Collections.Generic;

namespace Exanite.GravitationalTetris.Features.FrameGraphs;

public class FrameGraph
{

}

public abstract class FrameGraphPass
{
    public readonly List<FrameGraphPass> Dependencies = new();

    public abstract void Setup();
}
