using Arch.Core;

namespace Exanite.WarGames.Systems;

public abstract class EcsSystem
{
    public required World World { get; init; }
}
