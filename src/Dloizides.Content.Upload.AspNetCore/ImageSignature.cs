namespace Dloizides.Content.Upload.AspNetCore;

/// <summary>
/// Identifies an image by its leading magic bytes. The declared Content-Type is client-controlled,
/// so it is never trusted on its own. SVG is deliberately absent: it is a script-capable document.
/// </summary>
public static class ImageSignature
{
  /// <summary>Bytes needed to recognise every supported format (WebP needs 12).</summary>
  public const int HeaderLength = 12;

  private const int WebpTagOffset = 8;

  private static ReadOnlySpan<byte> Png => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
  private static ReadOnlySpan<byte> Jpeg => [0xFF, 0xD8, 0xFF];
  private static ReadOnlySpan<byte> Gif87 => "GIF87a"u8;
  private static ReadOnlySpan<byte> Gif89 => "GIF89a"u8;
  private static ReadOnlySpan<byte> Riff => "RIFF"u8;
  private static ReadOnlySpan<byte> Webp => "WEBP"u8;

  /// <summary>Returns the media type the bytes prove, or <c>null</c> when they are not a supported image.</summary>
  public static string? Detect(ReadOnlySpan<byte> header)
  {
    if (header.StartsWith(Png))
      return "image/png";
    if (header.StartsWith(Jpeg))
      return "image/jpeg";
    if (header.StartsWith(Gif87) || header.StartsWith(Gif89))
      return "image/gif";
    var isWebp = header.Length >= HeaderLength && header.StartsWith(Riff) && header[WebpTagOffset..].StartsWith(Webp);
    return isWebp ? "image/webp" : null;
  }

  /// <summary>File extension (with dot) for a media type returned by <see cref="Detect"/>.</summary>
  public static string ExtensionFor(string mediaType) => mediaType switch
  {
    "image/png" => ".png",
    "image/jpeg" => ".jpg",
    "image/gif" => ".gif",
    _ => ".webp",
  };
}
