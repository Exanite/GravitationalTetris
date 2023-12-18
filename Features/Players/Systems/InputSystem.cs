using Exanite.Ecs.Systems;
using Microsoft.Xna.Framework.Input;

namespace Exanite.GravitationalTetris.Features.Players.Systems;

public class InputSystem : IUpdateSystem
{
    private readonly GameInputData input;

    public InputSystem(GameInputData input)
    {
        this.input = input;
    }

    public void Update()
    {
        var temp = input.Current;
        input.Current = input.Previous;
        input.Previous = temp;

        input.Current.Keyboard = Keyboard.GetState();
        input.Current.Mouse = Mouse.GetState();
    }
}
