using ProcessDiary.Domain.Abstractions;

namespace ProcessDiary.ViewModels;

/// <summary>A selectable overlay placement corner.</summary>
/// <param name="Corner">The display work-area corner.</param>
/// <param name="Label">The text shown to the user.</param>
public sealed record OverlayCornerOption(OverlayCorner Corner, string Label);
