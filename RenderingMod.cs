using Exanite.Core.Properties;
using Exanite.Engine.Rendering;

namespace Exanite.GravitationalTetris;

public static class RenderingMod
{
    public static PropertyDefinition<ShaderModule> ScreenVertexModule = new("/Rendering/Screen.vertex.hlsl");

    public static PropertyDefinition<ShaderModule> PassthroughFragmentModule = new("/Rendering/Passthrough.fragment.hlsl");

    public static PropertyDefinition<ShaderModule> BloomDownFragmentModule = new("/Rendering/BloomDown.fragment.hlsl");
    public static PropertyDefinition<ShaderModule> BloomUpFragmentModule = new("/Rendering/BloomUp.fragment.hlsl");

    public static PropertyDefinition<ShaderModule> ToneMapFragmentModule = new("/Rendering/ToneMap.fragment.hlsl");
}
