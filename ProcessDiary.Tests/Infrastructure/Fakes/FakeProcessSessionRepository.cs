using System.Collections.Concurrent;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Tests.Infrastructure.Fakes;

/// <summary>In-memory session repository; only the members used by the scheduler are implemented.</summary>
internal sealed class FakeProcessSessionRepository : IProcessSessionRepository
{
    private readonly ConcurrentDictionary<Guid, ProcessSession> _items = new();

    /// <summary>Gets a snapshot of the stored sessions.</summary>
    public IReadOnlyList<ProcessSession> Items => _items.Values.OrderBy(s => s.StartedAt).ToList();

    /// <inheritdoc />
    public Task AddAsync(ProcessSession session, CancellationToken cancellationToken)
    {
        _items[session.Id] = session;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateAsync(ProcessSession session, CancellationToken cancellationToken)
    {
        _items[session.Id] = session;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ProcessSession>> GetOverlappingAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    /// <inheritdoc />
    public Task<DateTimeOffset?> GetEarliestStartAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

    /// <inheritdoc />
    public Task DeleteAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken) => throw new NotSupportedException();

    /// <inheritdoc />
    public Task DeleteEndedBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken) => throw new NotSupportedException();

    /// <inheritdoc />
    public Task DeleteAllAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
}
