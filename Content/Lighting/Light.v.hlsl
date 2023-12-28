struct Input
{
    uint VertexId : SV_VertexID;
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
    float4 positionUvs[4];
    positionUvs[0] = float4(-0.5f, 0.5f, 0, 1);
    positionUvs[1] = float4(-0.5f, -0.5f, 0, 0);
    positionUvs[2] = float4(0.5f, 0.5f, 1, 1);
    positionUvs[3] = float4(0.5f, -0.5f, 1, 0);

    output.Pos = float4(positionUvs[input.VertexId].xy, 0, 1);
    output.Uv = float2(positionUvs[input.VertexId].zw);
}
