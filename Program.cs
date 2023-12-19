using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Exanite.GravitationalTetris;

public static class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        Thread.CurrentThread.Name = "Main";

        try
        {
            await using var game = new Game1();
            game.Run();
        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    private static void HandleException(Exception e)
    {
        Directory.CreateDirectory(GameDirectories.PersistentDataDirectory);
        using (var stream = File.Open(Path.Join(GameDirectories.PersistentDataDirectory, "Game.log"), FileMode.Append))
        using (var streamWriter = new StreamWriter(stream))
        {
            streamWriter.WriteLine(e);
        }

        Console.Error.WriteLine(e);

        Environment.Exit(-1);
    }
}
