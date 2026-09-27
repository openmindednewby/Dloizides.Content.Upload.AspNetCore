namespace Dloizides.Content.Upload.AspNetCore.Http;

/// <summary>Sends an image to ContentService's upload endpoint.</summary>
public interface IContentUploadClient
{
  /// <returns>The stored item, or <c>null</c> when ContentService refused or returned no public URL.</returns>
  Task<ContentUploadResponse?> UploadAsync(ImageUpload upload, CancellationToken ct);
}
