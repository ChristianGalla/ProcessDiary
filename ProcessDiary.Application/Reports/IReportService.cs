using ProcessDiary.Domain.Reports;

namespace ProcessDiary.Application.Reports;

/// <summary>Builds activity reports.</summary>
public interface IReportService
{
    /// <summary>Builds a report of time spent per application.</summary>
    /// <param name="filter">Time range to cover.</param>
    /// <param name="context">Time context used to resolve the filter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The report.</returns>
    Task<ActivityReport> GetTimePerApplicationAsync(ReportFilter filter, ReportContext context, CancellationToken cancellationToken);

    /// <summary>Builds a report of time spent per category.</summary>
    /// <param name="filter">Time range to cover.</param>
    /// <param name="context">Time context used to resolve the filter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The report.</returns>
    Task<ActivityReport> GetTimePerCategoryAsync(ReportFilter filter, ReportContext context, CancellationToken cancellationToken);
}
