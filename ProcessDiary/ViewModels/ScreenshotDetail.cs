namespace ProcessDiary.ViewModels;

/// <summary>A labeled value shown in the details of a screenshot.</summary>
/// <param name="Label">Name of the value.</param>
/// <param name="Value">The value as text.</param>
public sealed record ScreenshotDetail(string Label, string Value);
