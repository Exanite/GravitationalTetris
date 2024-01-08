using System;
using System.Threading;
using System.Threading.Tasks;
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
                LoggingUtility.RemoveProgramCrashLog(GameDirectories.LogsDirectory);
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
}
