using Arch.System;
using Exanite.Extraction.Features.Cameras.Components;
using Exanite.Extraction.Features.Time;
using Exanite.Extraction.Systems;
using Microsoft.Xna.Framework.Input;

namespace Exanite.Extraction.Features.Players.Systems;

public partial class CameraZoomSystem : EcsSystem, IUpdateSystem
{
    private readonly GameInputData input;
    private readonly GameTimeData time;

    public CameraZoomSystem(GameInputData input, GameTimeData time)
    {
        this.input = input;
        this.time = time;
    }

    public void Update()
    {
        UpdateQuery(World);
    }

    [Query]
    private void Update(ref CameraComponent camera)
    {
        camera.VerticalHeight += input.Current.Keyboard.IsKeyDown(Keys.Space) ? 6 * time.DeltaTime : 0;
        camera.VerticalHeight -= input.Current.Keyboard.IsKeyDown(Keys.LeftShift) ? 6 * time.DeltaTime : 0;
    }
}
