using System;
using System.Drawing;
using System.Numerics;
using Exanite.Engine.Rendering;
using Exanite.Engine.Windowing;
using Exanite.GravitationalTetris.Features.Rendering;
using Exanite.GravitationalTetris.Features.Sprites.Systems;
using FontStashSharp;
using FontStashSharp.Interfaces;
using Myra.Graphics2D;
using Myra.Platform;

namespace Exanite.GravitationalTetris.Features.Ui;

public class ExaniteEngineMyraRenderer : IMyraRenderer
{
    private readonly SpriteBatchSystem spriteBatchSystem;
    private readonly Window window;

    public ExaniteEngineMyraRenderer(SpriteBatchSystem spriteBatchSystem, Window window, ITexture2DManager textureManager)
    {
        this.spriteBatchSystem = spriteBatchSystem;
        this.window = window;
        TextureManager = textureManager;
    }

    public ITexture2DManager TextureManager { get; }

    public RendererType RendererType => RendererType.Sprite;
    public Rectangle Scissor { get; set; }

    public void Begin(TextureFiltering textureFiltering)
    {
        // Todo No batching/instancing for now
    }

    public void End()
    {
        // Todo No batching/instancing for now
    }

    public void DrawSprite(object texture, Vector2 pos, Rectangle? src, FSColor color, float rotation, Vector2 scale, float depth)
    {
        var typedTexture = (Texture2D)texture;

        var offset = Vector2.Zero;
        var pixelSize = new Vector2(typedTexture.Width, typedTexture.Height);
        var size = Vector2.One;
        if (src.HasValue)
        {
            var rect = src.Value;
            offset = new Vector2((float)rect.X / typedTexture.Width, (float)rect.Y / typedTexture.Height);
            pixelSize = new Vector2(rect.Width, rect.Height);
            size = new Vector2((float)rect.Width / typedTexture.Width, (float)rect.Height / typedTexture.Height);
        }

        var world = Matrix4x4.CreateTranslation(0.5f, 0.5f, 0) * Matrix4x4.CreateScale(scale.X * pixelSize.X, scale.Y * pixelSize.Y, 1) * Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateTranslation(pos.X, pos.Y, 0);
        var view = Matrix4x4.CreateTranslation(0, 0, -10);
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, window.Settings.Width, 0, window.Settings.Height, 0.001f, 1000f) * Matrix4x4.CreateRotationZ(float.Pi) * Matrix4x4.CreateScale(-1, 1, 1);

        spriteBatchSystem.DrawSprite(typedTexture, new SpriteUniformData
        {
            World = world,
            View = view,
            Projection = projection,

            Color = new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f),

            Offset = offset,
            Size = size,
        });
    }

    public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
    {
        throw new NotSupportedException();
    }
}
