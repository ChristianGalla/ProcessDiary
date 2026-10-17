namespace ProcessDiary.Domain.Reports;

/// <summary>Time context needed to resolve a filter; supplied by the caller so resolution stays pure.</summary>
/// <param name="Now">Current time (carries the local offset).</param>
/// <param name="FirstRecordedAt">Start of the earliest stored session, if any.</param>
/// <param name="CurrentRecordingStartedAt">When the current ProcessDiary recording run began, if any.</param>
public sealed record ReportContext(DateTimeOffset Now, DateTimeOffset? FirstRecordedAt, DateTimeOffset? CurrentRecordingStartedAt);
