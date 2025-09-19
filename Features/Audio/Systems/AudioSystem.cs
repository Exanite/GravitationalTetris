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

namespace Exanite.GravitationalTetris.Features.Audio.Systems;

public partial class AudioSystem : GameSystem, IStartSystem, IStopSystem, IFrameUpdateSystem, IDisposable
{
    public const string SwitchGravity = "/Exanite.GravitationalTetris/Audio/SwitchGravity.wav";
    public const string RotateShape = "/Exanite.GravitationalTetris/Audio/RotateShape.wav";
    public const string ClearTile = "/Exanite.GravitationalTetris/Audio/ClearTile.wav";
    public const string Restart = "/Exanite.GravitationalTetris/Audio/Restart.wav";

    private readonly AudioPlaybackDevice playbackDevice;
    private readonly Lifetime lifetime = new();

    private readonly ResourceManager resourceManager;
    private readonly MiniAudioEngine engine;
    private readonly EcsCommandBuffer commandBuffer;

    public AudioSystem(ResourceManager resourceManager, MiniAudioEngine engine, EcsCommandBuffer commandBuffer)
    {
        this.resourceManager = resourceManager;
        this.engine = engine;
        this.commandBuffer = commandBuffer;

        var device = engine.PlaybackDevices.FirstOrDefault(d => d.IsDefault);
        playbackDevice = engine.InitializePlaybackDevice(device, AudioConstants.DefaultFormat).DisposeWith(lifetime);
    }

    public void Start()
    {
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
        var provider = resourceManager.GetResource<AudioData>(resourceKey).Value.CreateProvider(engine);
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
        lifetime.Dispose();
    }
}
