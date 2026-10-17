namespace ProcessDiary.Services;

/// <summary>Runs work on the UI thread.</summary>
public interface IUiDispatcher
{
    /// <summary>Queues an action on the UI thread; runs it directly when already on that thread.</summary>
    /// <param name="action">The action to run.</param>
    void Post(Action action);
}
