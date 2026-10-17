using CommunityToolkit.Mvvm.ComponentModel;
using ProcessDiary.Domain.Reports;

namespace ProcessDiary.ViewModels;

/// <summary>Lets the user choose the period shown by the timeline and the statistics.</summary>
public sealed partial class FilterViewModel : ObservableObject
{
    private static readonly IReadOnlyList<FilterOption> AllOptions =
    [
        new(ReportFilterKind.Last24Hours, "Last 24 hours"),
        new(ReportFilterKind.Day, "Day"),
        new(ReportFilterKind.Week, "Week"),
        new(ReportFilterKind.SinceStartOfRecording, "Since the first recording"),
        new(ReportFilterKind.SinceStartOfProcessDiaryRecording, "Since this recording started"),
        new(ReportFilterKind.Between, "Between two dates")
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDate))]
    [NotifyPropertyChangedFor(nameof(ShowRange))]
    private FilterOption _selectedOption = AllOptions[0];

    [ObservableProperty]
    private DateTimeOffset? _selectedDate = DateTimeOffset.Now.Date;

    [ObservableProperty]
    private DateTimeOffset? _fromDate = DateTimeOffset.Now.Date;

    [ObservableProperty]
    private TimeSpan _fromTime = TimeSpan.Zero;

    [ObservableProperty]
    private DateTimeOffset? _toDate = DateTimeOffset.Now.Date;

    [ObservableProperty]
    private TimeSpan _toTime = new(23, 59, 0);

    /// <summary>Gets the selectable periods.</summary>
    public IReadOnlyList<FilterOption> Options => AllOptions;

    /// <summary>Gets a value indicating whether a single date must be chosen.</summary>
    public bool ShowDate => SelectedOption.Kind is ReportFilterKind.Day or ReportFilterKind.Week;

    /// <summary>Gets a value indicating whether a start and an end must be chosen.</summary>
    public bool ShowRange => SelectedOption.Kind == ReportFilterKind.Between;

    /// <summary>Creates the report filter described by the current choices.</summary>
    /// <param name="filter">The filter when the choices are valid.</param>
    /// <param name="error">A message for the user when the choices are not valid.</param>
    /// <returns>True when a filter could be created.</returns>
    public bool TryCreateFilter(out ReportFilter filter, out string? error)
    {
        filter = ReportFilter.Last24Hours();
        error = null;

        switch (SelectedOption.Kind)
        {
            case ReportFilterKind.Day:
            case ReportFilterKind.Week:
                if (SelectedDate is not { } date)
                {
                    error = "Choose a date.";
                    return false;
                }

                var day = DateOnly.FromDateTime(date.Date);
                filter = SelectedOption.Kind == ReportFilterKind.Day ? ReportFilter.Day(day) : ReportFilter.Week(day);
                return true;

            case ReportFilterKind.Between:
                if (FromDate is not { } from || ToDate is not { } to)
                {
                    error = "Choose a start and an end.";
                    return false;
                }

                var start = ToLocal(from, FromTime);
                var end = ToLocal(to, ToTime);
                if (end < start)
                {
                    error = "The end must not be before the start.";
                    return false;
                }

                filter = ReportFilter.Between(start, end);
                return true;

            case ReportFilterKind.SinceStartOfRecording:
                filter = ReportFilter.SinceStartOfRecording();
                return true;

            case ReportFilterKind.SinceStartOfProcessDiaryRecording:
                filter = ReportFilter.SinceStartOfProcessDiaryRecording();
                return true;

            default:
                filter = ReportFilter.Last24Hours();
                return true;
        }
    }

    private static DateTimeOffset ToLocal(DateTimeOffset date, TimeSpan time) =>
        new(DateTime.SpecifyKind(date.Date + time, DateTimeKind.Unspecified));
}
