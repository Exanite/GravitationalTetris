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
            // Model
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
            // UvOffset
            new InputElementDesc()
            {
                ByteCount = 8,
                Format = Format.R32G32Sfloat,
            },
            // UvSize
            new InputElementDesc()
            {
                ByteCount = 8,
                Format = Format.R32G32Sfloat,
            },
            // TextureIndex
            new InputElementDesc()
            {
                ByteCount = 4,
                Format = Format.R32Uint,
            },
        ],
    };

    public Matrix4x4 Model;

    public Vector4 Color;

    public Vector2 UvOffset;
    public Vector2 UvSize;

    public required uint TextureIndex;

    public SpriteInstanceData()
    {
        Color = Vector4.One;

        UvOffset = Vector2.Zero;
        UvSize = Vector2.One;
    }
}
