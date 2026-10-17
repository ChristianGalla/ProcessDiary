using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Domain.Abstractions;

/// <summary>Persistence of process sessions.</summary>
public interface IProcessSessionRepository
{
    /// <summary>Stores a new session.</summary>
    /// <param name="session">The session to add.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the session is stored.</returns>
    Task AddAsync(ProcessSession session, CancellationToken cancellationToken);

    /// <summary>Updates an existing session, for example when it ends.</summary>
    /// <param name="session">The session with its new values.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the session is updated.</returns>
    Task UpdateAsync(ProcessSession session, CancellationToken cancellationToken);

    /// <summary>Gets sessions overlapping the range [from, to), including open sessions.</summary>
    /// <param name="from">Inclusive start of the range.</param>
    /// <param name="to">Exclusive end of the range.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The overlapping sessions.</returns>
    Task<IReadOnlyList<ProcessSession>> GetOverlappingAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    /// <summary>Gets the start of the earliest stored session.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The earliest start, or null when there are no sessions.</returns>
    Task<DateTimeOffset?> GetEarliestStartAsync(CancellationToken cancellationToken);

    /// <summary>Deletes the given sessions.</summary>
    /// <param name="sessionIds">Identifiers of the sessions to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the sessions are deleted.</returns>
    Task DeleteAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken);

    /// <summary>Deletes sessions that ended before the cutoff.</summary>
    /// <param name="cutoff">Sessions that ended before this time are deleted.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the sessions are deleted.</returns>
    Task DeleteEndedBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken);

    /// <summary>Deletes all sessions.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when all sessions are deleted.</returns>
    Task DeleteAllAsync(CancellationToken cancellationToken);
}
