using System.Diagnostics.CodeAnalysis;
using Exanite.Myriad.Ecs;
using SoundFlow.Components;

namespace Exanite.GravitationalTetris.Features.Audio.Components;

public struct CAudioSource : IComponent
{
    public required SoundPlayer Player;

    [SetsRequiredMembers]
    public CAudioSource(SoundPlayer player)
    {
        Player = player;
    }
}
