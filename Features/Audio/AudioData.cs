using SoundFlow.Abstracts;
using SoundFlow.Providers;

namespace Exanite.GravitationalTetris.Features.Audio;

public class AudioData
{
    public byte[] Data { get; }

    public AudioData(byte[] data)
    {
        Data = data;
    }

    public AssetDataProvider CreateProvider(AudioEngine engine)
    {
        return new AssetDataProvider(engine, AudioConstants.DefaultFormat, Data);
    }
}
