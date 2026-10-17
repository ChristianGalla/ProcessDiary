using Microsoft.EntityFrameworkCore;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Infrastructure.Persistence;

/// <summary>SQLite implementation of <see cref="IApplicationMetadataRepository"/>.</summary>
internal sealed class ApplicationMetadataRepository : IApplicationMetadataRepository
{
    private readonly IDbContextFactory<DiaryDbContext> _factory;

    /// <summary>Initializes the repository.</summary>
    /// <param name="factory">Factory that creates short-lived database contexts.</param>
    public ApplicationMetadataRepository(IDbContextFactory<DiaryDbContext> factory) => _factory = factory;

    /// <inheritdoc />
    public async Task<IReadOnlyList<ApplicationMetadata>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var rows = await context.Applications.AsNoTracking()
            .OrderBy(a => a.ProcessName)
            .ToListAsync(cancellationToken);
        return rows.Select(a => new ApplicationMetadata(a.Id, a.ProcessName, a.DisplayName ?? a.ProcessName, a.Category)).ToList();
    }

    /// <inheritdoc />
    public async Task UpsertAsync(ApplicationMetadata metadata, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var row = await context.Applications
            .FirstOrDefaultAsync(a => a.ProcessName == metadata.ProcessName, cancellationToken);

        if (row is null)
        {
            context.Applications.Add(new ApplicationRow
            {
                Id = metadata.Id,
                ProcessName = metadata.ProcessName,
                DisplayName = metadata.DisplayName,
                Category = metadata.Category
            });
        }
        else
        {
            row.DisplayName = metadata.DisplayName;
            row.Category = metadata.Category;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
