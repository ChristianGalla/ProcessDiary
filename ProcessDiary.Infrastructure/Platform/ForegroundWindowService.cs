using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ProcessDiary.Domain.Abstractions;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace ProcessDiary.Infrastructure.Platform;

/// <summary>Reads the foreground window using Win32 calls.</summary>
internal sealed class ForegroundWindowService : IForegroundWindowService
{
    private readonly ILogger<ForegroundWindowService> _logger;

    /// <summary>Initializes the service.</summary>
    /// <param name="logger">The logger.</param>
    public ForegroundWindowService(ILogger<ForegroundWindowService> logger) => _logger = logger;

    /// <inheritdoc />
    public ForegroundWindowInfo? GetCurrent()
    {
        var window = PInvoke.GetForegroundWindow();
        if (window == HWND.Null)
        {
            return null;
        }

        uint processId;
        unsafe
        {
            _ = PInvoke.GetWindowThreadProcessId(window, &processId);
        }

        if (processId == 0)
        {
            return null;
        }

        try
        {
            using var process = Process.GetProcessById((int)processId);
            var name = process.ProcessName + ".exe";
            return new ForegroundWindowInfo(name, TryGetExecutablePath(process), GetTitle(window), (int)processId);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            // The process ended between the two calls; treat it as "unknown".
            _logger.LogDebug(ex, "Foreground process {ProcessId} is no longer available", processId);
            return null;
        }
    }

    private string TryGetExecutablePath(Process process)
    {
        try
        {
            return process.MainModule?.FileName ?? string.Empty;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or NotSupportedException)
        {
            // Elevated or protected processes refuse access to their module list.
            _logger.LogDebug("Executable path of process {ProcessId} is not accessible", process.Id);
            return string.Empty;
        }
    }

    private static string GetTitle(HWND window)
    {
        var length = PInvoke.GetWindowTextLength(window);
        if (length <= 0)
        {
            return string.Empty;
        }

        Span<char> buffer = new char[length + 1];
        var copied = PInvoke.GetWindowText(window, buffer);
        return copied > 0 ? new string(buffer[..copied]) : string.Empty;
    }
}
