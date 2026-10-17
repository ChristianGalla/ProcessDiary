using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProcessDiary.Infrastructure.Persistence;

/// <summary>Creates the diary database when it does not exist yet; runs before the recording starts.</summary>
internal sealed class DiaryDatabaseInitializer : IHostedService
{
    private readonly IDbContextFactory<DiaryDbContext> _factory;
    private readonly DiaryDatabaseOptions _options;
    private readonly ILogger<DiaryDatabaseInitializer> _logger;

    /// <summary>Initializes the initializer.</summary>
    /// <param name="factory">Factory that creates database contexts.</param>
    /// <param name="options">Database location.</param>
    /// <param name="logger">The logger.</param>
    public DiaryDatabaseInitializer(
        IDbContextFactory<DiaryDbContext> factory,
        DiaryDatabaseOptions options,
        ILogger<DiaryDatabaseInitializer> logger)
    {
        _factory = factory;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken) => InitializeAsync(cancellationToken);

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>Ensures the database folder and schema exist.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the database is ready.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var folder = Path.GetDirectoryName(_options.DatabasePath);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);
            _logger.LogInformation("Diary database ready at {DatabasePath}", _options.DatabasePath);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Diary database could not be initialized");
            throw;
        }
    }
}
