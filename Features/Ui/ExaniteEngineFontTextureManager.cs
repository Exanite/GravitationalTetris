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
        var texture = new Texture2D($"FontStashSharp Texture #{nextId}", width, height, rendererContext, Usage.Dynamic);
        textures.Add(texture);

        nextId++;

        return texture;
    }

    public Point GetTextureSize(object texture)
    {
        var typedTexture = (Texture2D)texture;

        return new Point(typedTexture.Width, typedTexture.Height);
    }

    public unsafe void SetTextureData(object texture, Rectangle bounds, byte[] data)
    {
        var typedTexture = (Texture2D)texture;

        var resource = rendererContext.DeviceContext.MapTextureSubresource(typedTexture.Texture, 0, 0, MapType.Write, MapFlags.Discard, new Box
        {
            MinX = (uint)bounds.X,
            MinY = (uint)bounds.Y,
            MaxX = (uint)(bounds.X + bounds.Width),
            MaxY = (uint)(bounds.Y + bounds.Height),
        });
        {
            var resourceData = new Span<byte>((void*)resource.Data, bounds.Width * bounds.Height * 4);
            for (var i = 0; i < resourceData.Length; i++)
            {
                resourceData[i] = data[i];
            }
        }
        rendererContext.DeviceContext.UnmapTextureSubresource(typedTexture.Texture, 0, 0);
    }

    public void Dispose()
    {
        foreach (var texture in textures)
        {
            texture.Dispose();
        }
    }
}
