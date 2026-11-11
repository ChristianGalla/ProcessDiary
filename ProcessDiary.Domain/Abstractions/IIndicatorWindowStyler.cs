namespace ProcessDiary.Domain.Abstractions;

/// <summary>Turns a window into a small borderless dot in the corner of the primary monitor.</summary>
public interface IIndicatorWindowStyler
{
    /// <summary>Removes the window frame, shapes the window as a circle and places it in the top-right corner.</summary>
    /// <param name="windowHandle">Native handle of the window.</param>
    /// <param name="diameterDips">Diameter of the dot in device independent pixels.</param>
    /// <param name="marginDips">Distance to the monitor work area edges in device independent pixels.</param>
    /// <returns>True when the window was shaped and placed.</returns>
    bool ApplyDotStyle(nint windowHandle, int diameterDips, int marginDips);

    /// <summary>Moves the window back above all other topmost windows, such as full screen applications, without activating it.</summary>
    /// <param name="windowHandle">Native handle of the window.</param>
    /// <returns>True when the window order was updated.</returns>
    bool BringToTop(nint windowHandle);
}
