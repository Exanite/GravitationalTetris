using System.Threading.Tasks;
using Exanite.Engine.Framework;

namespace Exanite.GravitationalTetris;

public static class Program
{
    public const string CompanyName = "Exanite";
    public const string GameName = "GravitationalTetris";

    public static readonly EngineGameHost Host = new(args =>
    {
        var settings = new EngineSettings(CompanyName, GameName);
        return new EngineGame(settings, [new GravitationalTetrisGameModule()]);
    });

    public static Task Main(string[] args)
    {
        return Host.Run(args);
    }
}
