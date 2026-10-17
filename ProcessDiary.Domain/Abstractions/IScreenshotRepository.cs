using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Domain.Abstractions;

/// <summary>Persistence of screenshot records.</summary>
public interface IScreenshotRepository
{
    /// <summary>Stores a new screenshot record.</summary>
    /// <param name="screenshot">The screenshot to add.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the record is stored.</returns>
    Task AddAsync(Screenshot screenshot, CancellationToken cancellationToken);

    /// <summary>Gets the screenshots of a session.</summary>
    /// <param name="sessionId">Identifier of the session.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The screenshots belonging to the session.</returns>
    Task<IReadOnlyList<Screenshot>> GetBySessionAsync(Guid sessionId, CancellationToken cancellationToken);

    /// <summary>Gets screenshots captured within a time range.</summary>
    /// <param name="from">Inclusive start of the range.</param>
    /// <param name="to">Exclusive end of the range.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The screenshots captured in the range.</returns>
    Task<IReadOnlyList<Screenshot>> GetCapturedBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    /// <summary>Deletes the given screenshot records.</summary>
    /// <param name="screenshotIds">Identifiers of the screenshots to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the records are deleted.</returns>
    Task DeleteAsync(IReadOnlyCollection<Guid> screenshotIds, CancellationToken cancellationToken);

    /// <summary>Gets screenshots captured before the cutoff, for example to apply retention.</summary>
    /// <param name="cutoff">Screenshots captured before this time are returned.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The older screenshots.</returns>
    Task<IReadOnlyList<Screenshot>> GetCapturedBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken);

    /// <summary>Deletes all screenshot records.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when all records are deleted.</returns>
    Task DeleteAllAsync(CancellationToken cancellationToken);
}
