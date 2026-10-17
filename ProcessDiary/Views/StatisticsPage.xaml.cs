using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProcessDiary.ViewModels;

namespace ProcessDiary.Views;

/// <summary>Shows time spent per application or category.</summary>
public sealed partial class StatisticsPage : Page
{
    /// <summary>Initializes the page.</summary>
    /// <param name="viewModel">The view model of the page.</param>
    public StatisticsPage(StatisticsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    /// <summary>Gets the view model of the page.</summary>
    public StatisticsViewModel ViewModel { get; }

    private void OnLoaded(object sender, RoutedEventArgs e) => ViewModel.LoadCommand.Execute(null);
}
