using Arch.System;
using Exanite.Ecs.Systems;
using Exanite.WarGames.Features.Cameras.Components;
using Exanite.WarGames.Features.Transforms.Components;

namespace Exanite.WarGames.Features.Cameras.Systems;

public partial class CameraFollowTargetSystem : EcsSystem, IUpdateSystem
{
    public void Update()
    {
        UpdateQuery(World);
    }

    [Query]
    private void Update(ref CameraTargetComponent cameraTarget, ref TransformComponent transform)
    {
        UpdateCameraQuery(World, ref transform);
    }

    [Query]
    private void UpdateCamera([Data] ref TransformComponent targetTransform, ref CameraComponent camera, ref TransformComponent transform)
    {
        transform.Position = targetTransform.Position;
    }
}
