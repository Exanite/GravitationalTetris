using System;
using System.Linq;
using Exanite.Core.Runtime;
using Exanite.Engine.Ecs.Queries;
using Exanite.Engine.Ecs.Systems;
using Exanite.GravitationalTetris.Features.Audio.Components;
using Exanite.Myriad.Ecs;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.ResourceManagement;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;

namespace Exanite.GravitationalTetris.Features.Audio.Systems;

public partial class AudioSystem : GameSystem, IStartSystem, IStopSystem, IFrameUpdateSystem, IDisposable
{
    public const string SwitchGravity = "/Base/Audio/SwitchGravity.wav";
    public const string RotateShape = "/Base/Audio/RotateShape.wav";
    public const string ClearTile = "/Base/Audio/ClearTile.wav";
    public const string Restart = "/Base/Audio/Restart.wav";

    private EcsCommandBuffer commandBuffer = null!;
    private readonly AudioPlaybackDevice playbackDevice;
    private readonly DisposableCollection disposables = new();

    private readonly ResourceManager resourceManager;
    private readonly MiniAudioEngine engine;

    public AudioSystem(ResourceManager resourceManager, MiniAudioEngine engine)
    {
        this.resourceManager = resourceManager;
        this.engine = engine;

        var device = engine.PlaybackDevices.FirstOrDefault(d => d.IsDefault);
        playbackDevice = engine.InitializePlaybackDevice(device, AudioConstants.DefaultFormat).AddTo(disposables);
    }

    public void Start()
    {
        commandBuffer = new EcsCommandBuffer(World);
        playbackDevice.Start();
    }

    public void Stop()
    {
        playbackDevice.Stop();
    }

    public void FrameUpdate()
    {
        commandBuffer.Execute();
        DestroyCompletedAudioSourcesQuery(World);
        commandBuffer.Execute();
    }

    public void Play(string resourceKey)
    {
        var data = resourceManager.GetResource<AudioData>(resourceKey);
        var provider = new AssetDataProvider(engine, AudioConstants.DefaultFormat, data.Value.Data);
        var player = new SoundPlayer(engine, AudioConstants.DefaultFormat, provider);

        playbackDevice.MasterMixer.AddComponent(player);
        player.Play();

        commandBuffer.Create()
            .Set(new ComponentAudioSource(player));
    }

    [Query]
    private void DestroyCompletedAudioSources(Entity entity, ref ComponentAudioSource audioSource)
    {
        if (audioSource.Player.State == PlaybackState.Stopped)
        {
            playbackDevice.MasterMixer.RemoveComponent(audioSource.Player);
            audioSource.Player.Dispose();

            commandBuffer.Destroy(entity);
        }
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}
