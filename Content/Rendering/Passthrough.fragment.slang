[[vk::binding(0, 0)]] Texture2D Texture;
[[vk::binding(1, 0)]] SamplerState TextureSampler;

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
    float2 uv = float2(input.Uv.x, 1 - input.Uv.y);

    output.Color = (Texture.Sample(TextureSampler, uv));
}
