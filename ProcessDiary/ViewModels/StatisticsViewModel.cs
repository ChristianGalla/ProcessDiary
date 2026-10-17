using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ProcessDiary.Application.Reports;
using ProcessDiary.Presentation;

namespace ProcessDiary.ViewModels;

/// <summary>Shows how much time was spent per application or per category.</summary>
public sealed partial class StatisticsViewModel : ObservableObject
{
    private readonly IReportService _reports;
    private readonly IReportContextProvider _contextProvider;
    private readonly ILogger<StatisticsViewModel> _logger;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _groupByCategory;

    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private string _totalText = string.Empty;

    /// <summary>Initializes the view model.</summary>
    /// <param name="reports">Builds the reports.</param>
    /// <param name="contextProvider">Resolves the period.</param>
    /// <param name="logger">The logger.</param>
    public StatisticsViewModel(
        IReportService reports, IReportContextProvider contextProvider, ILogger<StatisticsViewModel> logger)
    {
        _reports = reports;
        _contextProvider = contextProvider;
        _logger = logger;
    }

    /// <summary>Gets the period selection.</summary>
    public FilterViewModel Filter { get; } = new();

    /// <summary>Gets the rows of the report, longest time first.</summary>
    public ObservableCollection<ReportEntryViewModel> Entries { get; } = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (!Filter.TryCreateFilter(out var filter, out var error))
        {
            Message = error;
            return;
        }

        IsLoading = true;
        Message = null;
        try
        {
            var context = await _contextProvider.GetAsync(CancellationToken.None);
            var report = GroupByCategory
                ? await _reports.GetTimePerCategoryAsync(filter, context, CancellationToken.None)
                : await _reports.GetTimePerApplicationAsync(filter, context, CancellationToken.None);

            Entries.Clear();
            foreach (var entry in report.Entries)
            {
                Entries.Add(new ReportEntryViewModel(entry));
            }

            TotalText = $"Total: {DurationFormatter.Format(report.Total)}";
            if (report.Entries.Count == 0)
            {
                Message = "No activity was recorded in this period.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Statistics could not be loaded");
            Message = "The statistics could not be loaded.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnGroupByCategoryChanged(bool value) => LoadCommand.Execute(null);
}
