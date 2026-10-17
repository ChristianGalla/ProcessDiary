using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ProcessDiary.Views;

namespace ProcessDiary.Services;

/// <summary>Creates pages through the container and shows them in the main window.</summary>
public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private ContentControl? _host;

    /// <summary>Initializes the service.</summary>
    /// <param name="services">Container used to create pages with their view models.</param>
    public NavigationService(IServiceProvider services) => _services = services;

    /// <inheritdoc />
    public void Initialize(ContentControl host) => _host = host;

    /// <inheritdoc />
    public void Navigate(string key)
    {
        if (_host is null)
        {
            return;
        }

        Type? pageType = key switch
        {
            NavigationKeys.Timeline => typeof(TimelinePage),
            NavigationKeys.Statistics => typeof(StatisticsPage),
            NavigationKeys.Settings => typeof(SettingsPage),
            _ => null
        };

        if (pageType is not null)
        {
            _host.Content = _services.GetRequiredService(pageType);
        }
    }
}
