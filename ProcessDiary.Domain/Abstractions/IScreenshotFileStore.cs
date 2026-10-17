using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Domain.Abstractions;

/// <summary>Stores screenshot image files on disk.</summary>
public interface IScreenshotFileStore
{
    /// <summary>Writes a captured image to the configured folder.</summary>
    /// <param name="result">The captured image.</param>
    /// <param name="session">The session the screenshot belongs to.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The screenshot record describing the stored file.</returns>
    Task<Screenshot> SaveAsync(ScreenshotResult result, ProcessSession session, CancellationToken cancellationToken);

    /// <summary>Determines whether the image file of a screenshot still exists.</summary>
    /// <param name="screenshot">The screenshot to check.</param>
    /// <returns>True when the file exists.</returns>
    bool Exists(Screenshot screenshot);

    /// <summary>Deletes image files. Files that are already missing are skipped.</summary>
    /// <param name="filePaths">Full paths of the files to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The number of files that were deleted.</returns>
    Task<int> DeleteFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken);
}
