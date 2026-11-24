using System.ComponentModel;

namespace ProcessDiary.Domain.Abstractions;

/// <summary>Controls whether ProcessDiary starts when the current user signs in.</summary>
public interface IStartupRegistrationService : INotifyPropertyChanged
{
    /// <summary>Gets whether the current user has automatic startup enabled, or null while the state is loading.</summary>
    bool? IsEnabled { get; }

    /// <summary>Enables or disables automatic startup for the current user.</summary>
    /// <param name="enabled">Whether automatic startup should be enabled.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the registration has been updated.</returns>
    Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken);
}
