using ProcessDiary.Domain.Reports;

namespace ProcessDiary.Application.Timeline;

/// <summary>Builds the timeline of sessions and screenshots.</summary>
public interface ITimelineService
{
    /// <summary>Gets the sessions and screenshots of a range.</summary>
    /// <param name="filter">Time range to cover; the same filters as the reports.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The timeline, newest session first.</returns>
    Task<ActivityTimeline> GetTimelineAsync(ReportFilter filter, CancellationToken cancellationToken);
}
