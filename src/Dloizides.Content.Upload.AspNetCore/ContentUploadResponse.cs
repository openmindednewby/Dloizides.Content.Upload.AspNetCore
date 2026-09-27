namespace Dloizides.Content.Upload.AspNetCore;

/// <summary>Result of a successful upload.</summary>
/// <param name="ContentId">ContentService id of the stored item.</param>
/// <param name="Url">Public URL of the stored image.</param>
public sealed record ContentUploadResponse(string ContentId, string Url);
