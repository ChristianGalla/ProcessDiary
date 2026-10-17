using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Application.Privacy;

/// <summary>Lets the user remove recorded data and applies the automatic retention rule.</summary>
public interface IDataManagementService
{
    /// <summary>Deletes all sessions, screenshot records and screenshot files.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when everything is deleted.</returns>
    Task DeleteAllAsync(CancellationToken cancellationToken);

    /// <summary>Deletes the given screenshots, both the records and the image files.</summary>
    /// <param name="screenshots">The screenshots to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the screenshots are deleted.</returns>
    Task DeleteScreenshotsAsync(IReadOnlyCollection<Screenshot> screenshots, CancellationToken cancellationToken);

    /// <summary>Deletes the given sessions together with their screenshots.</summary>
    /// <param name="sessionIds">Identifiers of the sessions to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the sessions are deleted.</returns>
    Task DeleteSessionsAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken);

    /// <summary>Deletes data older than the configured retention period. Does nothing when no period is set.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when old data is deleted.</returns>
    Task ApplyRetentionAsync(CancellationToken cancellationToken);
}
