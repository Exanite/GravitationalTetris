using Autofac;
using Exanite.Engine.Modding;
using Exanite.GravitationalTetris.Features.Audio.Loaders;
using SoundFlow.Backends.MiniAudio;

namespace Exanite.GravitationalTetris.Features.Audio.Modules;

public class AudioModule : GameModule
{
    protected override void OnConfigureContainer(ContainerBuilder builder)
    {
        base.OnConfigureContainer(builder);

        builder.RegisterType<MiniAudioEngine>().SingleInstance();
        builder.RegisterType<AudioDataLoader>().AsSelf().AsImplementedInterfaces().SingleInstance();
    }
}