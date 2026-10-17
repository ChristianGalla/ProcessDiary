namespace ProcessDiary.Domain.Abstractions;

/// <summary>Defines which area a screenshot covers.</summary>
public enum CaptureMode
{
    /// <summary>Only the foreground window.</summary>
    ActiveWindow,

    /// <summary>The whole desktop.</summary>
    WholeDesktop
}
