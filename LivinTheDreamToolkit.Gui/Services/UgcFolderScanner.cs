using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivinTheDreamToolkit.Gui.Models;

namespace LivinTheDreamToolkit.Gui.Services;
public static class UgcFolderScanner
{
    private const string UgctexSuffix = ".ugctex.zs";
    private const string ThumbSuffix = "_Thumb_ugctex.zs";
    private const string CanvasSuffix = ".canvas.zs";

    public static IReadOnlyList<UgcTextureEntry> Scan(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return Array.Empty<UgcTextureEntry>();

        var allFiles = Directory.EnumerateFiles(folderPath, "*.zs", SearchOption.TopDirectoryOnly)
            .ToList();

        var mainFiles = allFiles
            .Where(f => f.EndsWith(UgctexSuffix, StringComparison.OrdinalIgnoreCase)
                     && !f.EndsWith(ThumbSuffix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
            .ToList();

        var filesByName = allFiles.ToDictionary(
            f => Path.GetFileName(f).ToLowerInvariant(),
            f => f);

        var entries = new List<UgcTextureEntry>(mainFiles.Count);
        foreach (string mainPath in mainFiles)
        {
            string fileName = Path.GetFileName(mainPath);
            string stem = fileName[..^UgctexSuffix.Length];

            string? thumbPath = null;
            string thumbName = (stem + ThumbSuffix).ToLowerInvariant();
            if (filesByName.TryGetValue(thumbName, out string? t))
                thumbPath = t;

            string? canvasPath = null;
            string canvasName = (stem + CanvasSuffix).ToLowerInvariant();
            if (filesByName.TryGetValue(canvasName, out string? c))
                canvasPath = c;

            entries.Add(new UgcTextureEntry
            {
                Stem = stem,
                UgctexPath = mainPath,
                ThumbPath = thumbPath,
                CanvasPath = canvasPath,
            });
        }

        return entries;
    }
}
