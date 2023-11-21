using System;
using System.IO;

namespace Exanite.WarGames;

public static class GameDirectories
{
    public static string InstallDirectory => AppContext.BaseDirectory;
    
    public static string ContentDirectory
    {
        get
        {
#if DEBUG
            return Path.Join(InstallDirectory, "../../../Content");
#else
            return Path.Join(InstallDirectory, "Content");
#endif
        }
    }

    public static string SaveDirectory => Path.Join(AppContext.BaseDirectory, "Data");
}
