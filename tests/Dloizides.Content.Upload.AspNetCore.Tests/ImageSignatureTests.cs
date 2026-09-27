namespace Dloizides.Content.Upload.AspNetCore.Tests;

public class ImageSignatureTests
{
  [Theory]
  [InlineData("89504E470D0A1A0A0000000D", "image/png")]
  [InlineData("FFD8FFE000104A4649460001", "image/jpeg")]
  [InlineData("474946383961010001000000", "image/gif")]
  [InlineData("524946462400000057454250", "image/webp")]
  public void Detect_WhenHeaderIsSupportedImage_ReturnsMediaType(string hex, string expected)
  {
    Assert.Equal(expected, ImageSignature.Detect(Convert.FromHexString(hex)));
  }

  [Theory]
  [InlineData("3C3F786D6C2076657273696F")] // <?xml (SVG)
  [InlineData("524946462400000057415645")] // RIFF....WAVE
  [InlineData("89504E47")] // truncated PNG signature
  [InlineData("")]
  public void Detect_WhenHeaderIsNotSupportedImage_ReturnsNull(string hex)
  {
    Assert.Null(ImageSignature.Detect(Convert.FromHexString(hex)));
  }
}
