using ProcessDiary.Domain.Reports;

namespace ProcessDiary.ViewModels;

/// <summary>A selectable report period.</summary>
/// <param name="Kind">The kind of filter the option creates.</param>
/// <param name="Label">The text shown to the user.</param>
public sealed record FilterOption(ReportFilterKind Kind, string Label);
