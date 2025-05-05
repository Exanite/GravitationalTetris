using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Avalonia;
using Exanite.Engine.Avalonia;
using Exanite.Engine.Framework;
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

            await using var game = CreateGame();
            try
            {
                game.Initialize();
                game.Run();
            }
            catch (Exception e)
            {
                LoggingUtility.LogProgramCrash(game.EngineSettings.Paths.LogsFolder, typeof(Program), e);

                exitCode = 1;
            }
        }

        Environment.Exit(exitCode);
    }

    private static EngineGame CreateGame()
    {
        var settings = new EngineSettings(CompanyName, GameName);
        return new EngineGame(settings, [new GravitationalTetrisGameModule()]);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        var game = CreateGame();
        return game.Initialize().Resolve<AvaloniaContext>().Start(true);
    }
}
