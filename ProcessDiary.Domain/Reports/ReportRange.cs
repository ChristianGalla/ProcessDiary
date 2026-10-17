namespace ProcessDiary.Domain.Reports;

/// <summary>A resolved, half-open time range (From, To).</summary>
/// <param name="From">Inclusive start.</param>
/// <param name="To">Exclusive end.</param>
public sealed record ReportRange(DateTimeOffset From, DateTimeOffset To);
