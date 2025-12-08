using Exanite.Engine.Framework;

namespace Exanite.GravitationalTetris;

public static class Program
{
    public const string CompanyName = "Exanite";
    public const string GameName = "GravitationalTetris";

    public static int Main(string[] args)
    {
        var settings = new EngineSettings(CompanyName, GameName);
        var engine = new EngineRoot(settings, [new GravitationalTetrisGameModule()]);

        return engine.Launch(args);
    }
}
