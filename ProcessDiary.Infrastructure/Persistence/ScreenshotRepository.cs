using Microsoft.EntityFrameworkCore;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Infrastructure.Persistence;

/// <summary>SQLite implementation of <see cref="IScreenshotRepository"/>.</summary>
internal sealed class ScreenshotRepository : IScreenshotRepository
{
    private readonly IDbContextFactory<DiaryDbContext> _factory;

    /// <summary>Initializes the repository.</summary>
    /// <param name="factory">Factory that creates short-lived database contexts.</param>
    public ScreenshotRepository(IDbContextFactory<DiaryDbContext> factory) => _factory = factory;

    /// <inheritdoc />
    public async Task AddAsync(Screenshot screenshot, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        context.Screenshots.Add(new ScreenshotRow
        {
            Id = screenshot.Id,
            SessionId = screenshot.ProcessSessionId,
            FilePath = screenshot.FilePath,
            CapturedAtUtcTicks = TimestampConverter.ToTicks(screenshot.CapturedAt),
            Width = screenshot.Width,
            Height = screenshot.Height
        });
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Screenshot>> GetBySessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var rows = await context.Screenshots.AsNoTracking()
            .Where(s => s.SessionId == sessionId)
            .OrderBy(s => s.CapturedAtUtcTicks)
            .ToListAsync(cancellationToken);
        return rows.Select(ToEntity).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Screenshot>> GetCapturedBetweenAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var fromTicks = TimestampConverter.ToTicks(from);
        var toTicks = TimestampConverter.ToTicks(to);
        var rows = await context.Screenshots.AsNoTracking()
            .Where(s => s.CapturedAtUtcTicks >= fromTicks && s.CapturedAtUtcTicks < toTicks)
            .OrderBy(s => s.CapturedAtUtcTicks)
            .ToListAsync(cancellationToken);
        return rows.Select(ToEntity).ToList();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(IReadOnlyCollection<Guid> screenshotIds, CancellationToken cancellationToken)
    {
        if (screenshotIds.Count == 0)
        {
            return;
        }

        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        await context.Screenshots.Where(s => screenshotIds.Contains(s.Id)).ExecuteDeleteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Screenshot>> GetCapturedBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var cutoffTicks = TimestampConverter.ToTicks(cutoff);
        var rows = await context.Screenshots.AsNoTracking()
            .Where(s => s.CapturedAtUtcTicks < cutoffTicks)
            .OrderBy(s => s.CapturedAtUtcTicks)
            .ToListAsync(cancellationToken);
        return rows.Select(ToEntity).ToList();
    }

    /// <inheritdoc />
    public async Task DeleteAllAsync(CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        await context.Screenshots.ExecuteDeleteAsync(cancellationToken);
    }

    private static Screenshot ToEntity(ScreenshotRow row) => new(
        row.Id,
        row.SessionId,
        row.FilePath,
        TimestampConverter.FromTicks(row.CapturedAtUtcTicks),
        row.Width,
        row.Height);
}
