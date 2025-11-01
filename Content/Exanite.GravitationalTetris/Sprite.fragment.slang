[[vk::binding(1, 0)]] Texture2D Textures[64];
[[vk::binding(2, 0)]] SamplerState TextureSampler;

struct Input
{
    float4 Position : SV_Position;
    float2 Uv : Uv;
    float4 Color: Color;
    uint TextureIndex : TexIndex;
};

struct Output
{
    float4 Color : SV_Target;
};

[shader("fragment")]
void main(in Input input, out Output output)
{
    output.Color = Textures[input.TextureIndex].Sample(TextureSampler, input.Uv) * input.Color;
}
