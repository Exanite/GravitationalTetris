using Exanite.Engine.Framework;

namespace Exanite.GravitationalTetris;

public static class Program
{
    public const string CompanyName = "Exanite";
    public const string ProgramName = "GravitationalTetris";

    public static int Main(string[] args)
    {
        var settings = new EngineSettings(CompanyName, ProgramName);
        var engine = EngineRoot.Create(settings, [new GravitationalTetrisGameModule()]);

        return engine.Launch(args);
    }
}
