using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Application.Timeline;

/// <summary>One session on the timeline with its screenshots.</summary>
/// <param name="Session">The session.</param>
/// <param name="Duration">Time of the session that falls inside the timeline range.</param>
/// <param name="Screenshots">Screenshots taken during the session.</param>
public sealed record TimelineEntry(ProcessSession Session, TimeSpan Duration, IReadOnlyList<TimelineScreenshot> Screenshots);
