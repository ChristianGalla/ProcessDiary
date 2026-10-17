namespace ProcessDiary.Domain.Entities;

/// <summary>User-facing information about an application, such as its display name and category.</summary>
/// <param name="Id">Unique identifier of the metadata entry.</param>
/// <param name="ProcessName">Name of the process the metadata applies to.</param>
/// <param name="DisplayName">Friendly name shown in the UI.</param>
/// <param name="Category">Optional category used for grouping in reports.</param>
public sealed record ApplicationMetadata(
    Guid Id,
    string ProcessName,
    string DisplayName,
    string? Category);
