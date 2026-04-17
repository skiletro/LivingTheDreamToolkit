using System.IO;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LivinTheDreamToolkit.Gui.Services;
public static class BitmapConverter
{
    public static Bitmap ToAvaloniaBitmap(Image<Rgba32> image)
    {
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        ms.Position = 0;
        return new Bitmap(ms);
    }
}
