using ProcessDiary.Domain.Reports;

namespace ProcessDiary.Application.Reports;

/// <summary>Supplies the time context needed to resolve report filters.</summary>
public interface IReportContextProvider
{
    /// <summary>Builds the current context.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The context with the current time, first recorded time and current recording start.</returns>
    Task<ReportContext> GetAsync(CancellationToken cancellationToken);
}
