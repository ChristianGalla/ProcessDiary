using System.Runtime.InteropServices;
using ProcessDiary.Domain.Abstractions;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ProcessDiary.Infrastructure.Platform;

/// <summary>Uses a popup window style and system rounded corners, so no frame or border shows and the dot edge is smooth.</summary>
internal sealed class IndicatorWindowStyler : IIndicatorWindowStyler
{
    private const double BaseDpi = 96.0;
    private const uint NoBorderColor = 0xFFFFFFFE;
    private const uint WmNcCalcSize = 0x0083;
    private const nuint SubclassId = 1;

    private readonly SUBCLASSPROC _subclassProc;
    private nint _subclassedWindow;

    /// <summary>Initializes the styler.</summary>
    public IndicatorWindowStyler()
    {
        _subclassProc = OnWindowMessage;    }

    /// <inheritdoc />
    public unsafe bool ApplyDotStyle(nint windowHandle, int diameterDips, int marginDips)
    {
        var window = new HWND(windowHandle);

        var monitor = PInvoke.MonitorFromPoint(default, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);
        var info = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
        if (!PInvoke.GetMonitorInfo(monitor, ref info))
        {
            return false;
        }

        var borderColor = NoBorderColor;
        PInvoke.DwmSetWindowAttribute(window, DWMWINDOWATTRIBUTE.DWMWA_BORDER_COLOR, &borderColor, sizeof(uint));

        // The system rounds the corners with anti-aliasing. A square window of twice the corner radius becomes a circle.
        var corners = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
        PInvoke.DwmSetWindowAttribute(window, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, &corners, sizeof(DWM_WINDOW_CORNER_PREFERENCE));

        var visible = PInvoke.GetWindowLong(window, WINDOW_LONG_PTR_INDEX.GWL_STYLE) & (int)WINDOW_STYLE.WS_VISIBLE;
        PInvoke.SetWindowLong(window, WINDOW_LONG_PTR_INDEX.GWL_STYLE, visible | unchecked((int)WINDOW_STYLE.WS_POPUP));

        // WinUI keeps an invisible border around the client area. Removing it lets the XAML content cover the whole window.
        if (_subclassedWindow != windowHandle)
        {
            PInvoke.SetWindowSubclass(window, _subclassProc, SubclassId, 0);
            _subclassedWindow = windowHandle;
        }

        // Move onto the primary monitor first so that the DPI used for the size belongs to that monitor.
        const SET_WINDOW_POS_FLAGS flags = SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE;
        PInvoke.SetWindowPos(window, HWND.Null, info.rcWork.left, info.rcWork.top, 0, 0, flags | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);

        var scale = PInvoke.GetDpiForWindow(window) / BaseDpi;
        var diameter = (int)Math.Ceiling(diameterDips * scale);
        var margin = (int)Math.Ceiling(marginDips * scale);

        var placed = PInvoke.SetWindowPos(
            window,
            HWND.Null,
            info.rcWork.right - diameter - margin,
            info.rcWork.top + margin,
            diameter,
            diameter,
            flags | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED);

        return placed;
    }

    private LRESULT OnWindowMessage(HWND window, uint message, WPARAM wParam, LPARAM lParam, nuint subclassId, nuint referenceData)
    {
        // Returning 0 for WM_NCCALCSIZE makes the client area cover the whole window.
        if (message == WmNcCalcSize && wParam.Value != 0)
        {
            return default;
        }

        return PInvoke.DefSubclassProc(window, message, wParam, lParam);
    }
    }
