using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Infrastructure.Capture;
using ProcessDiary.Infrastructure.Persistence;
using ProcessDiary.Infrastructure.Platform;
using ProcessDiary.Infrastructure.Scheduling;
using ProcessDiary.Infrastructure.Settings;
using ProcessDiary.Infrastructure.Storage;

namespace ProcessDiary.Infrastructure;

/// <summary>Registers the infrastructure services.</summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>Registers persistence, Win32 services, capture, storage and the capture scheduler.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="databaseOptions">Database location; defaults to the local application data folder.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddProcessDiaryInfrastructure(
        this IServiceCollection services, DiaryDatabaseOptions? databaseOptions = null)
    {
        var options = databaseOptions ?? DiaryDatabaseOptions.Default;
        services.AddSingleton(options);
        services.AddDbContextFactory<DiaryDbContext>(o => o.UseSqlite($"Data Source={options.DatabasePath}"));

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<DiarySettingsProvider>();
        services.AddSingleton<IDiarySettingsProvider>(sp => sp.GetRequiredService<DiarySettingsProvider>());
        services.AddSingleton<PersistentSettingsStore>();
        services.AddSingleton<IDiarySettingsStore>(sp => sp.GetRequiredService<PersistentSettingsStore>());
        services.AddSingleton<IWindowCaptureProtection, WindowCaptureProtection>();
        services.AddSingleton<IIndicatorWindowStyler, IndicatorWindowStyler>();

        services.AddSingleton<IProcessSessionRepository, ProcessSessionRepository>();
        services.AddSingleton<IScreenshotRepository, ScreenshotRepository>();
        services.AddSingleton<IApplicationMetadataRepository, ApplicationMetadataRepository>();

        services.AddSingleton<IForegroundWindowService, ForegroundWindowService>();
        services.AddSingleton<IScreenshotService, ScreenshotService>();
        services.AddSingleton<IScreenshotFileStore, ScreenshotFileStore>();

        // Hosted services start in registration order: the database must exist before recording starts.
        services.AddHostedService(sp => sp.GetRequiredService<PersistentSettingsStore>());
        services.AddHostedService<DiaryDatabaseInitializer>();
        services.AddSingleton<CaptureScheduler>();
        services.AddSingleton<ICaptureScheduler>(sp => sp.GetRequiredService<CaptureScheduler>());
        services.AddHostedService(sp => sp.GetRequiredService<CaptureScheduler>());

        return services;
    }
}
