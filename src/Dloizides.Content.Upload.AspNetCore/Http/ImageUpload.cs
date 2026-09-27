namespace Dloizides.Content.Upload.AspNetCore.Http;

/// <summary>A verified image on its way to ContentService.</summary>
/// <param name="Content">Image bytes.</param>
/// <param name="FileName">File name, with the extension matching <paramref name="MediaType"/>.</param>
/// <param name="MediaType">Media type proven by the magic bytes.</param>
/// <param name="Authorization">Caller's Authorization header, forwarded as-is.</param>
public sealed record ImageUpload(Stream Content, string FileName, string MediaType, string? Authorization);
