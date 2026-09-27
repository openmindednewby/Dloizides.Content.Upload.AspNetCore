using System.Net;
using System.Security.Claims;
using Dloizides.Content.Upload.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Dloizides.Content.Upload.AspNetCore.Tests;

public class ContentUploadProcessorTests
{
  private const string UploaderRole = "Uploader";
  private const string TenantId = "11111111-1111-1111-1111-111111111111";
  private const string Bearer = "Bearer test-token";
  private const long CapBytes = 1024;

  private static readonly byte[] PngBytes =
    [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52];

  private readonly FakeContentServiceHandler _handler = new();

  private readonly ContentUploadOptions _options = new()
  {
    Route = "/uploads/image",
    AllowedRoles = [UploaderRole],
    MaxBytes = CapBytes,
    ContentServiceBaseUrl = new Uri("http://content-api/"),
  };

  [Fact]
  public async Task ProcessAsync_WhenUserLacksAllowedRole_ReturnsForbiddenWithoutCallingContentService()
  {
    var outcome = await CreateSut().ProcessAsync(Principal("Doorstaff", TenantId), Image(PngBytes, "image/png"), Bearer, default);

    Assert.Equal(UploadFailure.Forbidden, outcome.Failure);
    Assert.Equal(0, _handler.Requests);
  }

  [Fact]
  public async Task ProcessAsync_WhenTenantClaimMissing_ReturnsForbidden()
  {
    var outcome = await CreateSut().ProcessAsync(Principal(UploaderRole, null), Image(PngBytes, "image/png"), Bearer, default);

    Assert.Equal(UploadFailure.Forbidden, outcome.Failure);
    Assert.Equal(0, _handler.Requests);
  }

  [Fact]
  public async Task ProcessAsync_WhenFileMissing_ReturnsMissingFile()
  {
    var outcome = await CreateSut().ProcessAsync(Principal(UploaderRole, TenantId), null, Bearer, default);

    Assert.Equal(UploadFailure.MissingFile, outcome.Failure);
  }

  [Fact]
  public async Task ProcessAsync_WhenFileExceedsCap_ReturnsTooLargeWithoutCallingContentService()
  {
    var oversized = new byte[CapBytes + 1];
    PngBytes.CopyTo(oversized, 0);

    var outcome = await CreateSut().ProcessAsync(Principal(UploaderRole, TenantId), Image(oversized, "image/png"), Bearer, default);

    Assert.Equal(UploadFailure.TooLarge, outcome.Failure);
    Assert.Equal(0, _handler.Requests);
  }

  [Fact]
  public async Task ProcessAsync_WhenBytesAreNotAnImageDespiteImageHeader_ReturnsNotAnImage()
  {
    var html = "<html><script>alert(1)</script></html>"u8.ToArray();

    var outcome = await CreateSut().ProcessAsync(Principal(UploaderRole, TenantId), Image(html, "image/png"), Bearer, default);

    Assert.Equal(UploadFailure.NotAnImage, outcome.Failure);
    Assert.Equal(0, _handler.Requests);
  }

  [Fact]
  public async Task ProcessAsync_WhenDeclaredTypeIsNotImage_ReturnsNotAnImage()
  {
    var outcome = await CreateSut().ProcessAsync(Principal(UploaderRole, TenantId), Image(PngBytes, "application/pdf"), Bearer, default);

    Assert.Equal(UploadFailure.NotAnImage, outcome.Failure);
  }

  [Fact]
  public async Task ProcessAsync_WhenValidPng_ForwardsBearerAndSniffedTypeAndReturnsPublicUrl()
  {
    var outcome = await CreateSut().ProcessAsync(Principal(UploaderRole, TenantId), Image(PngBytes, "image/jpeg"), Bearer, default);

    Assert.True(outcome.IsSuccess);
    Assert.Equal(FakeContentServiceHandler.PublicUrl, outcome.Value!.Url);
    Assert.Equal("c-1", outcome.Value.ContentId);
    Assert.Equal(1, _handler.Requests);
    Assert.Equal("/api/v1/content/upload", _handler.LastPath);
    Assert.Equal(Bearer, _handler.LastAuthorization);
    Assert.Contains("Content-Type: image/png", _handler.LastBody);
    Assert.Contains("name=File; filename=photo.png", _handler.LastBody);
    Assert.Contains("name=Category\r\n\r\nImage", _handler.LastBody);
    Assert.Contains("name=IsPublic\r\n\r\ntrue", _handler.LastBody);
  }

  [Fact]
  public async Task ProcessAsync_WhenContentServiceFails_ReturnsUpstreamFailed()
  {
    _handler.Status = HttpStatusCode.InternalServerError;

    var outcome = await CreateSut().ProcessAsync(Principal(UploaderRole, TenantId), Image(PngBytes, "image/png"), Bearer, default);

    Assert.Equal(UploadFailure.UpstreamFailed, outcome.Failure);
  }

  [Fact]
  public async Task ProcessAsync_WhenContentServiceReturnsNoUrl_ReturnsUpstreamFailed()
  {
    _handler.ResponseJson = "{\"contentId\":\"c-1\",\"status\":\"Ready\"}";

    var outcome = await CreateSut().ProcessAsync(Principal(UploaderRole, TenantId), Image(PngBytes, "image/png"), Bearer, default);

    Assert.Equal(UploadFailure.UpstreamFailed, outcome.Failure);
  }

  private ContentUploadProcessor CreateSut()
  {
    var options = Options.Create(_options);
    var http = new HttpClient(_handler) { BaseAddress = _options.ContentServiceBaseUrl };
    var client = new ContentServiceUploadClient(http, options, NullLogger<ContentServiceUploadClient>.Instance);
    return new ContentUploadProcessor(client, options, NullLogger<ContentUploadProcessor>.Instance);
  }

  private static ClaimsPrincipal Principal(string role, string? tenantId)
  {
    var claims = new List<Claim> { new(ClaimTypes.Role, role) };
    if (tenantId is not null)
      claims.Add(new Claim(ContentUploadOptions.DefaultTenantClaimType, tenantId));
    return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
  }

  private static FormFile Image(byte[] bytes, string contentType) =>
    new(new MemoryStream(bytes), 0, bytes.Length, "File", "photo.jpg")
    {
      Headers = new HeaderDictionary(),
      ContentType = contentType,
    };
}
