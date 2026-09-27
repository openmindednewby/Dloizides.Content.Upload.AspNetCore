using System.Net;
using System.Text;

namespace Dloizides.Content.Upload.AspNetCore.Tests;

/// <summary>Stands in for ContentService: records the request, returns a canned response.</summary>
internal sealed class FakeContentServiceHandler : HttpMessageHandler
{
  public const string PublicUrl = "https://cdn.example/public/t1/image/c-1.png";

  public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

  public string ResponseJson { get; set; } = "{\"contentId\":\"c-1\",\"status\":\"Ready\",\"url\":\"" + PublicUrl + "\"}";

  public int Requests { get; private set; }

  public string? LastPath { get; private set; }

  public string? LastAuthorization { get; private set; }

  public string LastBody { get; private set; } = string.Empty;

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    Requests++;
    LastPath = request.RequestUri!.AbsolutePath;
    LastAuthorization = request.Headers.Authorization?.ToString();
    LastBody = await request.Content!.ReadAsStringAsync(cancellationToken);
    return new HttpResponseMessage(Status) { Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json") };
  }
}
