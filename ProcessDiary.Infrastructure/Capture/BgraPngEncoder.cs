using SkiaSharp;

namespace ProcessDiary.Infrastructure.Capture;

/// <summary>Encodes raw BGRA pixel data as PNG.</summary>
internal static class BgraPngEncoder
{
    /// <summary>Encodes a pixel buffer as PNG.</summary>
    /// <param name="pixels">Pointer to the first pixel row (BGRA, 8 bits per channel).</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="rowPitch">Number of bytes between the starts of consecutive rows.</param>
    /// <returns>The PNG bytes.</returns>
    public static byte[] Encode(IntPtr pixels, int width, int height, int rowPitch)
    {
        // Screens are opaque; ignoring alpha avoids transparent output from capture surfaces.
        var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        using var image = SKImage.FromPixels(info, pixels, rowPitch);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
