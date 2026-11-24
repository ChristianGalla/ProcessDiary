using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using ProcessDiary.Domain.Abstractions;
using System.ComponentModel;
using Windows.ApplicationModel;

namespace ProcessDiary.Infrastructure.Platform;

/// <summary>Registers ProcessDiary to start for the current Windows user.</summary>
public sealed class StartupRegistrationService : IStartupRegistrationService, IHostedService, INotifyPropertyChanged
{
    private const string StartupTaskId = "ProcessDiaryStartup";
    private const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RegistryValueName = "ProcessDiary";
    private bool? _isEnabled;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public bool? IsEnabled
    {
        get => _isEnabled;
        private set
        {
            if (_isEnabled == value)
            {
                return;
            }

            _isEnabled = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
        }
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IsEnabled = IsPackaged()
            ? await GetPackagedStartupStateAsync(cancellationToken)
            : GetUnpackagedStartupState();
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc />
    public async Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (IsPackaged())
        {
            await SetPackagedStartupAsync(enabled, cancellationToken);
        }
        else
        {
            SetUnpackagedStartup(enabled);
        }

        IsEnabled = enabled;
    }

    private static async Task<bool> GetPackagedStartupStateAsync(CancellationToken cancellationToken)
    {
        var task = await StartupTask.GetAsync(StartupTaskId);
        cancellationToken.ThrowIfCancellationRequested();
        return task.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
    }

    private static bool GetUnpackagedStartupState()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
        return key?.GetValue(RegistryValueName) is not null;
    }

    private static async Task SetPackagedStartupAsync(bool enabled, CancellationToken cancellationToken)
    {
        var task = await StartupTask.GetAsync(StartupTaskId);
        cancellationToken.ThrowIfCancellationRequested();

        if (!enabled)
        {
            task.Disable();
            return;
        }

        var state = await task.RequestEnableAsync();
        cancellationToken.ThrowIfCancellationRequested();
        if (state is not StartupTaskState.Enabled and not StartupTaskState.EnabledByPolicy)
        {
            throw new InvalidOperationException($"Automatic startup is unavailable because its state is {state}.");
        }
    }

    private static void SetUnpackagedStartup(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RegistryPath);
        if (!enabled)
        {
            key.DeleteValue(RegistryValueName, throwOnMissingValue: false);
            return;
        }

        var executablePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("The ProcessDiary executable path could not be determined.");
        key.SetValue(RegistryValueName, $"\"{executablePath}\"");
    }

    private static bool IsPackaged()
    {
        try
        {
            _ = Package.Current.Id;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
