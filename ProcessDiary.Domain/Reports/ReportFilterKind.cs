namespace ProcessDiary.Domain.Reports;

/// <summary>The kinds of time range a report can cover.</summary>
public enum ReportFilterKind
{
    /// <summary>A single calendar day.</summary>
    Day,

    /// <summary>A calendar week starting on Monday.</summary>
    Week,

    /// <summary>From the earliest stored session until now.</summary>
    SinceStartOfRecording,

    /// <summary>From the start of the current ProcessDiary recording run until now.</summary>
    SinceStartOfProcessDiaryRecording,

    /// <summary>The 24 hours before now.</summary>
    Last24Hours,

    /// <summary>A custom range between two date times.</summary>
    Between
}
