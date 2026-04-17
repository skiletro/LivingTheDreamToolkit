using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using LivinTheDreamToolkit.Gui.Models;
using LivinTheDreamToolkit.Gui.Services;

namespace LivinTheDreamToolkit.Gui.Views;

public partial class HomeView : UserControl
{
    public event Action<string, IReadOnlyList<UgcTextureEntry>>? FolderOpened;

    public HomeView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OpenFolderButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var status = this.FindControl<TextBlock>("StatusText")!;
        status.IsVisible = false;

        var window = GetTopLevelWindow();
        if (window is null)
            return;

        var provider = window.StorageProvider;

        var folders = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select your Tomodachi Life UGC save folder",
            AllowMultiple = false,
        });

        if (folders is null || folders.Count == 0)
            return;

        var folder = folders[0];
        string? path = folder.TryGetLocalPath();
        if (string.IsNullOrEmpty(path))
        {
            ShowError(status, "Couldn't resolve the selected folder to a local path. " +
                              "Please pick a regular folder on your disk.");
            return;
        }

        try
        {
            IReadOnlyList<UgcTextureEntry> entries = await Task.Run(() => UgcFolderScanner.Scan(path));

            if (entries.Count == 0)
            {
                ShowError(status,
                    $"No .ugctex.zs files found in:\n{path}\n\n" +
                    "Make sure you picked the folder that actually contains the UGC textures.");
                return;
            }

            FolderOpened?.Invoke(path, entries);
        }
        catch (Exception ex)
        {
            ShowError(status, $"Error scanning folder: {ex.Message}");
        }
    }

    private static void ShowError(TextBlock status, string message)
    {
        status.Text = message;
        status.IsVisible = true;
    }

    private Window? GetTopLevelWindow()
    {
        if (TopLevel.GetTopLevel(this) is Window w)
            return w;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime life)
            return life.MainWindow;

        return null;
    }
}
