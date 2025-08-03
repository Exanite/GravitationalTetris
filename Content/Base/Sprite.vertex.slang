struct UniformData
{
    row_major float4x4 View;
    row_major float4x4 Projection;
};

struct Input
{
    // Per vertex
    uint VertexId : SV_VERTEXID;

    // Per instance
    float4 WorldR1 : ATTRIB0;
    float4 WorldR2 : ATTRIB1;
    float4 WorldR3 : ATTRIB2;
    float4 WorldR4 : ATTRIB3;

    float4 Color : ATTRIB4;
    float2 UvOffset : ATTRIB5;
    float2 UvSize : ATTRIB6;
    uint TextureIndex : ATTRIB7;
};

struct Output
{
    float4 Pos : SV_POSITION;
    float2 Uv : TEX_COORD;
    float4 Color: COLOR;
    uint TextureIndex : TEX_INDEX;
};

[[vk::binding(0, 0)]] ConstantBuffer<UniformData> Uniforms;

void main(
    in Input input,
    out Output output)
{
    float4 positionUvs[4];
    positionUvs[0] = float4(-0.5f, -0.5f, 0, 0);
    positionUvs[1] = float4(0.5f, -0.5f, 1, 0);
    positionUvs[2] = float4(-0.5f, 0.5f, 0, 1);
    positionUvs[3] = float4(0.5f, 0.5f, 1, 1);

    float4 position = float4(positionUvs[input.VertexId].xy, 0, 1);
    float2 uv = float2(positionUvs[input.VertexId].zw);

    float4x4 world = float4x4(input.WorldR1, input.WorldR2, input.WorldR3, input.WorldR4);

    output.Pos = position;
    output.Pos = mul(output.Pos, world);
    output.Pos = mul(output.Pos, Uniforms.View);
    output.Pos = mul(output.Pos, Uniforms.Projection);

    output.Uv = float2(uv.x * input.UvSize.x + input.UvOffset.x, uv.y * input.UvSize.y + input.UvOffset.y);
    output.Color = input.Color;
    output.TextureIndex = input.TextureIndex;
}
