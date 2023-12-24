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
    uint VertexId : SV_VertexID;
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
    float4 positionUvs[4];
    positionUvs[0] = float4(-0.5f, 0.5f, 0, 1);
    positionUvs[1] = float4(-0.5f, -0.5f, 0, 0);
    positionUvs[2] = float4(0.5f, 0.5f, 1, 1);
    positionUvs[3] = float4(0.5f, -0.5f, 1, 0);

    float4 position = float4(positionUvs[input.VertexId].xy, 0, 1);
    float2 uv = float2(positionUvs[input.VertexId].zw);

    output.Pos = mul(World * View * Projection, position);
    output.Uv = float2(uv.x * Size.x + Offset.x, uv.y * Size.y + Offset.y);
    output.Color = Color;
}
