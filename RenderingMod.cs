using Exanite.Core.Properties;
using Exanite.Engine.Rendering;

namespace Exanite.GravitationalTetris;

public static class RenderingMod
{
    public static PropertyDefinition<Shader> ScreenShader = new("/Rendering/Screen.v.hlsl");

    public static PropertyDefinition<Shader> PassthroughShader = new("/Rendering/Passthrough.p.hlsl");

    public static PropertyDefinition<Shader> BloomDownShader = new("/Rendering/BloomDown.p.hlsl");
    public static PropertyDefinition<Shader> BloomUpShader = new("/Rendering/BloomUp.p.hlsl");

    public static PropertyDefinition<Shader> ToneMapShader = new("/Rendering/ToneMap.p.hlsl");
}
