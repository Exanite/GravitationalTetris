using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Exanite.Logging;
using Serilog;

namespace Exanite.GravitationalTetris;

public static class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        var logger = LoggingModule.CreateBootstrapLogger(GameDirectories.LogsDirectory);

        Thread.CurrentThread.Name = "Main";

        try
        {
            await using var game = new Game1();
            logger = game.Container.Resolve<ILogger>();

            game.Run();
        }
        catch (Exception e)
        {
            logger.Fatal(e, "Unhandled exception");
        }
    }
}
