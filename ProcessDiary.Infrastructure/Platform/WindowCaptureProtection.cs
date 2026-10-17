using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using ProcessDiary.Domain.Abstractions;

namespace ProcessDiary.Infrastructure.Platform;

/// <summary>Uses the window display affinity to hide windows from screen capture.</summary>
internal sealed class WindowCaptureProtection : IWindowCaptureProtection
{
    /// <inheritdoc />
    public bool ExcludeFromCapture(nint windowHandle) =>
        PInvoke.SetWindowDisplayAffinity(new HWND(windowHandle), WINDOW_DISPLAY_AFFINITY.WDA_EXCLUDEFROMCAPTURE);
}
