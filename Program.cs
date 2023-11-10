using System;
using System.Threading;
using System.Threading.Tasks;

namespace Exanite.Extraction;

public class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        Thread.CurrentThread.Name = "Main";

        await using (var game = new ExtractionGame())
        {
            game.Run();
        }
    }
}
