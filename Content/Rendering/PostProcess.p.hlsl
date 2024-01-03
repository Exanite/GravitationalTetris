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

void main(
    in Input input,
    out Output output)
{
    float2 uv = input.Uv;

    uv += 0.1 * float2(
        sin(input.Uv.y * 30) * 0.1 * sin(Time.x * 3),
        sin(input.Uv.x * 20) * 0.02 * sin(Time.x * 2));

    uv = float2(uv.x, 1 - uv.y);

    output.Color = (Texture.Sample(TextureSampler, uv) / 2)
        + (Texture.Sample(TextureSampler, uv) * (((sin(Time) + 1) / 4)))
        + float4((input.Uv * ((sin(Time) + 1) / 4)).xy, 0, 0);
}
