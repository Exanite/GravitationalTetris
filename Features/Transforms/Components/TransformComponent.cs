using Microsoft.Xna.Framework;

namespace Exanite.WarGames.Features.Transforms.Components;

public struct TransformComponent
{
    public Vector2 Position;
    public Vector2 Size;
    public float Rotation;

    public TransformComponent()
    {
        Size = Vector2.One;
    }
}
