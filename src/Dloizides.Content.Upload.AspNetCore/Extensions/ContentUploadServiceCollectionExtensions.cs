using Dloizides.Content.Upload.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dloizides.Content.Upload.AspNetCore.Extensions;

public static class ContentUploadServiceCollectionExtensions
{
  /// <summary>
  /// Registers <see cref="ContentUploadEndpoint"/>'s dependencies. Invalid options throw here, at
  /// startup, not on the first upload. FastEndpoints discovers the endpoint by assembly scan; a host
  /// that lists assemblies explicitly must add <c>typeof(ContentUploadEndpoint).Assembly</c>.
  /// </summary>
  public static IServiceCollection AddContentUpload(this IServiceCollection services, Action<ContentUploadOptions> configure)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configure);

    var probe = new ContentUploadOptions();
    configure(probe);
    probe.Validate();

    services.Configure(configure);
    services.AddHttpClient<IContentUploadClient, ContentServiceUploadClient>(
        client => client.BaseAddress = WithTrailingSlash(probe.ContentServiceBaseUrl!));
    services.AddTransient<IContentUploadProcessor, ContentUploadProcessor>();
    return services;
  }

  private static Uri WithTrailingSlash(Uri baseUrl)
  {
    var text = baseUrl.ToString();
    return text.EndsWith('/') ? baseUrl : new Uri(text + "/");
  }
}
