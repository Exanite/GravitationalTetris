using Exanite.ResourceManagement;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.GravitationalTetris.Features.Sprites.Components;

public struct SpriteComponent
{
    public IResourceHandle<Texture2D> Resource;

    public SpriteComponent(IResourceHandle<Texture2D> resource)
    {
        Resource = resource;
    }
}
