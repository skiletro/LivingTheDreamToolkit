using System.Globalization;
using LivinTheDreamToolkit.Gui.Models;

namespace LivinTheDreamToolkit.Gui.Services;
public static class BackupService
{
    public const string BackupFolderName = "Backup";

    public static string BackupEntry(UgcTextureEntry entry)
    {
        string backupRoot = Path.Combine(entry.Directory, BackupFolderName);
        Directory.CreateDirectory(backupRoot);

        string numberedFolder = GetNextNumberedFolder(backupRoot);
        Directory.CreateDirectory(numberedFolder);

        var filesToBackup = new List<string>();
        if (File.Exists(entry.UgctexPath)) filesToBackup.Add(entry.UgctexPath);
        if (entry.ThumbPath is not null && File.Exists(entry.ThumbPath))
            filesToBackup.Add(entry.ThumbPath);
        if (entry.CanvasPath is not null && File.Exists(entry.CanvasPath))
            filesToBackup.Add(entry.CanvasPath);

        foreach (string src in filesToBackup)
        {
            string dst = Path.Combine(numberedFolder, Path.GetFileName(src));
            File.Copy(src, dst, overwrite: false);
        }

        return numberedFolder;
    }

    private static string GetNextNumberedFolder(string backupRoot)
    {
        int highest = 0;
        int widthSeen = 3;

        foreach (string dir in Directory.EnumerateDirectories(backupRoot))
        {
            string name = Path.GetFileName(dir);
            if (int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out int n))
            {
                if (n > highest) highest = n;
                if (name.Length > widthSeen) widthSeen = name.Length;
            }
        }

        int next = highest + 1;
        string nextName = next.ToString(CultureInfo.InvariantCulture).PadLeft(widthSeen, '0');
        return Path.Combine(backupRoot, nextName);
    }
}
