using System.Runtime.InteropServices;

namespace ProcessDiary.Infrastructure.Capture;

/// <summary>COM interface that exposes the native Direct3D object behind a WinRT Direct3D surface.</summary>
[ComImport]
[Guid("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDirect3DDxgiInterfaceAccess
{
    /// <summary>Gets the underlying native interface.</summary>
    /// <param name="iid">Interface identifier to request.</param>
    /// <returns>Pointer to the requested interface.</returns>
    IntPtr GetInterface(ref Guid iid);
}
