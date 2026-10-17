using ProcessDiary.Domain.Abstractions;

namespace ProcessDiary.ViewModels;

/// <summary>A selectable capture mode.</summary>
/// <param name="Mode">The capture mode.</param>
/// <param name="Label">The text shown to the user.</param>
public sealed record CaptureModeOption(CaptureMode Mode, string Label);
