using Microsoft.Extensions.DependencyInjection;
using ProcessDiary.Application.Privacy;
using ProcessDiary.Application.Reports;
using ProcessDiary.Application.Timeline;

namespace ProcessDiary.Application;

/// <summary>Registers the application services.</summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>Registers reports, timeline, data management and the retention job.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddProcessDiaryApplication(this IServiceCollection services)
    {
        services.AddSingleton<IReportContextProvider, ReportContextProvider>();
        services.AddSingleton<IReportService, ReportService>();
        services.AddSingleton<ITimelineService, TimelineService>();
        services.AddSingleton<IDataManagementService, DataManagementService>();
        services.AddHostedService<RetentionBackgroundService>();
        return services;
    }
}
