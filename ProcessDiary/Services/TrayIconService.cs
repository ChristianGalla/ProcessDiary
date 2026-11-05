using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;
using Microsoft.UI.Xaml.Controls;
using ProcessDiary.Domain;
using ProcessDiary.ViewModels;

namespace ProcessDiary.Services;

/// <summary>The tray icon that shows the recording state and offers the main actions.</summary>
public sealed class TrayIconService : IDisposable
{
    private readonly MainViewModel _viewModel;
    private readonly IShellWindowService _shell;
    private TaskbarIcon? _icon;
    private MenuFlyoutItem? _primaryItem;
    private string? _currentIconName;

    /// <summary>Initializes the service.</summary>
    /// <param name="viewModel">Provides the state and commands.</param>
    /// <param name="shell">Shows the main window.</param>
    public TrayIconService(MainViewModel viewModel, IShellWindowService shell)
    {
        _viewModel = viewModel;
        _shell = shell;
    }

    /// <summary>Creates the tray icon. Must be called on the UI thread.</summary>
    public void Initialize()
    {
        var open = new MenuFlyoutItem { Text = "Open ProcessDiary", Command = new RelayCommand(_shell.Show) };
        _primaryItem = new MenuFlyoutItem { Command = _viewModel.PrimaryActionCommand };
        var capture = new MenuFlyoutItem { Text = "Take screenshot now", Command = _viewModel.CaptureNowCommand };
        var exit = new MenuFlyoutItem { Text = "Exit", Command = _viewModel.ExitCommand };

        var menu = new MenuFlyout();
        menu.Items.Add(open);
        menu.Items.Add(new MenuFlyoutSeparator());
        menu.Items.Add(_primaryItem);
        menu.Items.Add(capture);
        menu.Items.Add(new MenuFlyoutSeparator());
        menu.Items.Add(exit);

        _icon = new TaskbarIcon
        {
            ContextFlyout = menu,
            LeftClickCommand = new RelayCommand(_shell.Show),
            NoLeftClickDelay = true
        };

        Refresh();
        _viewModel.PropertyChanged += (_, _) => Refresh();
        _icon.ForceCreate(enablesEfficiencyMode: false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _icon?.Dispose();
        _icon = null;
    }

    private static string GetIconName(RecordingState state) => state switch
    {
        RecordingState.Recording => "Recording",
        RecordingState.Paused => "Paused",
        RecordingState.Error => "Error",
        _ => "Disabled"
    };

    private void Refresh()
    {
        if (_icon is null || _primaryItem is null)
        {
            return;
        }

        _icon.ToolTipText = $"ProcessDiary – {_viewModel.StatusText}";
        var name = GetIconName(_viewModel.State);
        if (name != _currentIconName)
        {
            // TaskbarIcon takes ownership and disposes the previous icon, so a new instance is needed each time.
            _icon.Icon = new System.Drawing.Icon(Path.Combine(AppContext.BaseDirectory, "Assets", "Tray", $"ProcessDiary-{name}.ico"));
            _currentIconName = name;
        }

        _primaryItem.Text = _viewModel.PrimaryActionText;
    }
}
