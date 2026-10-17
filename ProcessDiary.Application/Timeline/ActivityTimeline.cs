using ProcessDiary.Domain.Reports;

namespace ProcessDiary.Application.Timeline;

/// <summary>The sessions of a resolved range, newest first.</summary>
/// <param name="Range">The range the timeline covers.</param>
/// <param name="Entries">The sessions with their screenshots.</param>
public sealed record ActivityTimeline(ReportRange Range, IReadOnlyList<TimelineEntry> Entries);
