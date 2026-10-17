namespace ProcessDiary.Domain;

/// <summary>Always-visible recording status.</summary>
public enum RecordingState
{
    /// <summary>Activity is being recorded.</summary>
    Recording,

    /// <summary>Recording was paused by the user.</summary>
    Paused,

    /// <summary>Recording is turned off.</summary>
    Disabled,

    /// <summary>Recording stopped because of an error.</summary>
    Error
}
