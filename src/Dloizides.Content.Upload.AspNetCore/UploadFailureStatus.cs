using Microsoft.AspNetCore.Http;

namespace Dloizides.Content.Upload.AspNetCore;

/// <summary>Maps an <see cref="UploadFailure"/> to its HTTP status code.</summary>
public static class UploadFailureStatus
{
  public static int ToStatusCode(UploadFailure failure) => failure switch
  {
    UploadFailure.Forbidden => StatusCodes.Status403Forbidden,
    UploadFailure.MissingFile => StatusCodes.Status400BadRequest,
    UploadFailure.TooLarge => StatusCodes.Status413PayloadTooLarge,
    UploadFailure.NotAnImage => StatusCodes.Status415UnsupportedMediaType,
    UploadFailure.UpstreamFailed => StatusCodes.Status502BadGateway,
    _ => StatusCodes.Status500InternalServerError,
  };
}
