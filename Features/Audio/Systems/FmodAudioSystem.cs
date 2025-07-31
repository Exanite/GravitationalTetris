using System;
using System.IO;
using Exanite.Engine.Ecs.Systems;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Audio.Systems;

public class FmodAudioSystem : GameSystem, ISetupSystem, IFrameUpdateSystem, IDisposable
{
    public const string SwitchGravity = "event:/SwitchGravity";
    public const string RotateShape = "event:/RotateShape";
    public const string ClearTile = "event:/ClearTile";
    public const string Restart = "event:/Restart";

    private readonly ResourceManager resourceManager;

    public FmodAudioSystem(ResourceManager resourceManager)
    {
        this.resourceManager = resourceManager;
    }

    public void Setup()
    {
        // FmodStudio.create(out fmodStudio);
        // fmodStudio.getCoreSystem(out fmod);
        //
        // fmodStudio.initialize(
        //     maxchannels: 128,
        //     studioflags: INITFLAGS.NORMAL,
        //     flags: FMOD.INITFLAGS.NORMAL,
        //     extradriverdata: nint.Zero
        // );
        //
        // LoadBank("/Base/Tetris.bank");
        // LoadBank("/Base/Tetris.strings.bank");
    }

    public void Play(string eventName)
    {
        // fmodStudio.getEvent(eventName, out var eventDescription);
        // eventDescription.createInstance(out var eventInstance);
        // eventInstance.start();
        // eventInstance.release();
    }

    public void FrameUpdate()
    {
        // fmodStudio.update();
    }

    public void Dispose()
    {
        // fmodStudio.unloadAll();
    }
}
