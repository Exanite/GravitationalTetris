using System;
using System.Drawing;
using System.Numerics;
using Exanite.Engine.Rendering;
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

    public ExaniteEngineMyraRenderer(ITexture2DManager textureManager, SpriteBatchSystem spriteBatchSystem)
    {
        this.spriteBatchSystem = spriteBatchSystem;

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

        spriteBatchSystem.DrawSprite(typedTexture, new SpriteUniformData
        {
            World = Matrix4x4.Identity,
            View = Matrix4x4.Identity,
            Projection = Matrix4x4.Identity,
            Color = Vector4.One,
        });
    }

    public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
    {
        throw new NotSupportedException();
    }
}
