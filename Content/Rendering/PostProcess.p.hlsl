cbuffer Uniforms
{
    float Time;
}

Texture2D Texture;
SamplerState TextureSampler;

struct Input
{
    float2 Uv : TEX_COORD;
};

struct Output
{
    float4 Color : SV_TARGET;
};

float Luminance(float3 rgb)
{
    return 0.2125 * rgb.r + 0.7154 * rgb.g + 0.0721 * rgb.b;
}

float Reinhard(float x)
{
    return x / (9.6 * 25);
}

// Conversion functions from http://brucelindbloom.com/index.html?Math.html
float3 Convert_xyYToXYZ(float3 xyY)
{
    float x = xyY.x * xyY.z / xyY.y;
    float y = xyY.z;
    float z = (1 - xyY.x - xyY.y) * xyY.z / xyY.y;

    return float3(x, y, z);
}

float3 Convert_XYZToxyY(float3 xyz)
{
    return float3(0, 0, 0);
}

float3 Convert_XYZToRGB(float3 xyz)
{
    return float3(0, 0, 0);
}

float3 Convert_RGBToXYZ(float3 rgb)
{
    return float3(0, 0, 0);
}

void main(
    in Input input,
    out Output output)
{
    float2 uv = float2(input.Uv.x, 1 - input.Uv.y);
    float4 color = Texture.Sample(TextureSampler, uv);

    output.Color = color;
    // output.Color = float4(Reinhard(color.x), Reinhard(color.y), Reinhard(color.z), 1);
}
