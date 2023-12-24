cbuffer Uniforms
{
    float4x4 View;
    float4x4 Projection;
};

struct Input
{
    // Per vertex
    uint VertexId : SV_VertexID;

    // Per instance
    float4 World : ATTRIB0;

    float4 Color : ATTRIB1;
    float2 Offset : ATTRIB2;
    float2 Size : ATTRIB3;
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

    output.Pos = mul(input.World * View * Projection, position);
    output.Uv = float2(uv.x * input.Size.x + input.Offset.x, uv.y * input.Size.y + input.Offset.y);
    output.Color = input.Color;
}
