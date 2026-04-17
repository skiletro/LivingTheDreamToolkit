using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using LivinTheDreamToolkit.Gui.Models;
using LivinTheDreamToolkit.Gui.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TomodachiCanvasExport;

namespace LivinTheDreamToolkit.Gui.Views;

public partial class EditorView : UserControl
{
    private MainWindow? _mainWindow;
    private string _folderPath = string.Empty;
    private List<UgcTextureEntry> _entries = new();
    private ListBox _fileList = null!;
    private Avalonia.Controls.Image _previewImage = null!;
    private TextBlock _placeholderText = null!;
    private TextBlock _statusText = null!;
    private TextBlock _folderPathText = null!;
    private Button _importButton = null!;
    private Button _exportButton = null!;

    public EditorView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _fileList = this.FindControl<ListBox>("FileList")!;
        _previewImage = this.FindControl<Avalonia.Controls.Image>("PreviewImage")!;
        _placeholderText = this.FindControl<TextBlock>("PlaceholderText")!;
        _statusText = this.FindControl<TextBlock>("StatusText")!;
        _folderPathText = this.FindControl<TextBlock>("FolderPathText")!;
        _importButton = this.FindControl<Button>("ImportButton")!;
        _exportButton = this.FindControl<Button>("ExportButton")!;
    }
    public void Initialize(MainWindow mainWindow, string folderPath, IReadOnlyList<UgcTextureEntry> entries)
    {
        _mainWindow = mainWindow;
        _folderPath = folderPath;
        _entries = new List<UgcTextureEntry>(entries);

        _folderPathText.Text = folderPath;
        ToolTip.SetTip(_folderPathText, folderPath);
        _fileList.ItemsSource = _entries;

        if (_entries.Count > 0)
            _fileList.SelectedIndex = 0;
    }

    private UgcTextureEntry? SelectedEntry => _fileList.SelectedItem as UgcTextureEntry;

    private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainWindow?.ShowHome();
    }

    private async void FileList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var entry = SelectedEntry;
        if (entry is null)
        {
            _importButton.IsEnabled = false;
            _exportButton.IsEnabled = false;
            _previewImage.IsVisible = false;
            _placeholderText.IsVisible = true;
            _statusText.Text = string.Empty;
            return;
        }

        _importButton.IsEnabled = true;
        _exportButton.IsEnabled = true;

        await LoadPreviewAsync(entry);
    }
    private async Task LoadPreviewAsync(UgcTextureEntry entry)
    {
        _statusText.Text = $"Loading {entry.DisplayName}…";
        _placeholderText.IsVisible = false;
        _previewImage.IsVisible = false;

        string pathAtStart = entry.UgctexPath;

        try
        {
            Bitmap bmp = await Task.Run(() =>
            {
                using Image<Rgba32> img = TextureProcessor.DecodeFile(pathAtStart);
                return BitmapConverter.ToAvaloniaBitmap(img);
            });
            if (SelectedEntry?.UgctexPath != pathAtStart)
                return;

            _previewImage.Source = bmp;
            _previewImage.IsVisible = true;
            _placeholderText.IsVisible = false;

            string info = $"{entry.DisplayName}   ({bmp.PixelSize.Width}x{bmp.PixelSize.Height})";
            if (entry.HasThumb) info += "   • thumb present";
            if (entry.HasCanvas) info += "   • canvas present";
            _statusText.Text = info;
        }
        catch (Exception ex)
        {
            if (SelectedEntry?.UgctexPath != pathAtStart) return;
            _previewImage.IsVisible = false;
            _placeholderText.IsVisible = true;
            _placeholderText.Text = "Failed to decode texture";
            _statusText.Text = $"Error: {ex.Message}";
        }
    }
    private async void ExportButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var entry = SelectedEntry;
        if (entry is null || _mainWindow is null) return;

        var savedFile = await _mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export texture as PNG",
            SuggestedFileName = entry.DisplayName,
            DefaultExtension = "png",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("PNG image") { Patterns = new[] { "*.png" } }
            }
        });

        if (savedFile is null) return;

        string? targetPath = savedFile.TryGetLocalPath();
        if (string.IsNullOrEmpty(targetPath))
        {
            _statusText.Text = "Couldn't resolve the chosen save location.";
            return;
        }

        SetBusy(true);
        _statusText.Text = $"Exporting {entry.DisplayName}…";

        try
        {
            string path = targetPath;
            await Task.Run(() =>
            {
                using Image<Rgba32> img = TextureProcessor.DecodeFile(entry.UgctexPath);
                img.SaveAsPng(path);
            });

            _statusText.Text = $"Exported to {targetPath}";
        }
        catch (Exception ex)
        {
            _statusText.Text = $"Export failed: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void ImportButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var entry = SelectedEntry;
        if (entry is null || _mainWindow is null) return;

        var openFiles = await _mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = $"Import PNG to replace {entry.DisplayName}",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("PNG image") { Patterns = new[] { "*.png" } },
                new FilePickerFileType("All images")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.tga", "*.webp" }
                }
            }
        });

        if (openFiles is null || openFiles.Count == 0) return;

        string? pngPath = openFiles[0].TryGetLocalPath();
        if (string.IsNullOrEmpty(pngPath))
        {
            _statusText.Text = "Couldn't resolve the chosen PNG to a local path.";
            return;
        }

        bool regenerateThumb = false;
        if (entry.HasThumb)
        {
            regenerateThumb = await ConfirmDialog.ShowAsync(
                _mainWindow,
                "Regenerate thumbnail?",
                $"'{entry.DisplayName}' has a thumbnail file (_Thumb_ugctex.zs).\n\n" +
                "Do you also want to regenerate the thumbnail from the PNG you're importing?\n\n" +
                "Choose \"No\" to leave the existing thumbnail untouched. " +
                "You can replace the thumbnail file manually later if you want a different image for it.",
                yesText: "Yes, regenerate",
                noText: "No, leave it alone");
        }

        SetBusy(true);
        _statusText.Text = $"Backing up {entry.DisplayName}…";

        try
        {
            string backupFolder = await Task.Run(() => BackupService.BackupEntry(entry));

            string destStem = Path.Combine(entry.Directory, entry.Stem);
            string pngPathLocal = pngPath;
            bool writeThumb = regenerateThumb;
            bool writeCanvas = entry.HasCanvas;
            string originalUgctex = entry.UgctexPath;

            await Task.Run(() =>
            {
                TextureProcessor.ImportPng(
                    pngPath: pngPathLocal,
                    destStem: destStem,
                    writeCanvas: writeCanvas,
                    writeThumb: writeThumb,
                    noSrgb: false,
                    originalUgctexPath: originalUgctex,
                    log: _ => { });
            });

            var fresh = await Task.Run(() => UgcFolderScanner.Scan(_folderPath));
            await Dispatcher.UIThread.InvokeAsync(() => RefreshEntries(fresh, entry.Stem));

            string thumbNote = entry.HasThumb
                ? (regenerateThumb ? " (thumb regenerated)" : " (thumb left unchanged)")
                : "";
            _statusText.Text = $"Imported successfully{thumbNote}. Originals saved to {backupFolder}";
        }
        catch (Exception ex)
        {
            _statusText.Text = $"Import failed: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void RefreshEntries(IReadOnlyList<UgcTextureEntry> fresh, string? stemToSelect)
    {
        _entries = new List<UgcTextureEntry>(fresh);
        _fileList.ItemsSource = _entries;

        if (stemToSelect is not null)
        {
            int idx = _entries.FindIndex(e =>
                string.Equals(e.Stem, stemToSelect, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) _fileList.SelectedIndex = idx;
        }
    }

    private void SetBusy(bool busy)
    {
        _importButton.IsEnabled = !busy && SelectedEntry is not null;
        _exportButton.IsEnabled = !busy && SelectedEntry is not null;
        _fileList.IsEnabled = !busy;
    }
}
