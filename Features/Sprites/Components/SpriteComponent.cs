using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Sprites.Components;

public struct SpriteComponent
{
    public IResourceHandle<Texture2D> Resource;

    public SpriteComponent(IResourceHandle<Texture2D> resource)
    {
        Resource = resource;
    }
}
