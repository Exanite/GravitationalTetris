using System.Diagnostics.CodeAnalysis;
using Exanite.Engine.OldRendering;
using Exanite.ResourceManagement;
using Myriad.ECS;

namespace Exanite.GravitationalTetris.Features.Sprites.Components;

public struct ComponentSprite : IComponent
{
    public required IResourceHandle<Texture2D> Texture;

    [SetsRequiredMembers]
    public ComponentSprite(IResourceHandle<Texture2D> texture)
    {
        Texture = texture;
    }
}
