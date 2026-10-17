namespace ProcessDiary.Domain.Reports;

/// <summary>Describes which time range a report covers.</summary>
/// <param name="Kind">The kind of filter.</param>
/// <param name="Anchor">Day used by <see cref="ReportFilterKind.Day"/> and <see cref="ReportFilterKind.Week"/>.</param>
/// <param name="From">Start used by <see cref="ReportFilterKind.Between"/>.</param>
/// <param name="To">End used by <see cref="ReportFilterKind.Between"/>.</param>
public sealed record ReportFilter(ReportFilterKind Kind, DateOnly? Anchor = null, DateTimeOffset? From = null, DateTimeOffset? To = null)
{
    /// <summary>Creates a filter for one calendar day.</summary>
    /// <param name="day">The day.</param>
    /// <returns>The filter.</returns>
    public static ReportFilter Day(DateOnly day) => new(ReportFilterKind.Day, day);

    /// <summary>Creates a filter for the Monday-based week containing a day.</summary>
    /// <param name="dayInWeek">Any day within the week.</param>
    /// <returns>The filter.</returns>
    public static ReportFilter Week(DateOnly dayInWeek) => new(ReportFilterKind.Week, dayInWeek);

    /// <summary>Creates a filter from the earliest stored session until now.</summary>
    /// <returns>The filter.</returns>
    public static ReportFilter SinceStartOfRecording() => new(ReportFilterKind.SinceStartOfRecording);

    /// <summary>Creates a filter from the start of the current ProcessDiary recording run until now.</summary>
    /// <returns>The filter.</returns>
    public static ReportFilter SinceStartOfProcessDiaryRecording() => new(ReportFilterKind.SinceStartOfProcessDiaryRecording);

    /// <summary>Creates a filter for the last 24 hours.</summary>
    /// <returns>The filter.</returns>
    public static ReportFilter Last24Hours() => new(ReportFilterKind.Last24Hours);

    /// <summary>Creates a filter for a custom range.</summary>
    /// <param name="from">Start of the range.</param>
    /// <param name="to">End of the range.</param>
    /// <returns>The filter.</returns>
    public static ReportFilter Between(DateTimeOffset from, DateTimeOffset to) => new(ReportFilterKind.Between, null, from, to);

    /// <summary>Resolves the filter to a concrete half-open range. Weeks start on Monday.</summary>
    /// <param name="context">The time context used for relative filters.</param>
    /// <returns>The resolved range.</returns>
    /// <exception cref="ArgumentException">A custom range ends before it starts.</exception>
    public ReportRange Resolve(ReportContext context)
    {
        var now = context.Now;
        switch (Kind)
        {
            case ReportFilterKind.Day:
            {
                var start = StartOfDay(Anchor ?? DateOnly.FromDateTime(now.DateTime), now.Offset);
                return new ReportRange(start, start.AddDays(1));
            }
            case ReportFilterKind.Week:
            {
                var day = Anchor ?? DateOnly.FromDateTime(now.DateTime);
                var daysFromMonday = ((int)day.DayOfWeek + 6) % 7;
                var start = StartOfDay(day.AddDays(-daysFromMonday), now.Offset);
                return new ReportRange(start, start.AddDays(7));
            }
            case ReportFilterKind.SinceStartOfRecording:
                return new ReportRange(context.FirstRecordedAt ?? now, now);
            case ReportFilterKind.SinceStartOfProcessDiaryRecording:
                return new ReportRange(context.CurrentRecordingStartedAt ?? now, now);
            case ReportFilterKind.Last24Hours:
                return new ReportRange(now.AddHours(-24), now);
            case ReportFilterKind.Between:
            {
                var from = From ?? throw new InvalidOperationException("Between filter requires From.");
                var to = To ?? throw new InvalidOperationException("Between filter requires To.");
                if (to < from)
                {
                    throw new ArgumentException("The end of the range must not be before the start.");
                }
                return new ReportRange(from, to);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(Kind));
        }
    }

    private static DateTimeOffset StartOfDay(DateOnly day, TimeSpan offset) =>
        new(day.ToDateTime(TimeOnly.MinValue), offset);
}
