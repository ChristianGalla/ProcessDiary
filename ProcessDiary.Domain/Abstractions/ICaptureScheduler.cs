namespace ProcessDiary.Domain.Abstractions;

/// <summary>Controls recording of sessions and screenshots.</summary>
public interface ICaptureScheduler
{
    /// <summary>Gets the current recording state.</summary>
    RecordingState State { get; }

    /// <summary>Gets when the user last started recording in this run of the application, or null when recording was never started.</summary>
    DateTimeOffset? RecordingStartedAt { get; }

    /// <summary>Occurs when <see cref="State"/> changes.</summary>
    event EventHandler<RecordingState>? StateChanged;

    /// <summary>Starts recording. Recording never starts on its own; the user must start it each time the app launches.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when recording is started.</returns>
    Task StartRecordingAsync(CancellationToken cancellationToken);

    /// <summary>Pauses recording and ends the open session.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when recording is paused.</returns>
    Task PauseAsync(CancellationToken cancellationToken);

    /// <summary>Resumes recording after a pause.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when recording is resumed.</returns>
    Task ResumeAsync(CancellationToken cancellationToken);

    /// <summary>Takes a screenshot on request of the user.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The stored screenshot, or null when nothing could be captured.</returns>
    Task<Entities.Screenshot?> CaptureNowAsync(CancellationToken cancellationToken);
}
