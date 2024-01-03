cbuffer Uniforms
{
    float2 FilterStep;
}

Texture2D Texture;
SamplerState TextureSampler;

struct Input
{
    float4 Pos : SV_POSITION;
    float2 Uv : TEX_COORD;
};

struct Output
{
    float4 Color : SV_TARGET;
};

void main(
    in Input input,
    out Output output)
{
    output.Color = Texture.Sample(TextureSampler, input.Uv);
}
