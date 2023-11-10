using Arch.System;
using Exanite.Extraction.Features.Cameras.Components;
using Exanite.Extraction.Features.Transforms.Components;
using Exanite.Extraction.Systems;

namespace Exanite.Extraction.Features.Cameras.Systems;

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
