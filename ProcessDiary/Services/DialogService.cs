using Microsoft.UI.Xaml.Controls;

namespace ProcessDiary.Services;

/// <summary>Shows confirmation dialogs in the main window.</summary>
public sealed class DialogService : IDialogService
{
    private readonly IShellWindowService _shell;

    /// <summary>Initializes the service.</summary>
    /// <param name="shell">Provides the XAML root of the main window.</param>
    public DialogService(IShellWindowService shell) => _shell = shell;

    /// <inheritdoc />
    public async Task<bool> ConfirmAsync(string title, string message, string confirmText)
    {
        if (_shell.XamlRoot is not { } root)
        {
            return false;
        }

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = confirmText,
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = root
        };

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }
}
