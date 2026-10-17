namespace ProcessDiary.Services;

/// <summary>Lets the user choose a folder.</summary>
public interface IFolderPickerService
{
    /// <summary>Shows the folder picker.</summary>
    /// <returns>The chosen folder, or null when the user cancelled.</returns>
    Task<string?> PickFolderAsync();
}
