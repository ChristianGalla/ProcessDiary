using Microsoft.EntityFrameworkCore;
using ProcessDiary.Infrastructure.Persistence;

namespace ProcessDiary.Tests.Infrastructure;

/// <summary>Creates contexts for a temporary SQLite database used by integration tests.</summary>
internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "ProcessDiaryTests", Guid.NewGuid().ToString("N"));

    /// <summary>Initializes and creates the database.</summary>
    public TestDatabase()
    {
        Directory.CreateDirectory(_folder);
        var options = new DbContextOptionsBuilder<DiaryDbContext>()
            .UseSqlite($"Data Source={Path.Combine(_folder, "test.db")};Pooling=False")
            .Options;
        Factory = new ContextFactory(options);
        using var context = Factory.CreateDbContext();
        context.Database.EnsureCreated();
    }

    /// <summary>Gets the context factory.</summary>
    public IDbContextFactory<DiaryDbContext> Factory { get; }

    /// <summary>Gets the folder that contains the database.</summary>
    public string Folder => _folder;

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
            // Temp files are cleaned up by the OS eventually.
        }

        return ValueTask.CompletedTask;
    }

    private sealed class ContextFactory(DbContextOptions<DiaryDbContext> options) : IDbContextFactory<DiaryDbContext>
    {
        public DiaryDbContext CreateDbContext() => new(options);
    }
}
