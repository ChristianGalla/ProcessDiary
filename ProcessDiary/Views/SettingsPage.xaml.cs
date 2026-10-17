using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProcessDiary.ViewModels;

namespace ProcessDiary.Views;

/// <summary>Edits settings and offers the privacy actions.</summary>
public sealed partial class SettingsPage : Page
{
    /// <summary>Initializes the page.</summary>
    /// <param name="viewModel">The view model of the page.</param>
    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    /// <summary>Gets the view model of the page.</summary>
    public SettingsViewModel ViewModel { get; }

    private void OnLoaded(object sender, RoutedEventArgs e) => ViewModel.Reload();
}
