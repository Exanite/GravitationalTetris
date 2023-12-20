using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Sprites.Components;

public struct SpriteComponent
{
    public IResourceHandle<Texture2D> Texture;

    public SpriteComponent(IResourceHandle<Texture2D> texture)
    {
        Texture = texture;
    }
}
