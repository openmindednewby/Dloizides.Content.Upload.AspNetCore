namespace Dloizides.Content.Upload.AspNetCore;

/// <summary>Why an upload was refused. <see cref="None"/> means it succeeded.</summary>
public enum UploadFailure
{
  None = 0,
  Forbidden,
  MissingFile,
  TooLarge,
  NotAnImage,
  UpstreamFailed,
}
