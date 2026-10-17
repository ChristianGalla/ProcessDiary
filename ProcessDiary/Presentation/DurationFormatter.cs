namespace ProcessDiary.Presentation;

/// <summary>Formats durations for display.</summary>
public static class DurationFormatter
{
    /// <summary>Formats a duration as hours, minutes and seconds, for example <c>1 h 05 min</c>.</summary>
    /// <param name="duration">The duration to format.</param>
    /// <returns>The formatted text.</returns>
    public static string Format(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return $"{(int)duration.TotalHours} h {duration.Minutes:00} min";
        }

        return duration.TotalMinutes >= 1
            ? $"{duration.Minutes} min {duration.Seconds:00} s"
            : $"{Math.Max(0, duration.Seconds)} s";
    }
}
