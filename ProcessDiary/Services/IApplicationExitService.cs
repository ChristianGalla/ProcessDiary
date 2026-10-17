namespace ProcessDiary.Services;

/// <summary>Stops recording and closes the application.</summary>
public interface IApplicationExitService
{
    /// <summary>Stops the host so open sessions are closed, removes the tray icon and exits.</summary>
    /// <returns>A task that completes when the application is shutting down.</returns>
    Task ExitAsync();
}
