using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Exanite.Engine.Graphics;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Sprites;

[StructLayout(LayoutKind.Sequential)]
public struct SpriteInstanceData : IVertex
{
    public static InputLayoutDesc Layout { get; } = new()
    {
        Stride = Unsafe.SizeOf<SpriteInstanceData>(),
        Elements =
        [
            // LocalToWorld
            new InputElementDesc()
            {
                ByteCount = 16,
                Format = Format.R32G32B32A32Sfloat,
            },
            new InputElementDesc()
            {
                ByteCount = 16,
                Format = Format.R32G32B32A32Sfloat,
            },
            new InputElementDesc()
            {
                ByteCount = 16,
                Format = Format.R32G32B32A32Sfloat,
            },
            new InputElementDesc()
            {
                ByteCount = 16,
                Format = Format.R32G32B32A32Sfloat,
            },
            // Color
            new InputElementDesc()
            {
                ByteCount = 16,
                Format = Format.R32G32B32A32Sfloat,
            },
            // Offset
            new InputElementDesc()
            {
                ByteCount = 8,
                Format = Format.R32G32Sfloat,
            },
            // Size
            new InputElementDesc()
            {
                ByteCount = 8,
                Format = Format.R32G32Sfloat,
            },
        ],
    };

    private Matrix4x4 localToWorld;

    public Vector4 Color;

    public Vector2 Offset;
    public Vector2 Size;

    public required Matrix4x4 LocalToWorld
    {
        get => Matrix4x4.Transpose(localToWorld);
        set => localToWorld = Matrix4x4.Transpose(value);
    }

    public SpriteInstanceData()
    {
        Color = Vector4.One;

        Offset = Vector2.Zero;
        Size = Vector2.One;
    }
}
