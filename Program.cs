using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Avalonia;
using Exanite.Engine.Avalonia;
using Exanite.Engine.EngineUsage;
using Exanite.Logging;

namespace Exanite.GravitationalTetris;

public static class Program
{
    public const string CompanyName = "Exanite";
    public const string GameName = "GravitationalTetris";

    [STAThread]
    public static async Task Main(string[] args)
    {
        var exitCode = 0;
        {
            Thread.CurrentThread.Name = "Main";
            var config = new EngineSettings(CompanyName, GameName);

            await using var game = new Game1(config);

            try
            {
                game.Initialize();
                game.Run();
            }
            catch (Exception e)
            {
                LoggingUtility.LogProgramCrash(config.Paths.LogsFolder, typeof(Program), e);

                exitCode = 1;
            }
        }

        Environment.Exit(exitCode);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        var config = new EngineSettings(CompanyName, GameName);
        return new Game1(config).Initialize().Resolve<AvaloniaContext>().Start(true);
    }
}
