using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProcessDiary.Domain;
using ProcessDiary.Services;
using ProcessDiary.ViewModels;

namespace ProcessDiary;

/// <summary>The main window with the recording status and the page navigation.</summary>
public sealed partial class MainWindow : Window
{
    /// <summary>Initializes the window.</summary>
    /// <param name="viewModel">The view model of the shell.</param>
    /// <param name="navigation">Shows the pages.</param>
    /// <param name="shell">Receives this window for services that need its handle.</param>
    public MainWindow(MainViewModel viewModel, INavigationService navigation, IShellWindowService shell)
    {
        ViewModel = viewModel;
        InitializeComponent();

        shell.Attach(this);
        navigation.Initialize(PageHost);
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1100, 760));
        AppWindow.Closing += OnClosing;
        Navigation.SelectedItem = Navigation.MenuItems[0];
    }

    /// <summary>Gets the view model of the shell.</summary>
    public MainViewModel ViewModel { get; }

    private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ViewModel.Navigate(NavigationKeys.Settings);
        }
        else if (args.SelectedItemContainer?.Tag is string key)
        {
            ViewModel.Navigate(key);
        }
    }

    // While recording, closing the window keeps ProcessDiary running in the tray; the tray icon and the
    // overlay keep the recording visible. Otherwise closing the window exits the application.
    private async void OnClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        args.Cancel = true;
        if (ViewModel.State is RecordingState.Recording or RecordingState.Paused)
        {
            sender.Hide();
            return;
        }

        await ViewModel.ExitCommand.ExecuteAsync(null);
    }
}
