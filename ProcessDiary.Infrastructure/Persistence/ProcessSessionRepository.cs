using Microsoft.EntityFrameworkCore;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Infrastructure.Persistence;

/// <summary>SQLite implementation of <see cref="IProcessSessionRepository"/>.</summary>
internal sealed class ProcessSessionRepository : IProcessSessionRepository
{
    private readonly IDbContextFactory<DiaryDbContext> _factory;

    /// <summary>Initializes the repository.</summary>
    /// <param name="factory">Factory that creates short-lived database contexts.</param>
    public ProcessSessionRepository(IDbContextFactory<DiaryDbContext> factory) => _factory = factory;

    /// <inheritdoc />
    public async Task AddAsync(ProcessSession session, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);

        var application = await context.Applications
            .FirstOrDefaultAsync(a => a.ProcessName == session.ProcessName, cancellationToken);
        if (application is null)
        {
            application = new ApplicationRow { Id = Guid.NewGuid(), ProcessName = session.ProcessName };
            context.Applications.Add(application);
        }

        context.Sessions.Add(new SessionRow
        {
            Id = session.Id,
            ApplicationId = application.Id,
            ExecutablePath = session.ExecutablePath,
            WindowTitle = session.WindowTitle,
            StartedAtUtcTicks = TimestampConverter.ToTicks(session.StartedAt),
            EndedAtUtcTicks = TimestampConverter.ToTicks(session.EndedAt)
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(ProcessSession session, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var endedTicks = TimestampConverter.ToTicks(session.EndedAt);

        await context.Sessions
            .Where(s => s.Id == session.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.WindowTitle, session.WindowTitle)
                .SetProperty(s => s.ExecutablePath, session.ExecutablePath)
                .SetProperty(s => s.EndedAtUtcTicks, endedTicks),
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProcessSession>> GetOverlappingAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var fromTicks = TimestampConverter.ToTicks(from);
        var toTicks = TimestampConverter.ToTicks(to);

        var rows = await context.Sessions
            .AsNoTracking()
            .Include(s => s.Application)
            .Where(s => s.StartedAtUtcTicks < toTicks && (s.EndedAtUtcTicks == null || s.EndedAtUtcTicks > fromTicks))
            .OrderBy(s => s.StartedAtUtcTicks)
            .ToListAsync(cancellationToken);

        return rows.Select(ToEntity).ToList();
    }

    /// <inheritdoc />
    public async Task<DateTimeOffset?> GetEarliestStartAsync(CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var ticks = await context.Sessions.MinAsync(s => (long?)s.StartedAtUtcTicks, cancellationToken);
        return TimestampConverter.FromTicks(ticks);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken)
    {
        if (sessionIds.Count == 0)
        {
            return;
        }

        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        await context.Sessions.Where(s => sessionIds.Contains(s.Id)).ExecuteDeleteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteEndedBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var cutoffTicks = TimestampConverter.ToTicks(cutoff);
        await context.Sessions
            .Where(s => s.EndedAtUtcTicks != null && s.EndedAtUtcTicks < cutoffTicks)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAllAsync(CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        await context.Sessions.ExecuteDeleteAsync(cancellationToken);
    }

    private static ProcessSession ToEntity(SessionRow row) => new(
        row.Id,
        row.Application.ProcessName,
        row.ExecutablePath,
        row.WindowTitle,
        TimestampConverter.FromTicks(row.StartedAtUtcTicks),
        TimestampConverter.FromTicks(row.EndedAtUtcTicks));
}
