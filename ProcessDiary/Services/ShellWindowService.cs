using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace ProcessDiary.Services;

/// <summary>Holds the main window for services that need its handle or XAML root.</summary>
public sealed class ShellWindowService : IShellWindowService
{
    private Window? _window;

    /// <inheritdoc />
    public nint Handle => _window is null ? 0 : WindowNative.GetWindowHandle(_window);

    /// <inheritdoc />
    public XamlRoot? XamlRoot => _window?.Content?.XamlRoot;

    /// <inheritdoc />
    public void Attach(Window window) => _window = window;

    /// <inheritdoc />
    public void Show()
    {
        if (_window is null)
        {
            return;
        }

        _window.AppWindow.Show();
        _window.Activate();
    }

    /// <inheritdoc />
    public void Hide() => _window?.AppWindow.Hide();
}
