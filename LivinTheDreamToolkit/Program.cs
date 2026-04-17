namespace TomodachiCanvasExport;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            PrintUsage();
            return;
        }

        string first = args[0].ToLowerInvariant();
        if (first == "export")
        {
            RunExport(args[1..], NoSrgbFromArgs(args[1..]));
            return;
        }
        if (first == "import")
        {
            RunImport(args[1..], NoSrgbFromArgs(args[1..]));
            return;
        }

        bool noSrgb = NoSrgbFromArgs(args);
        string? inputPath = args.FirstOrDefault(a => !a.StartsWith('-'));
        if (inputPath is null)
        {
            Console.Error.WriteLine("Error: no input file specified.");
            PrintUsage();
            Environment.Exit(1);
        }

        string lower = inputPath.ToLowerInvariant();
        if (lower.EndsWith(".png"))
        {
            RunImport(args, noSrgb);
        }
        else if (lower.EndsWith(".zs"))
        {
            RunExport(args, noSrgb);
        }
        else
        {
            Console.Error.WriteLine(
                $"Error: don't know what to do with '{inputPath}'. " +
                "Expected a .png (for import) or a .zs file (for export).");
            Environment.Exit(1);
        }
    }

    private static bool NoSrgbFromArgs(string[] args) =>
        args.Any(a => a == "--no-srgb"); // This is kinda hacky, I should auto detect it, I'll work that in before release....

    private static void PrintUsage()
    {
        Console.WriteLine("Tomodachi Life Face Paint Canvas Tool");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  Drag a .png file onto the .exe to import it.");
        Console.WriteLine("  Drag a .canvas.zs / .ugctex.zs / _Thumb_ugctex.zs file onto the .exe to export it."); // _Thumb can be a in game render, for most things. So probably best to re-open it in palette shop to generate it.
        Console.WriteLine();
        Console.WriteLine("Or from the command line:");
        Console.WriteLine("  LivinTheDreamToolkit <file> [--no-srgb]");
        Console.WriteLine("  LivinTheDreamToolkit export <file.zs> [--no-srgb]");
        Console.WriteLine("  LivinTheDreamToolkit import <file.png> [--no-srgb]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --no-srgb    Skip sRGB <-> linear color conversion");
        Console.WriteLine("  -h, --help   Show this help message");
    }

    private static void RunExport(string[] args, bool noSrgb)
    {
        string? inputPath = args.FirstOrDefault(a => !a.StartsWith('-'));
        if (inputPath is null)
        {
            Console.Error.WriteLine("Error: no input file specified.");
            PrintUsage();
            Environment.Exit(1);
        }

        try
        {
            TextureProcessor.ExportFileToPngs(inputPath, noSrgb, Console.WriteLine);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static void RunImport(string[] args, bool noSrgb)
    {
        string? inputPath = args.FirstOrDefault(a => !a.StartsWith('-'));
        if (inputPath is null)
        {
            Console.Error.WriteLine("Error: no input image specified.");
            PrintUsage();
            Environment.Exit(1);
        }

        string dir = Path.GetDirectoryName(inputPath) ?? ".";
        string stem = Path.Combine(dir, Path.GetFileNameWithoutExtension(inputPath));

        try
        {
            TextureProcessor.ImportPng(inputPath, stem, writeThumb: true, noSrgb, Console.WriteLine);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
