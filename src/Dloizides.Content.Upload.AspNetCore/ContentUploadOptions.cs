namespace Dloizides.Content.Upload.AspNetCore;

/// <summary>
/// Host-supplied settings for the upload endpoint. <see cref="Route"/>, <see cref="AllowedRoles"/>
/// and <see cref="ContentServiceBaseUrl"/> are required; everything else has a default.
/// </summary>
public sealed class ContentUploadOptions
{
  /// <summary>Default size cap: 10 MB, the limit the portals apply to images.</summary>
  public const long DefaultMaxBytes = 10L * 1024 * 1024;

  /// <summary>ContentService's proxied upload route (FastEndpoints prefix <c>api/v1</c>).</summary>
  public const string DefaultUploadPath = "api/v1/content/upload";

  /// <summary>Claim that carries the caller's tenant id.</summary>
  public const string DefaultTenantClaimType = "tenantId";

  /// <summary>Route the endpoint is mounted on, e.g. <c>/admin/uploads/image</c>.</summary>
  public string Route { get; set; } = string.Empty;

  /// <summary>Roles allowed to upload. A caller needs at least one of them.</summary>
  public string[] AllowedRoles { get; set; } = [];

  /// <summary>Largest accepted file, in bytes. Larger files get 413.</summary>
  public long MaxBytes { get; set; } = DefaultMaxBytes;

  /// <summary>Base URL of ContentService, e.g. <c>http://content-api:8080/</c>.</summary>
  public Uri? ContentServiceBaseUrl { get; set; }

  /// <summary>Path of ContentService's upload endpoint, relative to <see cref="ContentServiceBaseUrl"/>.</summary>
  public string UploadPath { get; set; } = DefaultUploadPath;

  /// <summary>Claim type the tenant id is read from. A caller without it gets 403.</summary>
  public string TenantClaimType { get; set; } = DefaultTenantClaimType;

  internal void Validate()
  {
    if (string.IsNullOrWhiteSpace(Route))
      throw new InvalidOperationException("ContentUploadOptions.Route is required.");
    if (AllowedRoles.Length == 0 || AllowedRoles.Any(string.IsNullOrWhiteSpace))
      throw new InvalidOperationException("ContentUploadOptions.AllowedRoles needs at least one non-empty role.");
    if (MaxBytes <= 0)
      throw new InvalidOperationException("ContentUploadOptions.MaxBytes must be positive.");
    if (ContentServiceBaseUrl is null || !ContentServiceBaseUrl.IsAbsoluteUri)
      throw new InvalidOperationException("ContentUploadOptions.ContentServiceBaseUrl must be an absolute URI.");
    if (string.IsNullOrWhiteSpace(UploadPath) || string.IsNullOrWhiteSpace(TenantClaimType))
      throw new InvalidOperationException("ContentUploadOptions.UploadPath and TenantClaimType must not be empty.");
  }
}
