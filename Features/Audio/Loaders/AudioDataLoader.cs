using Exanite.Core.Utilities;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Audio.Loaders;

public class AudioDataLoader : SimpleResourceLoader<AudioData>
{
    public override void Load(IResourceLoadOperation<AudioData> loadOperation)
    {
        var bytes = loadOperation.OpenFile(loadOperation.Key).ReadAsBytesAndDispose();
        var data = new AudioData(bytes);

        loadOperation.Fulfill(data);
    }
}