using System.Runtime.InteropServices;

namespace ProcessDiary.Infrastructure.Capture;

/// <summary>COM interface that creates <c>GraphicsCaptureItem</c> instances for windows and monitors.</summary>
[ComImport]
[Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGraphicsCaptureItemInterop
{
    /// <summary>Creates a capture item for a window.</summary>
    /// <param name="window">The window handle.</param>
    /// <param name="iid">Interface identifier of the requested item interface.</param>
    /// <returns>Pointer to the requested interface.</returns>
    IntPtr CreateForWindow(IntPtr window, ref Guid iid);

    /// <summary>Creates a capture item for a monitor.</summary>
    /// <param name="monitor">The monitor handle.</param>
    /// <param name="iid">Interface identifier of the requested item interface.</param>
    /// <returns>Pointer to the requested interface.</returns>
    IntPtr CreateForMonitor(IntPtr monitor, ref Guid iid);
}
