using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace LivinTheDreamToolkit.Gui.Views;

public class ConfirmDialog : Window
{
    public bool Result { get; private set; }

    public ConfirmDialog(string title, string message,
                         string yesText = "Yes", string noText = "No")
    {
        Title = title;
        Width = 480;
        Height = 220;
        CanResize = false;
        SystemDecorations = SystemDecorations.Full;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;

        var messageBlock = new TextBlock
        {
            Text = message,
            Margin = new Thickness(20, 20, 20, 12),
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Top,
        };

        var yesButton = new Button
        {
            Content = yesText,
            Width = 100,
            Height = 36,
            Margin = new Thickness(6),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            IsDefault = true,
        };
        yesButton.Click += (_, _) => { Result = true; Close(); };

        var noButton = new Button
        {
            Content = noText,
            Width = 100,
            Height = 36,
            Margin = new Thickness(6),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            IsCancel = true,
        };
        noButton.Click += (_, _) => { Result = false; Close(); };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(12, 0, 12, 12),
        };
        buttonPanel.Children.Add(noButton);
        buttonPanel.Children.Add(yesButton);

        var root = new DockPanel();
        DockPanel.SetDock(buttonPanel, Dock.Bottom);
        root.Children.Add(buttonPanel);
        root.Children.Add(messageBlock);

        Content = root;
    }

    public static async Task<bool> ShowAsync(Window owner, string title, string message,
                                             string yesText = "Yes", string noText = "No")
    {
        var dialog = new ConfirmDialog(title, message, yesText, noText);
        await dialog.ShowDialog(owner);
        return dialog.Result;
    }
}
