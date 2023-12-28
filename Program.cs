﻿using System;
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
            await using var logger = LoggingUtility.CreateBootstrapLogger(GameDirectories.LogsDirectory);
            try
            {
                Thread.CurrentThread.Name = "Main";

                await using var game = new Game1();

                game.Run();
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Unhandled exception");

                exitCode = 1;
            }
        }

        Environment.Exit(exitCode);
    }
}
