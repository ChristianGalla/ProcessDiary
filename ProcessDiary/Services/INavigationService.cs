using Microsoft.UI.Xaml.Controls;

namespace ProcessDiary.Services;

/// <summary>Switches the page shown in the main window.</summary>
public interface INavigationService
{
    /// <summary>Registers the control that hosts the pages.</summary>
    /// <param name="host">The content host.</param>
    void Initialize(ContentControl host);

    /// <summary>Shows a page.</summary>
    /// <param name="key">One of <see cref="NavigationKeys"/>.</param>
    void Navigate(string key);
}
