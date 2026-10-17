using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using ProcessDiary.Application;
using ProcessDiary.Infrastructure;
using ProcessDiary.Infrastructure.Logging;
using ProcessDiary.Services;
using ProcessDiary.ViewModels;
using ProcessDiary.Views;

namespace ProcessDiary;

/// <summary>Application entry point and composition root.</summary>
public partial class App : Microsoft.UI.Xaml.Application
{
    private IHost? _host;
    private MainWindow? _window;
    private RecordingOverlayWindow? _overlay;

    /// <summary>Initializes the application.</summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>Starts the host, then shows the main window, the tray icon and the status overlay.</summary>
    /// <param name="args">Details about the launch request.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var host = BuildHost(DispatcherQueue.GetForCurrentThread());
        _host = host;
        await host.StartAsync();

        _window = host.Services.GetRequiredService<MainWindow>();
        _window.Activate();

        host.Services.GetRequiredService<TrayIconService>().Initialize();

        // Recording is never hidden: the overlay shows the state from the first moment.
        _overlay = host.Services.GetRequiredService<RecordingOverlayWindow>();
        _overlay.ShowWithoutActivation();

        host.Services.GetRequiredService<ILogger<App>>().LogInformation("ProcessDiary started");
    }

    private static IHost BuildHost(DispatcherQueue queue)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.AddProcessDiaryLogging();

        var services = builder.Services;
        services.AddProcessDiaryInfrastructure();
        services.AddProcessDiaryApplication();

        services.AddSingleton<IUiDispatcher>(new UiDispatcher(queue));
        services.AddSingleton<IShellWindowService, ShellWindowService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IFolderPickerService, FolderPickerService>();
        services.AddSingleton<IApplicationExitService, ApplicationExitService>();
        services.AddSingleton<TrayIconService>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<TimelineViewModel>();
        services.AddSingleton<StatisticsViewModel>();
        services.AddSingleton<SettingsViewModel>();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<RecordingOverlayWindow>();
        services.AddTransient<TimelinePage>();
        services.AddTransient<StatisticsPage>();
        services.AddTransient<SettingsPage>();

        return builder.Build();
    }
}
