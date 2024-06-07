using System;
using System.Collections.Generic;
using System.Drawing;
using Diligent;
using Exanite.Engine.Rendering;
using FontStashSharp.Interfaces;

namespace Exanite.GravitationalTetris.Features.Ui;

public class ExaniteEngineFontTextureManager : ITexture2DManager, IDisposable
{
    private int nextId = 0;
    private readonly List<Texture2D> textures = new();

    private readonly RendererContext rendererContext;

    public ExaniteEngineFontTextureManager(RendererContext rendererContext)
    {
        this.rendererContext = rendererContext;
    }

    public object CreateTexture(int width, int height)
    {
        var texture = new Texture2D(rendererContext, $"FontStashSharp Texture #{nextId}", width, height, Usage.Dynamic);
        textures.Add(texture);

        nextId++;

        return texture;
    }

    public Point GetTextureSize(object texture)
    {
        var typedTexture = (Texture2D)texture;

        return new Point(typedTexture.Width, typedTexture.Height);
    }

    public void SetTextureData(object texture, Rectangle bounds, byte[] data)
    {
        var typedTexture = (Texture2D)texture;

        using (typedTexture.Map(0, 0, MapType.Write, MapFlags.Discard, bounds, out var textureData))
        {
            for (var i = 0; i < textureData.Length; i++)
            {
                textureData[i] = data[i];
            }
        }
    }

    public void Dispose()
    {
        foreach (var texture in textures)
        {
            texture.Dispose();
        }
    }
}
