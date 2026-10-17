using Microsoft.Extensions.Logging;
using ProcessDiary.Domain.Abstractions;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace ProcessDiary.Infrastructure.Capture;

/// <summary>Captures screenshots with Windows Graphics Capture and encodes them as PNG.</summary>
internal sealed class ScreenshotService : IScreenshotService, IDisposable
{
    private static readonly TimeSpan FrameTimeout = TimeSpan.FromSeconds(5);

    private readonly IDiarySettingsProvider _settings;
    private readonly ILogger<ScreenshotService> _logger;
    private readonly SemaphoreSlim _captureLock = new(1, 1);
    private ID3D11Device? _device;
    private ID3D11DeviceContext? _context;
    private IDirect3DDevice? _winRtDevice;

    /// <summary>Initializes the service.</summary>
    /// <param name="settings">Provides the capture mode.</param>
    /// <param name="logger">The logger.</param>
    public ScreenshotService(IDiarySettingsProvider settings, ILogger<ScreenshotService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ScreenshotResult> CaptureAsync(CancellationToken cancellationToken)
    {
        await _captureLock.WaitAsync(cancellationToken);
        try
        {
            EnsureDevice();
            var item = CreateCaptureItem(_settings.Current.CaptureMode);
            var capturedAt = DateTimeOffset.Now;
            return await CaptureItemAsync(item, capturedAt, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Screenshot capture failed");
            throw;
        }
        finally
        {
            _captureLock.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _context?.Dispose();
        _device?.Dispose();
        _captureLock.Dispose();
    }

    private GraphicsCaptureItem CreateCaptureItem(CaptureMode mode)
    {
        if (mode == CaptureMode.ActiveWindow)
        {
            var window = PInvoke.GetForegroundWindow();
            if (window != HWND.Null)
            {
                try
                {
                    return GraphicsCaptureInterop.CreateItemForWindow(window);
                }
                catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or ArgumentException)
                {
                    // Some windows cannot be captured; the whole desktop is the next best, still visible, option.
                    _logger.LogWarning(ex, "Foreground window cannot be captured; capturing the desktop instead");
                }
            }
        }

        var monitor = PInvoke.MonitorFromWindow(HWND.Null, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);
        return GraphicsCaptureInterop.CreateItemForMonitor(monitor);
    }

    private void EnsureDevice()
    {
        if (_device is not null)
        {
            return;
        }

        var result = D3D11.D3D11CreateDevice(
            null,
            DriverType.Hardware,
            DeviceCreationFlags.BgraSupport,
            [FeatureLevel.Level_11_1, FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0],
            out _device,
            out _context);
        result.CheckError();

        using var dxgiDevice = _device!.QueryInterface<IDXGIDevice>();
        _winRtDevice = GraphicsCaptureInterop.CreateDirect3DDevice(dxgiDevice.NativePointer);
    }

    private async Task<ScreenshotResult> CaptureItemAsync(GraphicsCaptureItem item, DateTimeOffset capturedAt, CancellationToken cancellationToken)
    {
        using var framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
            _winRtDevice!, DirectXPixelFormat.B8G8R8A8UIntNormalized, 1, item.Size);
        using var session = framePool.CreateCaptureSession(item);
        session.IsCursorCaptureEnabled = false;

        var frameSource = new TaskCompletionSource<Direct3D11CaptureFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
        framePool.FrameArrived += (sender, _) =>
        {
            var next = sender.TryGetNextFrame();
            if (next is not null && !frameSource.TrySetResult(next))
            {
                next.Dispose();
            }
        };

        session.StartCapture();
        using var frame = await frameSource.Task.WaitAsync(FrameTimeout, cancellationToken);
        return ReadFrame(frame, capturedAt);
    }

    private ScreenshotResult ReadFrame(Direct3D11CaptureFrame frame, DateTimeOffset capturedAt)
    {
        var texturePointer = GraphicsCaptureInterop.GetNativeTexture(frame.Surface, typeof(ID3D11Texture2D).GUID);
        using var texture = new ID3D11Texture2D(texturePointer);

        var description = texture.Description;
        description.Usage = ResourceUsage.Staging;
        description.BindFlags = BindFlags.None;
        description.CPUAccessFlags = CpuAccessFlags.Read;
        description.MiscFlags = ResourceOptionFlags.None;

        using var staging = _device!.CreateTexture2D(description);
        _context!.CopyResource(staging, texture);

        var width = Math.Min(frame.ContentSize.Width, (int)description.Width);
        var height = Math.Min(frame.ContentSize.Height, (int)description.Height);

        var mapped = _context.Map(staging, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
        try
        {
            var png = BgraPngEncoder.Encode(mapped.DataPointer, width, height, (int)mapped.RowPitch);
            return new ScreenshotResult(png, width, height, capturedAt);
        }
        finally
        {
            _context.Unmap(staging, 0);
        }
    }
}
