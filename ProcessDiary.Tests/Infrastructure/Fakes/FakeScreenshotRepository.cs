using System.Collections.Concurrent;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Tests.Infrastructure.Fakes;

/// <summary>In-memory screenshot repository; only the members used by the scheduler are implemented.</summary>
internal sealed class FakeScreenshotRepository : IScreenshotRepository
{
    /// <summary>Gets the stored screenshots.</summary>
    public ConcurrentBag<Screenshot> Items { get; } = [];

    /// <inheritdoc />
    public Task AddAsync(Screenshot screenshot, CancellationToken cancellationToken)
    {
        Items.Add(screenshot);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Screenshot>> GetBySessionAsync(Guid sessionId, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    /// <inheritdoc />
    public Task<IReadOnlyList<Screenshot>> GetCapturedBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    /// <inheritdoc />
    public Task DeleteAsync(IReadOnlyCollection<Guid> screenshotIds, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    /// <inheritdoc />
    public Task<IReadOnlyList<Screenshot>> GetCapturedBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    /// <inheritdoc />
    public Task DeleteAllAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
}
