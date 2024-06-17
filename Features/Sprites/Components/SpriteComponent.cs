using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;
using Myriad.ECS;

namespace Exanite.GravitationalTetris.Features.Sprites.Components;

public struct SpriteComponent : IComponent
{
    public IResourceHandle<Texture2D> Texture;

    public SpriteComponent(IResourceHandle<Texture2D> texture)
    {
        Texture = texture;
    }
}
