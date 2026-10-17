using Microsoft.UI.Xaml;

namespace ProcessDiary.Services;

/// <summary>Gives services access to the main window without depending on its type.</summary>
public interface IShellWindowService
{
    /// <summary>Gets the native handle of the main window, or zero when it does not exist yet.</summary>
    nint Handle { get; }

    /// <summary>Gets the root used to show dialogs, or null when the window has no content yet.</summary>
    XamlRoot? XamlRoot { get; }

    /// <summary>Registers the main window.</summary>
    /// <param name="window">The main window.</param>
    void Attach(Window window);

    /// <summary>Shows the main window and brings it to the front.</summary>
    void Show();

    /// <summary>Hides the main window; recording continues and the tray icon stays visible.</summary>
    void Hide();
}
