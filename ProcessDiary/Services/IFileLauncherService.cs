namespace ProcessDiary.Services;

/// <summary>Opens files with the system's default application or reveals them in the file explorer.</summary>
public interface IFileLauncherService
{
    /// <summary>Opens a file with its default application.</summary>
    /// <param name="filePath">Full path of the file.</param>
    /// <returns>True when the file could be opened.</returns>
    bool OpenFile(string filePath);

    /// <summary>Shows a file in the file explorer.</summary>
    /// <param name="filePath">Full path of the file.</param>
    /// <returns>True when the location could be opened.</returns>
    bool ShowInFolder(string filePath);
}
