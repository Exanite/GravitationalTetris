using System.Diagnostics.CodeAnalysis;
using Exanite.Myriad.Ecs;
using SoundFlow.Components;

namespace Exanite.GravitationalTetris.Features.Audio.Components;

public struct ComponentAudioSource : IComponent
{
    public required SoundPlayer Player;

    [SetsRequiredMembers]
    public ComponentAudioSource(SoundPlayer player)
    {
        Player = player;
    }
}