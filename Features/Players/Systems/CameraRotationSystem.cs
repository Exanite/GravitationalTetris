using Arch.System;
using Exanite.Ecs.Systems;
using Exanite.WarGames.Features.Cameras.Components;
using Exanite.WarGames.Features.Time;
using Exanite.WarGames.Features.Transforms.Components;
using Microsoft.Xna.Framework.Input;

namespace Exanite.WarGames.Features.Players.Systems;

public partial class CameraRotationSystem : EcsSystem, IUpdateSystem
{
    private readonly GameInputData input;
    private readonly GameTimeData time;

    public CameraRotationSystem(GameInputData input, GameTimeData time)
    {
        this.input = input;
        this.time = time;
    }

    public void Update()
    {
        UpdateQuery(World);
    }

    [Query]
    private void Update(ref CameraComponent camera, ref TransformComponent transform)
    {
        transform.Rotation -= input.Current.Keyboard.IsKeyDown(Keys.E) ? time.DeltaTime : 0;
        transform.Rotation += input.Current.Keyboard.IsKeyDown(Keys.Q) ? time.DeltaTime : 0;
    }
}
