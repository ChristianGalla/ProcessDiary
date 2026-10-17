namespace ProcessDiary.Application.Reports;

/// <summary>One row of a report.</summary>
/// <param name="Key">Grouping key, such as a process name or category.</param>
/// <param name="DisplayName">Name shown in the UI.</param>
/// <param name="Duration">Total time for the key within the range.</param>
/// <param name="Percentage">Share of the report total, from 0 to 100.</param>
public sealed record ReportEntry(string Key, string DisplayName, TimeSpan Duration, double Percentage);
