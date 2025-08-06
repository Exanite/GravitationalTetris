using Exanite.ResourceManagement;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Providers;

namespace Exanite.GravitationalTetris.Features.Audio.Loaders;

public class AudioDataLoader : SimpleResourceLoader<AssetDataProvider>
{
    private readonly MiniAudioEngine engine;

    public AudioDataLoader(MiniAudioEngine engine)
    {
        this.engine = engine;
    }

    public override void Load(IResourceLoadOperation<AssetDataProvider> loadOperation)
    {
        using var stream = loadOperation.OpenFile(loadOperation.Key);
        var provider = new AssetDataProvider(engine, AudioConstants.DefaultFormat, stream);

        loadOperation.Fulfill(provider);
    }
}