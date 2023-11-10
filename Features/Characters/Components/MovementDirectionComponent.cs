using Microsoft.Xna.Framework;

namespace Exanite.WarGames.Features.Characters.Components;

public struct MovementDirectionComponent
{
    /// <summary>
    /// The direction to move in.
    /// <para/>
    /// Note: This vector usually is normalized, but doesn't strictly need to be. <br/>
    /// For example: Using a direction vector with a magnitude less than 1 allows an entity to move at less than their max movement speed.
    /// </summary>
    public Vector2 Direction;
}
