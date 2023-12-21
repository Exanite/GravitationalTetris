cbuffer Constants
{
    float4x4 World;
    float4x4 View;
    float4x4 Projection;

    float4 Color;

    float2 Offset;
    float2 Size;
};

struct Input
{
    float3 Pos : ATTRIB0;
    float2 Uv : ATTRIB1;
};

struct Output
{
    float4 Pos : SV_POSITION;
    float2 Uv : TEX_COORD;
    float4 Color: COLOR;
};

void main(
    in Input input,
    out Output output)
{
    output.Pos = mul(World * View * Projection, float4(input.Pos, 1.0));
    output.Uv = float2(input.Uv.x * Size.x + Offset.x, input.Uv.y * Size.y + Offset.y);
    output.Color = Color;
}
