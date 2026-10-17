using Microsoft.Extensions.Logging.Abstractions;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;
using ProcessDiary.Infrastructure.Settings;
using ProcessDiary.Infrastructure.Storage;

namespace ProcessDiary.Tests.Infrastructure;

/// <summary>Tests for <see cref="ScreenshotFileStore"/>.</summary>
public sealed class ScreenshotFileStoreTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ProcessDiaryTests", Guid.NewGuid().ToString("N"));
    private readonly ScreenshotFileStore _store;

    /// <summary>Initializes the store with a temporary root folder.</summary>
    public ScreenshotFileStoreTests()
    {
        var settings = new DiarySettingsProvider();
        settings.Update(DiarySettingsProvider.CreateDefaults() with { ScreenshotFolder = _root });
        _store = new ScreenshotFileStore(settings, NullLogger<ScreenshotFileStore>.Instance);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static ProcessSession Session() =>
        new(Guid.NewGuid(), "code.exe", @"C:\code.exe", "t", DateTimeOffset.Now);

    private static ScreenshotResult Result(DateTimeOffset at) => new([1, 2, 3], 4, 5, at);

    /// <summary>The file is stored in a dated folder using the documented name.</summary>
    [Fact]
    public async Task SaveAsync_WritesFileInDatedFolderWithExpectedName()
    {
        var at = new DateTimeOffset(2026, 3, 1, 14, 5, 9, TimeSpan.Zero);

        var screenshot = await _store.SaveAsync(Result(at), Session(), CancellationToken.None);

        Assert.Equal(Path.Combine(_root, "2026", "03", "01", "code_2026-03-01T14-05-09.png"), screenshot.FilePath);
        Assert.True(_store.Exists(screenshot));
        Assert.Equal(4, screenshot.Width);
    }

    /// <summary>Two captures within the same second keep both files.</summary>
    [Fact]
    public async Task SaveAsync_SameSecond_DoesNotOverwrite()
    {
        var at = new DateTimeOffset(2026, 3, 1, 14, 5, 9, TimeSpan.Zero);

        var first = await _store.SaveAsync(Result(at), Session(), CancellationToken.None);
        var second = await _store.SaveAsync(Result(at), Session(), CancellationToken.None);

        Assert.NotEqual(first.FilePath, second.FilePath);
        Assert.True(_store.Exists(first));
        Assert.True(_store.Exists(second));
    }

    /// <summary>Deleting files skips files that are already missing.</summary>
    [Fact]
    public async Task DeleteFilesAsync_MissingFile_IsSkipped()
    {
        var screenshot = await _store.SaveAsync(Result(DateTimeOffset.Now), Session(), CancellationToken.None);

        var deleted = await _store.DeleteFilesAsync([screenshot.FilePath, Path.Combine(_root, "missing.png")], CancellationToken.None);

        Assert.Equal(1, deleted);
        Assert.False(_store.Exists(screenshot));
    }
}
