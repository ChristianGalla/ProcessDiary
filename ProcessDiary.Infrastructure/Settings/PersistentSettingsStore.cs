using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Infrastructure.Persistence;

namespace ProcessDiary.Infrastructure.Settings;

/// <summary>
/// Keeps the user's settings in a JSON file next to the database. Settings are loaded when the host starts
/// and written whenever the user saves them.
/// </summary>
internal sealed class PersistentSettingsStore : IDiarySettingsStore, IHostedService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly DiarySettingsProvider _provider;
    private readonly string _filePath;
    private readonly ILogger<PersistentSettingsStore> _logger;

    /// <summary>Initializes the store.</summary>
    /// <param name="provider">Holds the current settings in memory.</param>
    /// <param name="options">Database location; the settings file is stored in the same folder.</param>
    /// <param name="logger">The logger.</param>
    public PersistentSettingsStore(
        DiarySettingsProvider provider, DiaryDatabaseOptions options, ILogger<PersistentSettingsStore> logger)
    {
        _provider = provider;
        _logger = logger;
        var folder = Path.GetDirectoryName(options.DatabasePath) ?? string.Empty;
        _filePath = Path.Combine(folder, "settings.json");
    }

    /// <inheritdoc />
    public event EventHandler? Changed;

    /// <inheritdoc />
    public DiarySettings Current => _provider.Current;

    /// <inheritdoc />
    public async Task SaveAsync(DiarySettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var folder = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Settings could not be saved");
            throw;
        }

        _provider.Update(settings);
        _logger.LogInformation("Settings saved");
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return;
        }

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var loaded = await JsonSerializer.DeserializeAsync<DiarySettings>(stream, JsonOptions, cancellationToken);
            if (loaded is not null && IsValid(loaded))
            {
                _provider.Update(loaded);
                _logger.LogInformation("Settings loaded");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            // Unreadable settings must not prevent the app from starting; defaults stay in effect.
            _logger.LogWarning(ex, "Settings could not be loaded; defaults are used");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static bool IsValid(DiarySettings settings) =>
        !string.IsNullOrWhiteSpace(settings.ScreenshotFolder)
        && settings.CaptureInterval > TimeSpan.Zero
        && settings.ForegroundPollInterval > TimeSpan.Zero
        && (settings.RetentionDays is null || settings.RetentionDays > 0);
}
