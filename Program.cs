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
        var logger = LoggingModule.CreateBootstrapLogger(GameDirectories.LogsDirectory);
        try
        {
            Thread.CurrentThread.Name = "Main";

            await using var game = new Game1();

            game.Run();
        }
        catch (Exception e)
        {
            logger.Fatal(e, "Unhandled exception");

            Environment.Exit(1);
        }
    }
}
