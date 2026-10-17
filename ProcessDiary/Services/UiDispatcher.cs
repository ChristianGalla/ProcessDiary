using Microsoft.UI.Dispatching;

namespace ProcessDiary.Services;

/// <summary>Dispatches work through a WinUI <see cref="DispatcherQueue"/>.</summary>
public sealed class UiDispatcher : IUiDispatcher
{
    private readonly DispatcherQueue _queue;

    /// <summary>Initializes the dispatcher.</summary>
    /// <param name="queue">The queue of the UI thread.</param>
    public UiDispatcher(DispatcherQueue queue) => _queue = queue;

    /// <inheritdoc />
    public void Post(Action action)
    {
        if (_queue.HasThreadAccess)
        {
            action();
            return;
        }

        _queue.TryEnqueue(() => action());
    }
}
