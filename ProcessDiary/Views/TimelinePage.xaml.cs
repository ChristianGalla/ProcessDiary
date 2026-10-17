using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProcessDiary.ViewModels;

namespace ProcessDiary.Views;

/// <summary>Shows sessions and screenshots on a timeline.</summary>
public sealed partial class TimelinePage : Page
{
    /// <summary>Initializes the page.</summary>
    /// <param name="viewModel">The view model of the page.</param>
    public TimelinePage(TimelineViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    /// <summary>Gets the view model of the page.</summary>
    public TimelineViewModel ViewModel { get; }

    private void OnLoaded(object sender, RoutedEventArgs e) => ViewModel.LoadCommand.Execute(null);
}
