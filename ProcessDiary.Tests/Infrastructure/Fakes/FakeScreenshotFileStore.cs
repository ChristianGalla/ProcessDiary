using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Tests.Infrastructure.Fakes;

/// <summary>File store that stores nothing on disk.</summary>
internal sealed class FakeScreenshotFileStore : IScreenshotFileStore
{
    /// <inheritdoc />
    public Task<Screenshot> SaveAsync(ScreenshotResult result, ProcessSession session, CancellationToken cancellationToken) =>
        Task.FromResult(new Screenshot(Guid.NewGuid(), session.Id, $@"C:\fake\{Guid.NewGuid()}.png", result.CapturedAt, result.Width, result.Height));

    /// <inheritdoc />
    public bool Exists(Screenshot screenshot) => true;

    /// <inheritdoc />
    public Task<int> DeleteFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken) => Task.FromResult(0);
}
