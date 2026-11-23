namespace ProcessDiary.Domain.Abstractions;

/// <summary>Turns a window into a small borderless dot in a corner of the primary monitor.</summary>
public interface IIndicatorWindowStyler
{
    /// <summary>Removes the window frame, shapes the window as a circle and places it in the selected corner.</summary>
    /// <param name="windowHandle">Native handle of the window.</param>
    /// <param name="diameterDips">Diameter of the dot in device-independent pixels.</param>
    /// <param name="corner">Corner of the monitor work area in which to place the dot.</param>
    /// <param name="horizontalMarginDips">Horizontal distance from the nearest monitor border in device-independent pixels.</param>
    /// <param name="verticalMarginDips">Vertical distance from the nearest monitor border in device-independent pixels.</param>
    /// <returns>True when the window was shaped and placed.</returns>
    bool ApplyDotStyle(
        nint windowHandle,
        int diameterDips,
        OverlayCorner corner,
        double horizontalMarginDips,
        double verticalMarginDips);

    /// <summary>Moves the window back above all other topmost windows, such as full screen applications, without activating it.</summary>
    /// <param name="windowHandle">Native handle of the window.</param>
    /// <returns>True when the window order was updated.</returns>
    bool BringToTop(nint windowHandle);
}
