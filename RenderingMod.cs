using Exanite.Core.Properties;
using Exanite.Engine.Graphics;

namespace Exanite.GravitationalTetris;

public static class RenderingMod
{
    public static PropertyDefinition<ShaderModule> BloomDownFragmentModule = new("/Rendering/BloomDown.fragment.slang");
    public static PropertyDefinition<ShaderModule> BloomUpFragmentModule = new("/Rendering/BloomUp.fragment.slang");

    public static PropertyDefinition<ShaderModule> ToneMapFragmentModule = new("/Rendering/ToneMap.fragment.slang");
}
