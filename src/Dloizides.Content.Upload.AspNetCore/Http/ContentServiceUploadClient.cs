using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dloizides.Content.Upload.AspNetCore.Http;

/// <summary>
/// Typed HttpClient for ContentService <c>POST api/v1/content/upload</c>: multipart
/// <c>File</c> + <c>Category=Image</c> + <c>IsPublic=true</c>, response <c>{ contentId, status, url }</c>.
/// ContentService derives the tenant from the forwarded bearer token.
/// </summary>
internal sealed class ContentServiceUploadClient(
    HttpClient http,
    IOptions<ContentUploadOptions> options,
    ILogger<ContentServiceUploadClient> logger) : IContentUploadClient
{
  public async Task<ContentUploadResponse?> UploadAsync(ImageUpload upload, CancellationToken ct)
  {
    using var form = new MultipartFormDataContent();
    var file = new StreamContent(upload.Content);
    file.Headers.ContentType = new MediaTypeHeaderValue(upload.MediaType);
    form.Add(file, "File", upload.FileName);
    form.Add(new StringContent("Image"), "Category");
    form.Add(new StringContent("true"), "IsPublic");

    using var request = new HttpRequestMessage(HttpMethod.Post, options.Value.UploadPath) { Content = form };
    if (!string.IsNullOrWhiteSpace(upload.Authorization))
      request.Headers.TryAddWithoutValidation("Authorization", upload.Authorization);

    using var response = await http.SendAsync(request, ct);
    if (!response.IsSuccessStatusCode)
    {
      logger.LogWarning("ContentService upload returned {StatusCode}", (int)response.StatusCode);
      return null;
    }

    var body = await response.Content.ReadFromJsonAsync<UpstreamBody>(ct);
    return body is null ? null : ToResponse(body);
  }

  private ContentUploadResponse? ToResponse(UpstreamBody body)
  {
    var settings = options.Value;
    if (settings.PublicBaseUrl is null)
    {
      if (!string.IsNullOrWhiteSpace(body.Url))
        return new ContentUploadResponse(body.ContentId ?? string.Empty, body.Url);
      logger.LogWarning("ContentService upload succeeded but returned no URL");
      return null;
    }

    if (string.IsNullOrWhiteSpace(body.ContentId))
    {
      logger.LogWarning("ContentService upload succeeded but returned no content id");
      return null;
    }

    return new ContentUploadResponse(body.ContentId, settings.BuildPublicUrl(body.ContentId)!.ToString());
  }

  private sealed record UpstreamBody(string? ContentId, string? Url);
}
