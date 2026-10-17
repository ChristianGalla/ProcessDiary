using ProcessDiary.Application.Reports;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Reports;

namespace ProcessDiary.Application.Timeline;

/// <summary>Combines sessions and screenshots into a timeline.</summary>
public sealed class TimelineService : ITimelineService
{
    private readonly IProcessSessionRepository _sessions;
    private readonly IScreenshotRepository _screenshots;
    private readonly IScreenshotFileStore _fileStore;
    private readonly IReportContextProvider _contextProvider;

    /// <summary>Initializes the service.</summary>
    /// <param name="sessions">Source of sessions.</param>
    /// <param name="screenshots">Source of screenshot records.</param>
    /// <param name="fileStore">Used to detect missing image files.</param>
    /// <param name="contextProvider">Resolves the filter context.</param>
    public TimelineService(
        IProcessSessionRepository sessions,
        IScreenshotRepository screenshots,
        IScreenshotFileStore fileStore,
        IReportContextProvider contextProvider)
    {
        _sessions = sessions;
        _screenshots = screenshots;
        _fileStore = fileStore;
        _contextProvider = contextProvider;
    }

    /// <inheritdoc />
    public async Task<ActivityTimeline> GetTimelineAsync(ReportFilter filter, CancellationToken cancellationToken)
    {
        var context = await _contextProvider.GetAsync(cancellationToken).ConfigureAwait(false);
        var range = filter.Resolve(context);
        var sessions = await _sessions.GetOverlappingAsync(range.From, range.To, cancellationToken).ConfigureAwait(false);

        var entries = new List<TimelineEntry>(sessions.Count);
        foreach (var session in sessions.OrderByDescending(s => s.StartedAt))
        {
            var duration = ReportService.ClippedDuration(session, range, context.Now);
            if (duration <= TimeSpan.Zero)
            {
                continue;
            }

            var screenshots = await _screenshots.GetBySessionAsync(session.Id, cancellationToken).ConfigureAwait(false);
            var items = screenshots
                .OrderBy(s => s.CapturedAt)
                .Select(s => new TimelineScreenshot(s, _fileStore.Exists(s)))
                .ToList();
            entries.Add(new TimelineEntry(session, duration, items));
        }

        return new ActivityTimeline(range, entries);
    }
}
