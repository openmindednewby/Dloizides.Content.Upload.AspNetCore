namespace Dloizides.Content.Upload.AspNetCore;

/// <summary>Result of <see cref="IContentUploadProcessor.ProcessAsync"/>: a response or a failure.</summary>
public sealed class UploadOutcome
{
  private UploadOutcome(ContentUploadResponse? value, UploadFailure failure, string message)
  {
    Value = value;
    Failure = failure;
    Message = message;
  }

  public ContentUploadResponse? Value { get; }

  public UploadFailure Failure { get; }

  public string Message { get; }

  public bool IsSuccess => Failure == UploadFailure.None;

  public static UploadOutcome Success(ContentUploadResponse value) => new(value, UploadFailure.None, string.Empty);

  public static UploadOutcome Fail(UploadFailure failure, string message) => new(null, failure, message);
}
