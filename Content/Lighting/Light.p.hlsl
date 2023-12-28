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
    output.Color = float4(input.Uv.x, input.Uv.y, 0, 0);
}
