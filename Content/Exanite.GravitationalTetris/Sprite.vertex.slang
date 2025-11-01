struct UniformData
{
    row_major float4x4 View;
    row_major float4x4 Projection;
};

struct Input
{
    // Per vertex
    uint VertexId : SV_VertexId;

    // Per instance
    float4 WorldR1 : Attrib0;
    float4 WorldR2 : Attrib1;
    float4 WorldR3 : Attrib2;
    float4 WorldR4 : Attrib3;

    float4 Color : Attrib4;
    float2 UvOffset : Attrib5;
    float2 UvSize : Attrib6;
    uint TextureIndex : Attrib7;
};

struct Output
{
    float4 Position : SV_Position;
    float2 Uv : Uv;
    float4 Color: Color;
    uint TextureIndex : TexIndex;
};

[[vk::binding(0, 0)]] ConstantBuffer<UniformData> Uniforms;

[shader("vertex")]
void main(in Input input, out Output output)
{
    // This is intentionally out of order for compatibility purposes
    float4 positionUvs[4];
    positionUvs[0] = float4(-0.5f, -0.5f, 0, 0);
    positionUvs[2] = float4(0.5f, -0.5f, 1, 0);
    positionUvs[1] = float4(-0.5f, 0.5f, 0, 1);
    positionUvs[3] = float4(0.5f, 0.5f, 1, 1);

    float4 position = float4(positionUvs[input.VertexId].xy, 0, 1);
    float2 uv = float2(positionUvs[input.VertexId].zw);

    float4x4 world = float4x4(input.WorldR1, input.WorldR2, input.WorldR3, input.WorldR4);

    output.Position = position;
    output.Position = mul(output.Position, world);
    output.Position = mul(output.Position, Uniforms.View);
    output.Position = mul(output.Position, Uniforms.Projection);

    output.Uv = float2(uv.x * input.UvSize.x + input.UvOffset.x, uv.y * input.UvSize.y + input.UvOffset.y);
    output.Color = input.Color;
    output.TextureIndex = input.TextureIndex;
}
