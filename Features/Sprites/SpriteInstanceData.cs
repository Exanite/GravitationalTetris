using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Exanite.Engine.Rendering;
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
                Offset = 0,
                Format = Format.R32G32B32A32Sfloat,
            },
            new InputElementDesc()
            {
                Offset = 4 * 4,
                Format = Format.R32G32B32A32Sfloat,
            },
            new InputElementDesc()
            {
                Offset = 4 * 8,
                Format = Format.R32G32B32A32Sfloat,
            },
            new InputElementDesc()
            {
                Offset = 4 * 12,
                Format = Format.R32G32B32A32Sfloat,
            },
            new InputElementDesc()
            {
                Offset = 4 * 16,
                Format = Format.R32G32B32A32Sfloat,
            },
        ],
    };

    new()
    {
        LayoutElements = new LayoutElement[]
        {
            new()
            {
                HLSLSemantic = "ATTRIB",
                InputIndex = 0,

                NumComponents = 4,
                ValueType = ValueType.Float32,

                IsNormalized = false,

                Frequency = InputElementFrequency.PerInstance,
            },
            new()
            {
                HLSLSemantic = "ATTRIB",
                InputIndex = 1,

                NumComponents = 4,
                ValueType = ValueType.Float32,

                IsNormalized = false,

                Frequency = InputElementFrequency.PerInstance,
            },
            new()
            {
                HLSLSemantic = "ATTRIB",
                InputIndex = 2,

                NumComponents = 4,
                ValueType = ValueType.Float32,

                IsNormalized = false,

                Frequency = InputElementFrequency.PerInstance,
            },
            new()
            {
                HLSLSemantic = "ATTRIB",
                InputIndex = 3,

                NumComponents = 4,
                ValueType = ValueType.Float32,

                IsNormalized = false,

                Frequency = InputElementFrequency.PerInstance,
            },
            new()
            {
                HLSLSemantic = "ATTRIB",
                InputIndex = 4,

                NumComponents = 4,
                ValueType = ValueType.Float32,

                IsNormalized = false,

                Frequency = InputElementFrequency.PerInstance,
            },
            new()
            {
                HLSLSemantic = "ATTRIB",
                InputIndex = 5,

                NumComponents = 2,
                ValueType = ValueType.Float32,

                IsNormalized = false,

                Frequency = InputElementFrequency.PerInstance,
            },
            new()
            {
                HLSLSemantic = "ATTRIB",
                InputIndex = 6,

                NumComponents = 2,
                ValueType = ValueType.Float32,

                IsNormalized = false,

                Frequency = InputElementFrequency.PerInstance,
            },
        },
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
