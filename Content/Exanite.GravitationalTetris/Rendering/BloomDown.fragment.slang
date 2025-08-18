struct UniformData
{
    float2 FilterStep;
}

[[vk::binding(0, 0)]] Texture2D Texture;
[[vk::binding(1, 0)]] SamplerState TextureSampler;
[[vk::binding(2, 0)]] ConstantBuffer<UniformData> Uniforms;

struct Input
{
    float4 Pos : SV_POSITION;
    float2 Uv : TEX_COORD;
};

struct Output
{
    float4 Color : SV_TARGET;
};

// Based on https://learnopengl.com/Guest-Articles/2022/Phys.-Based-Bloom
void main(
    in Input input,
    out Output output)
{
    float2 uv = float2(input.Uv.x, 1 - input.Uv.y);

    float x = Uniforms.FilterStep.x;
    float y = Uniforms.FilterStep.y;

    // Take 13 samples around current texel
    float3 a = Texture.Sample(TextureSampler, float2(uv.x - 2 * x, uv.y + 2 * y)).rgb;
    float3 b = Texture.Sample(TextureSampler, float2(uv.x, uv.y + 2 * y)).rgb;
    float3 c = Texture.Sample(TextureSampler, float2(uv.x + 2 * x, uv.y + 2 * y)).rgb;

    float3 d = Texture.Sample(TextureSampler, float2(uv.x - 2 * x, uv.y)).rgb;
    float3 e = Texture.Sample(TextureSampler, float2(uv.x, uv.y)).rgb;
    float3 f = Texture.Sample(TextureSampler, float2(uv.x + 2 * x, uv.y)).rgb;

    float3 g = Texture.Sample(TextureSampler, float2(uv.x - 2 * x, uv.y - 2 * y)).rgb;
    float3 h = Texture.Sample(TextureSampler, float2(uv.x, uv.y - 2 * y)).rgb;
    float3 i = Texture.Sample(TextureSampler, float2(uv.x + 2 * x, uv.y - 2 * y)).rgb;

    float3 j = Texture.Sample(TextureSampler, float2(uv.x - x, uv.y + y)).rgb;
    float3 k = Texture.Sample(TextureSampler, float2(uv.x + x, uv.y + y)).rgb;
    float3 l = Texture.Sample(TextureSampler, float2(uv.x - x, uv.y - y)).rgb;
    float3 m = Texture.Sample(TextureSampler, float2(uv.x + x, uv.y - y)).rgb;

    // Apply weighted distribution
    float3 outputColor;
    outputColor = e * 0.125;
    outputColor += (a + c + g + i) * 0.03125;
    outputColor += (b + d + f + h) * 0.0625;
    outputColor += (j + k + l + m) * 0.125;

    output.Color = float4(outputColor, 1);
}
