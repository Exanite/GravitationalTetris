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
    output.Color = Texture.Sample(TextureSampler, float2(input.Uv.x, 1 - input.Uv.y));
}
