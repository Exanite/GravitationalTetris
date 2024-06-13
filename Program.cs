using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Avalonia;
using Exanite.Engine.Avalonia;
using Exanite.GravitationalTetris.Features.UserInterface;
using Exanite.Logging;

namespace Exanite.GravitationalTetris;

public static class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        var exitCode = 0;
        {
            try
            {
                Thread.CurrentThread.Name = "Main";

                await using var game = new Game1();
                try
                {
                    game.Run();
                }
                catch (Exception e)
                {
                    LoggingUtility.LogProgramCrash(GameDirectories.LogsDirectory, typeof(Program), e);

                    exitCode = 1;
                }
            }
            catch (Exception e)
            {
                LoggingUtility.LogProgramCrash(GameDirectories.LogsDirectory, typeof(Program), e);

                exitCode = 1;
            }
        }

        Environment.Exit(exitCode);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return new Game1().Container.Resolve<AvaloniaBuilder<App>>().Start(true);
    }
}
