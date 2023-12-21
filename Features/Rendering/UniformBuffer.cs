using System;
using System.Runtime.CompilerServices;
using Diligent;
using Exanite.Engine.Rendering;

namespace Exanite.GravitationalTetris.Features.Rendering;

public class UniformBuffer<T> : IDisposable
{
    private bool isDisposed = false;

    private readonly RendererContext rendererContext;

    public UniformBuffer(string name, RendererContext rendererContext, BufferDesc bufferDesc, int count = 1)
    {
        this.rendererContext = rendererContext;

        bufferDesc.Name = name;
        bufferDesc.Size = (ulong)(Unsafe.SizeOf<T>() * count);

        Count = count;
        Buffer = rendererContext.RenderDevice.CreateBuffer(bufferDesc);
    }

    public IBuffer Buffer { get; private set; }
    public int Count { get; }

    public unsafe Span<T> Map(MapType mapType, MapFlags mapFlags)
    {
        var pointer = rendererContext.DeviceContext.MapBuffer(Buffer, mapType, mapFlags);

        return new Span<T>((void*)pointer, Count);
    }

    public void Unmap(MapType mapType)
    {
        rendererContext.DeviceContext.UnmapBuffer(Buffer, mapType);
    }

    private void ReleaseUnmanagedResources()
    {
        Buffer.Dispose();
        Buffer = null!;
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);

        isDisposed = true;
    }

    ~UniformBuffer()
    {
        ReleaseUnmanagedResources();
    }
}
