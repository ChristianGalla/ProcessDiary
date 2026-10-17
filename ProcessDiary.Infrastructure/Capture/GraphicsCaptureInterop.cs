using System.Runtime.InteropServices;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using WinRT;

namespace ProcessDiary.Infrastructure.Capture;

/// <summary>Win32/COM interop needed to use Windows Graphics Capture from a desktop app.</summary>
internal static class GraphicsCaptureInterop
{
    private static readonly Guid GraphicsCaptureItemIid = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");
    private static readonly Guid InteropFactoryIid = new("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356");
    private static readonly Guid DxgiInterfaceAccessIid = new("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1");

    /// <summary>Creates a capture item for a window.</summary>
    /// <param name="window">The window handle.</param>
    /// <returns>The capture item.</returns>
    public static GraphicsCaptureItem CreateItemForWindow(IntPtr window) =>
        CreateItem(interop =>
        {
            var iid = GraphicsCaptureItemIid;
            return interop.CreateForWindow(window, ref iid);
        });

    /// <summary>Creates a capture item for a monitor.</summary>
    /// <param name="monitor">The monitor handle.</param>
    /// <returns>The capture item.</returns>
    public static GraphicsCaptureItem CreateItemForMonitor(IntPtr monitor) =>
        CreateItem(interop =>
        {
            var iid = GraphicsCaptureItemIid;
            return interop.CreateForMonitor(monitor, ref iid);
        });

    /// <summary>Wraps a DXGI device in a WinRT Direct3D device.</summary>
    /// <param name="dxgiDevice">Native pointer of the <c>IDXGIDevice</c>.</param>
    /// <returns>The WinRT device.</returns>
    public static IDirect3DDevice CreateDirect3DDevice(IntPtr dxgiDevice)
    {
        Marshal.ThrowExceptionForHR(CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice, out var inspectable));
        try
        {
            return MarshalInterface<IDirect3DDevice>.FromAbi(inspectable);
        }
        finally
        {
            Marshal.Release(inspectable);
        }
    }

    /// <summary>Gets the native texture behind a captured surface.</summary>
    /// <param name="surface">The captured surface.</param>
    /// <param name="textureIid">Interface identifier of the requested texture interface.</param>
    /// <returns>Native pointer; the caller owns one reference.</returns>
    public static IntPtr GetNativeTexture(IDirect3DSurface surface, Guid textureIid)
    {
        var unknown = MarshalInterface<IDirect3DSurface>.FromManaged(surface);
        try
        {
            Marshal.ThrowExceptionForHR(Marshal.QueryInterface(unknown, in DxgiInterfaceAccessIid, out var access));
            try
            {
                var accessor = (IDirect3DDxgiInterfaceAccess)Marshal.GetObjectForIUnknown(access);
                return accessor.GetInterface(ref textureIid);
            }
            finally
            {
                Marshal.Release(access);
            }
        }
        finally
        {
            Marshal.Release(unknown);
        }
    }

    private static GraphicsCaptureItem CreateItem(Func<IGraphicsCaptureItemInterop, IntPtr> create)
    {
        var interop = GetInteropFactory();
        var itemPointer = create(interop);
        try
        {
            return MarshalInspectable<GraphicsCaptureItem>.FromAbi(itemPointer);
        }
        finally
        {
            Marshal.Release(itemPointer);
        }
    }

    private static IGraphicsCaptureItemInterop GetInteropFactory()
    {
        const string className = "Windows.Graphics.Capture.GraphicsCaptureItem";
        Marshal.ThrowExceptionForHR(WindowsCreateString(className, className.Length, out var hstring));
        try
        {
            var iid = InteropFactoryIid;
            Marshal.ThrowExceptionForHR(RoGetActivationFactory(hstring, ref iid, out var factory));
            try
            {
                return (IGraphicsCaptureItemInterop)Marshal.GetObjectForIUnknown(factory);
            }
            finally
            {
                Marshal.Release(factory);
            }
        }
        finally
        {
            _ = WindowsDeleteString(hstring);
        }
    }

    [DllImport("d3d11.dll", ExactSpelling = true)]
    private static extern int CreateDirect3D11DeviceFromDXGIDevice(IntPtr dxgiDevice, out IntPtr graphicsDevice);

    [DllImport("combase.dll", ExactSpelling = true)]
    private static extern int RoGetActivationFactory(IntPtr activatableClassId, ref Guid iid, out IntPtr factory);

    [DllImport("combase.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int WindowsCreateString(string sourceString, int length, out IntPtr hstring);

    [DllImport("combase.dll", ExactSpelling = true)]
    private static extern int WindowsDeleteString(IntPtr hstring);
}
