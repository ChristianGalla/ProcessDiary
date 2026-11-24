namespace ProcessDiary.Services;

/// <summary>Shows dialogs to the user.</summary>
public interface IDialogService
{
    /// <summary>Shows a confirmation dialog with a confirm and a cancel button.</summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The explanation of what will happen.</param>
    /// <param name="confirmText">The text of the confirm button.</param>
    /// <returns>True when the user confirmed.</returns>
    Task<bool> ConfirmAsync(string title, string message, string confirmText);

    /// <summary>Shows an error dialog.</summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The error message.</param>
    Task ShowErrorAsync(string title, string message);
}
