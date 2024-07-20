using System;
using System.IO;

namespace Exanite.GravitationalTetris;

public static class GamePaths
{
    public static string InstallFolder => AppContext.BaseDirectory;

    public static string ContentFolder
    {
        get
        {
#if DEBUG
            return Path.Join(InstallFolder, "../../../../Content");
#else
            return Path.Join(InstallFolder, "Content");
#endif
        }
    }

    public static string PersistentDataFolder => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Exanite", "GravitationalTetris");

    public static string LogsFolder => Path.Join(PersistentDataFolder, "Logs");
}
