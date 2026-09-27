using Microsoft.AspNetCore.Http;

namespace Dloizides.Content.Upload.AspNetCore;

/// <summary>Multipart request: one file part named <c>File</c>.</summary>
public sealed class ContentUploadRequest
{
  /// <summary>The uploaded image.</summary>
  public IFormFile? File { get; set; }
}
