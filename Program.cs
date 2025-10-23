using Exanite.Engine.Framework;

namespace Exanite.GravitationalTetris;

public static class Program
{
    public const string CompanyName = "Exanite";
    public const string GameName = "GravitationalTetris";

    public static void Main(string[] args)
    {
        var settings = new EngineSettings(CompanyName, GameName);
        new EngineHost(settings, [new GravitationalTetrisGameModule()]).Launch();
    }
}
