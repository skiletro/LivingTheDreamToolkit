using System.IO;

namespace LivinTheDreamToolkit.Gui.Models;

public sealed class UgcTextureEntry
{
    public required string Stem { get; init; }

    public required string UgctexPath { get; init; }

    public string? ThumbPath { get; init; }

    public string? CanvasPath { get; init; }

    public string Directory => Path.GetDirectoryName(UgctexPath) ?? ".";

    public bool HasThumb => ThumbPath is not null && File.Exists(ThumbPath);

    public bool HasCanvas => CanvasPath is not null && File.Exists(CanvasPath);

    public string DisplayName => Stem;

    public override string ToString() => DisplayName;
}
