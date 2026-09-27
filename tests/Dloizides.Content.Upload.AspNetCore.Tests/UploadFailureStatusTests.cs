namespace Dloizides.Content.Upload.AspNetCore.Tests;

public class UploadFailureStatusTests
{
  [Theory]
  [InlineData(UploadFailure.Forbidden, 403)]
  [InlineData(UploadFailure.MissingFile, 400)]
  [InlineData(UploadFailure.TooLarge, 413)]
  [InlineData(UploadFailure.NotAnImage, 415)]
  [InlineData(UploadFailure.UpstreamFailed, 502)]
  public void ToStatusCode_ForEachFailure_ReturnsDocumentedStatus(UploadFailure failure, int expected)
  {
    Assert.Equal(expected, UploadFailureStatus.ToStatusCode(failure));
  }
}
