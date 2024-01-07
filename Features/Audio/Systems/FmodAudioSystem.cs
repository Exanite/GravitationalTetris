using System;
using System.IO;
using Exanite.Ecs.Systems;
using Exanite.ResourceManagement;
using Fmod = FMOD.System;
using FmodStudio = FMOD.Studio.System;
using FMOD.Studio;

namespace Exanite.GravitationalTetris.Features.Audio.Systems;

public class FmodAudioSystem : ISetupSystem, IUpdateSystem, IDisposable
{
    public const string SwitchGravity = "event:/SwitchGravity";
    public const string RotateShape = "event:/RotateShape";
    public const string ClearTile = "event:/ClearTile";
    public const string Restart = "event:/Restart";

    private Fmod fmod;
    private FmodStudio fmodStudio;

    private readonly ResourceManager resourceManager;

    public FmodAudioSystem(ResourceManager resourceManager)
    {
        this.resourceManager = resourceManager;
    }

    public void Setup()
    {
        FmodStudio.create(out fmodStudio);
        fmodStudio.getCoreSystem(out fmod);

        fmodStudio.initialize(
            maxchannels: 128,
            studioflags: INITFLAGS.NORMAL,
            flags: FMOD.INITFLAGS.NORMAL,
            extradriverdata: IntPtr.Zero
        );

        LoadBank("Base:Tetris.bank");
        LoadBank("Base:Tetris.strings.bank");
    }

    public void Play(string eventName)
    {
        fmodStudio.getEvent(eventName, out var eventDescription);
        eventDescription.createInstance(out var eventInstance);
        eventInstance.start();
        eventInstance.release();
    }

    public void Update()
    {
        fmodStudio.update();
    }

    public void Dispose()
    {
        fmodStudio.unloadAll();
    }

    private Bank LoadBank(string resourceKey)
    {
        using (var stream = resourceManager.Open(resourceKey))
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            var data = memoryStream.ToArray();

            fmodStudio.loadBankMemory(data, LOAD_BANK_FLAGS.NORMAL, out var bank);

            return bank;
        }
    }
}
