using System;
using System.Threading;
using System.Threading.Tasks;

namespace Exanite.GravitationalTetris;

public static class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        Thread.CurrentThread.Name = "Main";

        await using (var game = new Game1())
        {
            game.Run();
        }
    }
}
