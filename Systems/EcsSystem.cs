using Arch.Core;

namespace Exanite.Extraction.Systems;

public abstract class EcsSystem
{
    public required World World { get; init; }
}
