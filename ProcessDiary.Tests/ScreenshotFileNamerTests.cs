using ProcessDiary.Domain.Screenshots;

namespace ProcessDiary.Tests;

/// <summary>Tests for <see cref="ScreenshotFileNamer"/>.</summary>
public class ScreenshotFileNamerTests
{
    private static readonly DateTimeOffset At = new(2026, 3, 7, 14, 5, 9, TimeSpan.FromHours(1));

    [Theory]
    [InlineData("code.exe", "code_2026-03-07T14-05-09.png")]
    [InlineData("chrome", "chrome_2026-03-07T14-05-09.png")]
    [InlineData("", "desktop_2026-03-07T14-05-09.png")]
    public void BuildFileName_UsesPattern(string process, string expected) =>
        Assert.Equal(expected, ScreenshotFileNamer.BuildFileName(process, At));

    [Fact]
    public void BuildFileName_ReplacesInvalidCharacters()
    {
        var name = ScreenshotFileNamer.BuildFileName("a:b*c", At);
        Assert.Equal(-1, name.IndexOfAny(Path.GetInvalidFileNameChars()));
    }

    [Fact]
    public void BuildFolder_UsesYearMonthDay() =>
        Assert.Equal(Path.Combine("root", "2026", "03", "07"), ScreenshotFileNamer.BuildFolder("root", At));
}
