using ProcessDiary.Application.Reports;
using ProcessDiary.Presentation;

namespace ProcessDiary.ViewModels;

/// <summary>A row of the statistics list.</summary>
public sealed class ReportEntryViewModel
{
    /// <summary>Initializes the row.</summary>
    /// <param name="entry">The report entry to show.</param>
    public ReportEntryViewModel(ReportEntry entry)
    {
        Name = entry.DisplayName;
        DurationText = DurationFormatter.Format(entry.Duration);
        Percentage = entry.Percentage;
        PercentageText = $"{entry.Percentage:0.#} %";
    }

    /// <summary>Gets the application or category name.</summary>
    public string Name { get; }

    /// <summary>Gets the time spent.</summary>
    public string DurationText { get; }

    /// <summary>Gets the share of the total, from 0 to 100.</summary>
    public double Percentage { get; }

    /// <summary>Gets the share of the total as text.</summary>
    public string PercentageText { get; }
}
