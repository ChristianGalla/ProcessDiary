namespace ProcessDiary.Infrastructure.Persistence;

/// <summary>Converts between <see cref="DateTimeOffset"/> and the UTC ticks stored in the database.</summary>
internal static class TimestampConverter
{
    /// <summary>Converts a time to UTC ticks.</summary>
    /// <param name="value">The time.</param>
    /// <returns>The UTC ticks.</returns>
    public static long ToTicks(DateTimeOffset value) => value.UtcTicks;

    /// <summary>Converts a time to UTC ticks, keeping null.</summary>
    /// <param name="value">The time, or null.</param>
    /// <returns>The UTC ticks, or null.</returns>
    public static long? ToTicks(DateTimeOffset? value) => value?.UtcTicks;

    /// <summary>Converts stored ticks to a local time.</summary>
    /// <param name="ticks">The UTC ticks.</param>
    /// <returns>The time in the local offset.</returns>
    public static DateTimeOffset FromTicks(long ticks) =>
        new DateTimeOffset(ticks, TimeSpan.Zero).ToLocalTime();

    /// <summary>Converts stored ticks to a local time, keeping null.</summary>
    /// <param name="ticks">The UTC ticks, or null.</param>
    /// <returns>The time in the local offset, or null.</returns>
    public static DateTimeOffset? FromTicks(long? ticks) =>
        ticks is { } value ? FromTicks(value) : null;
}
