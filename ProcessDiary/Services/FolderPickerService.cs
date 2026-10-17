using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ProcessDiary.Services;

/// <summary>Shows the system folder picker attached to the main window.</summary>
public sealed class FolderPickerService : IFolderPickerService
{
    private readonly IShellWindowService _shell;

    /// <summary>Initializes the service.</summary>
    /// <param name="shell">Provides the window handle the picker is attached to.</param>
    public FolderPickerService(IShellWindowService shell) => _shell = shell;

    /// <inheritdoc />
    public async Task<string?> PickFolderAsync()
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        InitializeWithWindow.Initialize(picker, _shell.Handle);
        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }
}
