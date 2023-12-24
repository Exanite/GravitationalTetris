using System.Numerics;
using System.Runtime.InteropServices;
using Diligent;

namespace Exanite.GravitationalTetris.Features.Sprites;

[StructLayout(LayoutKind.Sequential)]
public struct SpriteInstanceData
{
    public static InputLayoutDesc Layout { get; } = new InputLayoutDesc
    {
        LayoutElements = new LayoutElement[]
        {
            new LayoutElement
            {
                HLSLSemantic = "ATTRIB",
                InputIndex = 0,

                NumComponents = 16,
                ValueType = ValueType.Float32,

                IsNormalized = false,

                Frequency = InputElementFrequency.PerInstance,
            },
            new LayoutElement
            {
                HLSLSemantic = "ATTRIB",
                InputIndex = 1,

                NumComponents = 4,
                ValueType = ValueType.Float32,

                IsNormalized = false,

                Frequency = InputElementFrequency.PerInstance,
            },
            new LayoutElement
            {
                HLSLSemantic = "ATTRIB",
                InputIndex = 2,

                NumComponents = 2,
                ValueType = ValueType.Float32,

                IsNormalized = false,

                Frequency = InputElementFrequency.PerInstance,
            },
            new LayoutElement
            {
                HLSLSemantic = "ATTRIB",
                InputIndex = 3,

                NumComponents = 2,
                ValueType = ValueType.Float32,

                IsNormalized = false,

                Frequency = InputElementFrequency.PerInstance,
            },
        },
    };

    public required Matrix4x4 World;

    public Vector4 Color;

    public Vector2 Offset;
    public Vector2 Size;

    public SpriteInstanceData()
    {
        Color = Vector4.One;

        Offset = Vector2.Zero;
        Size = Vector2.One;
    }
}
