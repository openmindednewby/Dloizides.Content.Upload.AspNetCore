using System.Security.Claims;
using Dloizides.Content.Upload.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dloizides.Content.Upload.AspNetCore;

/// <summary>
/// Default <see cref="IContentUploadProcessor"/>. The role check repeats the endpoint's
/// <c>Roles(...)</c> gate on purpose, so the processor cannot be called around it.
/// </summary>
internal sealed class ContentUploadProcessor(
    IContentUploadClient client,
    IOptions<ContentUploadOptions> options,
    ILogger<ContentUploadProcessor> logger) : IContentUploadProcessor
{
  private const string ImageMediaPrefix = "image/";

  public async Task<UploadOutcome> ProcessAsync(ClaimsPrincipal user, IFormFile? file, string? authorization, CancellationToken ct)
  {
    var settings = options.Value;
    var tenantId = user.FindFirst(settings.TenantClaimType)?.Value;
    var hasRole = settings.AllowedRoles.Any(user.IsInRole);
    if (!hasRole || string.IsNullOrWhiteSpace(tenantId))
      return UploadOutcome.Fail(UploadFailure.Forbidden, "You are not allowed to upload images for this tenant.");

    if (file is null || file.Length == 0)
      return UploadOutcome.Fail(UploadFailure.MissingFile, "An image file is required.");

    if (file.Length > settings.MaxBytes)
      return UploadOutcome.Fail(UploadFailure.TooLarge, $"The image exceeds the {settings.MaxBytes}-byte limit.");

    var mediaType = await SniffAsync(file, ct);
    var declaredImage = file.ContentType?.StartsWith(ImageMediaPrefix, StringComparison.OrdinalIgnoreCase) == true;
    if (mediaType is null || !declaredImage)
      return UploadOutcome.Fail(UploadFailure.NotAnImage, "Only PNG, JPEG, GIF or WebP images are accepted.");

    var fileName = Path.ChangeExtension(Path.GetFileName(file.FileName), ImageSignature.ExtensionFor(mediaType));
    await using var content = file.OpenReadStream();
    var uploaded = await client.UploadAsync(new ImageUpload(content, fileName, mediaType, authorization), ct);
    if (uploaded is null)
      return UploadOutcome.Fail(UploadFailure.UpstreamFailed, "The image could not be stored. Try again.");

    logger.LogInformation("Image uploaded for tenant {TenantId}: {ContentId}", tenantId, uploaded.ContentId);
    return UploadOutcome.Success(uploaded);
  }

  private static async Task<string?> SniffAsync(IFormFile file, CancellationToken ct)
  {
    var header = new byte[ImageSignature.HeaderLength];
    await using var stream = file.OpenReadStream();
    var read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
    return ImageSignature.Detect(header.AsSpan(0, read));
  }
}
