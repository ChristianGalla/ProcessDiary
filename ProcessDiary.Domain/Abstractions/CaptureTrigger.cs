namespace ProcessDiary.Domain.Abstractions;

/// <summary>Defines why a screenshot was taken.</summary>
public enum CaptureTrigger
{
    /// <summary>A recurring timer elapsed.</summary>
    Interval,

    /// <summary>The foreground application changed.</summary>
    ActivityChange,

    /// <summary>The user requested it.</summary>
    Manual
}
