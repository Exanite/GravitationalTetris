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
        var bootstrapLogName = "bootstrap-crash";
        var exitCode = 0;
        {
            try
            {
                LoggingUtility.RemoveBootstrapLogs(GameDirectories.LogsDirectory, bootstrapLogName);

                Thread.CurrentThread.Name = "Main";

                await using var game = new Game1();
                game.Run();
            }
            catch (Exception e)
            {
                await using var logger = LoggingUtility.CreateBootstrapLogger(GameDirectories.LogsDirectory, bootstrapLogName);
                logger.Fatal(e, "Unhandled exception");

                exitCode = 1;
            }
        }

        Environment.Exit(exitCode);
    }
}
