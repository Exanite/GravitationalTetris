using System.Collections.Generic;
using System.Drawing;
using Myra.Graphics2D.UI;
using Myra.Platform;
using Window = Exanite.Engine.Windowing.Window;

namespace Exanite.GravitationalTetris.Features.Ui;

public class ExaniteEngineMyraPlatform : IMyraPlatform
{
    private readonly TouchCollection touchState;

    private readonly Window window;

    public ExaniteEngineMyraPlatform(IMyraRenderer renderer, Window window)
    {
        this.window = window;
        Renderer = renderer;

        touchState = new TouchCollection
        {
            Touches = new List<TouchLocation>(),
            IsConnected = false,
        };
    }

    public Point ViewSize => new(window.Settings.Width, window.Settings.Height);
    public IMyraRenderer Renderer { get; }

    public MouseInfo GetMouseInfo()
    {
        // Todo

        return default;
    }

    public void SetKeysDown(bool[] keys)
    {
        // Todo
    }

    public void SetMouseCursorType(MouseCursorType mouseCursorType)
    {
        // Todo
    }

    public TouchCollection GetTouchState()
    {
        return touchState;
    }
}
