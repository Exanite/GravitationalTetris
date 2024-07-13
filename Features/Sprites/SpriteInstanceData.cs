using System.Numerics;
using System.Runtime.InteropServices;
using Diligent;

namespace Exanite.GravitationalTetris.Features.Sprites;

[StructLayout(LayoutKind.Sequential)]
public struct SpriteInstanceData
{
    public static InputLayoutDesc Layout { get; } = new()
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

    private Matrix4x4 world;

    public Vector4 Color;

    public Vector2 Offset;
    public Vector2 Size;

    public required Matrix4x4 World
    {
        get => Matrix4x4.Transpose(world);
        set => world = Matrix4x4.Transpose(value);
    }

    public SpriteInstanceData()
    {
        Color = Vector4.One;

        Offset = Vector2.Zero;
        Size = Vector2.One;
    }
}
