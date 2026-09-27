using Dloizides.Content.Upload.AspNetCore.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Dloizides.Content.Upload.AspNetCore.Tests;

public class ContentUploadServiceCollectionExtensionsTests
{
  private static readonly Uri BaseUrl = new("http://content-api:8080");

  [Fact]
  public void AddContentUpload_WhenRouteMissing_Throws()
  {
    var services = new ServiceCollection();

    Assert.Throws<InvalidOperationException>(() => services.AddContentUpload(o =>
    {
      o.AllowedRoles = ["Organizer"];
      o.ContentServiceBaseUrl = BaseUrl;
    }));
  }

  [Fact]
  public void AddContentUpload_WhenNoRoles_Throws()
  {
    var services = new ServiceCollection();

    Assert.Throws<InvalidOperationException>(() => services.AddContentUpload(o =>
    {
      o.Route = "/uploads/image";
      o.ContentServiceBaseUrl = BaseUrl;
    }));
  }

  [Fact]
  public void AddContentUpload_WhenPublicBaseUrlIsHttp_Throws()
  {
    var services = new ServiceCollection();

    Assert.Throws<InvalidOperationException>(() => services.AddContentUpload(o =>
    {
      o.Route = "/uploads/image";
      o.AllowedRoles = ["Organizer"];
      o.ContentServiceBaseUrl = BaseUrl;
      o.PublicBaseUrl = new Uri("http://content-api.example.com");
    }));
  }

  [Fact]
  public void AddContentUpload_WhenPublicDownloadPathLacksPlaceholder_Throws()
  {
    var services = new ServiceCollection();

    Assert.Throws<InvalidOperationException>(() => services.AddContentUpload(o =>
    {
      o.Route = "/uploads/image";
      o.AllowedRoles = ["Organizer"];
      o.ContentServiceBaseUrl = BaseUrl;
      o.PublicBaseUrl = new Uri("https://content-api.example.com");
      o.PublicDownloadPath = "api/v1/content/public-download";
    }));
  }

  [Fact]
  public void AddContentUpload_WhenValid_ResolvesProcessor()
  {
    var services = new ServiceCollection().AddLogging();
    services.AddContentUpload(o =>
    {
      o.Route = "/uploads/image";
      o.AllowedRoles = ["Organizer"];
      o.ContentServiceBaseUrl = BaseUrl;
    });

    using var provider = services.BuildServiceProvider(validateScopes: true);

    Assert.NotNull(provider.GetRequiredService<IContentUploadProcessor>());
  }
}
