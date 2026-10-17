using ProcessDiary.Domain.Reports;

namespace ProcessDiary.Application.Reports;

/// <summary>A time-per-key report for a resolved range.</summary>
/// <param name="Range">The range the report covers.</param>
/// <param name="Total">Total time across all entries.</param>
/// <param name="Entries">Entries ordered by descending duration.</param>
public sealed record ActivityReport(ReportRange Range, TimeSpan Total, IReadOnlyList<ReportEntry> Entries);
