using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Dloizides.Content.Upload.AspNetCore;

/// <summary>
/// <c>POST {Route}</c>, multipart <c>File</c>. Returns 201 with the public URL, or 400 (no file),
/// 401, 403 (role or tenant claim missing), 413 (over the cap), 415 (not an image by magic bytes),
/// 502 (ContentService failed). Route, roles and cap come from <see cref="ContentUploadOptions"/>.
/// </summary>
public sealed class ContentUploadEndpoint(
    IOptions<ContentUploadOptions> options,
    IContentUploadProcessor processor) : Endpoint<ContentUploadRequest, ContentUploadResponse>
{
  public override void Configure()
  {
    var settings = options.Value;
    Post(settings.Route);
    Roles(settings.AllowedRoles);
    AllowFileUploads();
    Description(b => b
      .Produces<ContentUploadResponse>(StatusCodes.Status201Created, "application/json")
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status413PayloadTooLarge)
      .Produces(StatusCodes.Status415UnsupportedMediaType)
      .Produces(StatusCodes.Status502BadGateway));
    Summary(s => s.Summary = "Upload an image to ContentService and return its public URL");
  }

  public override async Task HandleAsync(ContentUploadRequest req, CancellationToken ct)
  {
    var authorization = HttpContext.Request.Headers.Authorization.ToString();
    var outcome = await processor.ProcessAsync(User, req.File, authorization, ct);
    if (outcome.IsSuccess)
    {
      await Send.ResponseAsync(outcome.Value!, StatusCodes.Status201Created, ct);
      return;
    }

    AddError(outcome.Message);
    await Send.ErrorsAsync(UploadFailureStatus.ToStatusCode(outcome.Failure), ct);
  }
}
