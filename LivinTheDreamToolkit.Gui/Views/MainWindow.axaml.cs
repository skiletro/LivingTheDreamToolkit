using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LivinTheDreamToolkit.Gui.Models;
using System.Collections.Generic;

namespace LivinTheDreamToolkit.Gui.Views;

public partial class MainWindow : Window
{
    private ContentControl _rootContent = null!;

    public MainWindow()
    {
        InitializeComponent();
        _rootContent = this.FindControl<ContentControl>("RootContent")!;
        ShowHome();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void ShowHome()
    {
        var home = new HomeView();
        home.FolderOpened += (folder, entries) => ShowEditor(folder, entries);
        _rootContent.Content = home;
    }

    public void ShowEditor(string folderPath, IReadOnlyList<UgcTextureEntry> entries)
    {
        var editor = new EditorView();
        editor.Initialize(this, folderPath, entries);
        _rootContent.Content = editor;
    }
}
