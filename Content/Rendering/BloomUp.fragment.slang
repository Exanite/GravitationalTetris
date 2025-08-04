cbuffer Uniforms
{
    float2 FilterStep;
    float Alpha;
}

[[vk::binding(0, 0)]] Texture2D Texture;
[[vk::binding(1, 0)]] SamplerState TextureSampler;

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

    float x = FilterStep.x;
    float y = FilterStep.y;

    // Take 9 samples around current texel
    float3 a = Texture.Sample(TextureSampler, float2(uv.x - x, uv.y + y)).rgb;
    float3 b = Texture.Sample(TextureSampler, float2(uv.x, uv.y + y)).rgb;
    float3 c = Texture.Sample(TextureSampler, float2(uv.x + x, uv.y + y)).rgb;

    float3 d = Texture.Sample(TextureSampler, float2(uv.x - x, uv.y)).rgb;
    float3 e = Texture.Sample(TextureSampler, float2(uv.x, uv.y)).rgb;
    float3 f = Texture.Sample(TextureSampler, float2(uv.x + x, uv.y)).rgb;

    float3 g = Texture.Sample(TextureSampler, float2(uv.x - x, uv.y - y)).rgb;
    float3 h = Texture.Sample(TextureSampler, float2(uv.x, uv.y - y)).rgb;
    float3 i = Texture.Sample(TextureSampler, float2(uv.x + x, uv.y - y)).rgb;

    // Apply weighted distribution
    float3 outputColor;
    outputColor = e * 4.0;
    outputColor += (b + d + f + h) * 2.0;
    outputColor += (a + c + g + i);
    outputColor *= 1.0 / 16.0;

    output.Color = float4(outputColor * Alpha, 0); // Alpha channel is ignored by pipeline
}
