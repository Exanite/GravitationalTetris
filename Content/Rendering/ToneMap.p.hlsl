cbuffer Uniforms
{
    float Time;
}

Texture2D Texture;
SamplerState TextureSampler;

struct Input
{
    float2 Uv : TEX_COORD;
};

struct Output
{
    float4 Color : SV_TARGET;
};

// From https://www.shadertoy.com/view/XsGfWV
float3 TonemapAces(float3 color)
{
    float3x3 m1 = float3x3(
        0.59719, 0.07600, 0.02840,
        0.35458, 0.90834, 0.13383,
        0.04823, 0.01566, 0.83777
    );
    float3x3 m2 = float3x3(
        1.60475, -0.10208, -0.00327,
        -0.53108, 1.10813, -0.07276,
        -0.07367, -0.00605, 1.07602
    );
    float3 v = m1 * color;
    float3 a = v * (v + 0.0245786) - 0.000090537;
    float3 b = v * (0.983729 * v + 0.4329510) + 0.238081;

    return m2 * (a / b);
}

void main(
    in Input input,
    out Output output)
{
    float2 uv = float2(input.Uv.x, 1 - input.Uv.y);
    float4 color = Texture.Sample(TextureSampler, uv);

    output.Color = float4(TonemapAces(color.xyz * 1.75), 1);
}
