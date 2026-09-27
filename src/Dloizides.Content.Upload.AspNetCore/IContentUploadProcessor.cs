using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Dloizides.Content.Upload.AspNetCore;

/// <summary>Checks the caller and the file, then forwards the image to ContentService.</summary>
public interface IContentUploadProcessor
{
  /// <param name="user">Authenticated caller.</param>
  /// <param name="file">Uploaded file, or <c>null</c> when the part was missing.</param>
  /// <param name="authorization">Caller's <c>Authorization</c> header, forwarded to ContentService.</param>
  /// <param name="ct">Cancellation token.</param>
  Task<UploadOutcome> ProcessAsync(ClaimsPrincipal user, IFormFile? file, string? authorization, CancellationToken ct);
}
