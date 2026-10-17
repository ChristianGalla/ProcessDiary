using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Reports;

namespace ProcessDiary.Application.Reports;

/// <summary>Builds the report context from the clock, the stored sessions and the recording state.</summary>
public sealed class ReportContextProvider : IReportContextProvider
{
    private readonly IProcessSessionRepository _sessions;
    private readonly ICaptureScheduler _scheduler;
    private readonly TimeProvider _time;

    /// <summary>Initializes the provider.</summary>
    /// <param name="sessions">Source of the earliest recorded session.</param>
    /// <param name="scheduler">Source of the current recording start.</param>
    /// <param name="time">Provides the current time.</param>
    public ReportContextProvider(IProcessSessionRepository sessions, ICaptureScheduler scheduler, TimeProvider time)
    {
        _sessions = sessions;
        _scheduler = scheduler;
        _time = time;
    }

    /// <inheritdoc />
    public async Task<ReportContext> GetAsync(CancellationToken cancellationToken)
    {
        var first = await _sessions.GetEarliestStartAsync(cancellationToken).ConfigureAwait(false);
        return new ReportContext(_time.GetLocalNow(), first, _scheduler.RecordingStartedAt);
    }
}
