using System.Diagnostics.CodeAnalysis;
using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;
using Myriad.ECS;

namespace Exanite.GravitationalTetris.Features.Sprites.Components;

public struct SpriteComponent : IComponent
{
    public required IResourceHandle<Texture2D> Texture;

    [SetsRequiredMembers]
    public SpriteComponent(IResourceHandle<Texture2D> texture)
    {
        Texture = texture;
    }
}
