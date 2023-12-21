using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Exanite.Engine.Rendering;
using FontStashSharp;
using FontStashSharp.Interfaces;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Platform;
using Window = Exanite.Engine.Windowing.Window;

namespace Exanite.GravitationalTetris.Features.Ui;

public class ExaniteEngineMyraPlatform : IMyraPlatform
{
    private TouchCollection touchState;

    private readonly Window window;

    public ExaniteEngineMyraPlatform(IMyraRenderer renderer, Window window)
    {
        this.window = window;
        Renderer = renderer;

        touchState = new TouchCollection()
        {
            Touches = new List<TouchLocation>(),
            IsConnected = false,
        };
    }

    public Point ViewSize => new Point(window.Settings.Width, window.Settings.Height);
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

public class ExaniteEngineMyraRenderer : IMyraRenderer
{
    public ExaniteEngineMyraRenderer(ITexture2DManager textureManager)
    {
        TextureManager = textureManager;
    }

    public ITexture2DManager TextureManager { get; }

    public RendererType RendererType => RendererType.Sprite;
    public Rectangle Scissor { get; set; }

    public void Begin(TextureFiltering textureFiltering)
    {
        throw new NotImplementedException();
    }

    public void End()
    {
        throw new NotImplementedException();
    }

    public void DrawSprite(object texture, Vector2 pos, Rectangle? src, FSColor color, float rotation, Vector2 scale, float depth)
    {
        throw new NotImplementedException();
    }

    public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
    {
        throw new NotSupportedException();
    }
}

public class ExaniteEngineFontTextureManager : ITexture2DManager
{
    public object CreateTexture(int width, int height)
    {
        throw new NotImplementedException();
    }

    public Point GetTextureSize(object texture)
    {
        var typedTexture = (Texture2D)texture;

        return new Point(typedTexture.Width, typedTexture.Height);
    }

    public void SetTextureData(object texture, Rectangle bounds, byte[] data)
    {
        throw new NotImplementedException();
    }
}
