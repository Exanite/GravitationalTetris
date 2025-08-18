[[vk::binding(1, 0)]] Texture2D Textures[64];
[[vk::binding(2, 0)]] SamplerState TextureSampler;

struct Input
{
    float4 Pos : SV_POSITION;
    float2 Uv : TEX_COORD;
    float4 Color: COLOR;
    uint TextureIndex : TEX_INDEX;
};

struct Output
{
    float4 Color : SV_TARGET;
};

void main(
    in Input input,
    out Output output)
{
    output.Color = Textures[input.TextureIndex].Sample(TextureSampler, input.Uv) * input.Color;
}
