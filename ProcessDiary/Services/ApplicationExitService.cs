using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

namespace ProcessDiary.Services;

/// <summary>Shuts the host down cleanly before the application exits.</summary>
public sealed class ApplicationExitService : IApplicationExitService
{
    private readonly IServiceProvider _services;

    /// <summary>Initializes the service.</summary>
    /// <param name="services">Container used to resolve the host and the tray icon on demand.</param>
    public ApplicationExitService(IServiceProvider services) => _services = services;

    /// <inheritdoc />
    public async Task ExitAsync()
    {
        _services.GetRequiredService<TrayIconService>().Dispose();
        await _services.GetRequiredService<IHost>().StopAsync();
        Microsoft.UI.Xaml.Application.Current.Exit();
    }
}
