cbuffer Constants
{
    float4x4 WorldViewProjection;
    // float4x4 World;
    // float4x4 View;
    // float4x4 Projection;
    // float4 Color;
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
};

void main(
    in Input input,
    out Output output)
{
    output.Pos = mul(WorldViewProjection, float4(input.Pos, 1.0));
    output.Uv = input.Uv;
}
