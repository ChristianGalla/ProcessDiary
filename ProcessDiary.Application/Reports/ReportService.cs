using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;
using ProcessDiary.Domain.Reports;

namespace ProcessDiary.Application.Reports;

/// <summary>Both reports share one pipeline; only the grouping key differs.</summary>
public sealed class ReportService : IReportService
{
    /// <summary>Key used for applications without a category.</summary>
    public const string UncategorizedKey = "Uncategorized";

    private readonly IProcessSessionRepository _sessions;
    private readonly IApplicationMetadataRepository _metadata;

    /// <summary>Initializes the service.</summary>
    /// <param name="sessions">Source of sessions.</param>
    /// <param name="metadata">Source of display names and categories.</param>
    public ReportService(IProcessSessionRepository sessions, IApplicationMetadataRepository metadata)
    {
        _sessions = sessions;
        _metadata = metadata;
    }

    /// <inheritdoc />
    public Task<ActivityReport> GetTimePerApplicationAsync(ReportFilter filter, ReportContext context, CancellationToken cancellationToken) =>
        BuildAsync(filter, context, byCategory: false, cancellationToken);

    /// <inheritdoc />
    public Task<ActivityReport> GetTimePerCategoryAsync(ReportFilter filter, ReportContext context, CancellationToken cancellationToken) =>
        BuildAsync(filter, context, byCategory: true, cancellationToken);

    private async Task<ActivityReport> BuildAsync(ReportFilter filter, ReportContext context, bool byCategory, CancellationToken cancellationToken)
    {
        var range = filter.Resolve(context);
        var sessions = await _sessions.GetOverlappingAsync(range.From, range.To, cancellationToken).ConfigureAwait(false);
        var metadata = (await _metadata.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .GroupBy(m => m.ProcessName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var totals = new Dictionary<string, (string Display, TimeSpan Duration)>(StringComparer.OrdinalIgnoreCase);
        foreach (var session in sessions)
        {
            var duration = ClippedDuration(session, range, context.Now);
            if (duration <= TimeSpan.Zero)
            {
                continue;
            }

            metadata.TryGetValue(session.ProcessName, out var meta);
            var (key, display) = byCategory
                ? CategoryKey(meta)
                : (session.ProcessName, meta?.DisplayName ?? session.ProcessName);

            totals[key] = totals.TryGetValue(key, out var existing)
                ? (existing.Display, existing.Duration + duration)
                : (display, duration);
        }

        var total = totals.Values.Aggregate(TimeSpan.Zero, (sum, t) => sum + t.Duration);
        var entries = totals
            .OrderByDescending(kv => kv.Value.Duration)
            .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new ReportEntry(kv.Key, kv.Value.Display, kv.Value.Duration,
                total > TimeSpan.Zero ? kv.Value.Duration / total * 100d : 0d))
            .ToList();

        return new ActivityReport(range, total, entries);
    }

    private static (string Key, string Display) CategoryKey(ApplicationMetadata? meta)
    {
        var category = string.IsNullOrWhiteSpace(meta?.Category) ? UncategorizedKey : meta!.Category!;
        return (category, category);
    }

    /// <summary>Returns the part of a session that falls inside a range; open sessions run until <paramref name="now"/>.</summary>
    /// <param name="session">The session to measure.</param>
    /// <param name="range">The range to clip to.</param>
    /// <param name="now">End time used for open sessions.</param>
    /// <returns>The clipped duration; zero or negative when there is no overlap.</returns>
    internal static TimeSpan ClippedDuration(ProcessSession session, ReportRange range, DateTimeOffset now)
    {
        var start = session.StartedAt > range.From ? session.StartedAt : range.From;
        var rawEnd = session.EndedAt ?? now;
        var end = rawEnd < range.To ? rawEnd : range.To;
        return end - start;
    }
}
