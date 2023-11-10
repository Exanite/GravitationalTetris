using System;
using Arch.System;
using Exanite.Core.Utilities;
using Exanite.Extraction.Features.Characters.Components;
using Exanite.Extraction.Features.Enemies.Components;
using Exanite.Extraction.Features.Players.Components;
using Exanite.Extraction.Features.Transforms.Components;
using Exanite.Extraction.Systems;

namespace Exanite.Extraction.Features.Enemies.Systems;

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
