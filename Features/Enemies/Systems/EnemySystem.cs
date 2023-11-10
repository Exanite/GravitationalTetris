using System;
using Arch.System;
using Exanite.Core.Utilities;
using Exanite.WarGames.Features.Characters.Components;
using Exanite.WarGames.Features.Enemies.Components;
using Exanite.WarGames.Features.Players.Components;
using Exanite.WarGames.Features.Transforms.Components;
using Exanite.WarGames.Systems;

namespace Exanite.WarGames.Features.Enemies.Systems;

public partial class EnemySystem : EcsSystem, IUpdateSystem
{
    public void Update()
    {
        UpdateQuery(World);
    }

    [Query]
    private void Update(ref PlayerComponent player, ref TransformComponent transform)
    {
        MoveTowardPlayerQuery(World, ref transform);
    }

    [Query]
    private void MoveTowardPlayer([Data] ref TransformComponent playerTransform, ref EnemyComponent enemy, ref TransformComponent transform, ref MovementDirectionComponent movement)
    {
        var direction = (playerTransform.Position - transform.Position).AsNormalizedSafe();

        movement.Direction = direction;
        transform.Rotation = MathF.Atan2(direction.Y, direction.X);
    }
}
