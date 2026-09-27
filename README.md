# Dloizides.Content.Upload.AspNetCore

A FastEndpoints image-upload endpoint that stores the file in ContentService and returns its
public URL. The host decides the route and which roles may upload, so a product can narrow
uploads to, say, organizers, which ContentService's shared `admin`/`user` roles cannot express.

## What it enforces

| Check | Result |
|---|---|
| Caller has none of `AllowedRoles`, or no tenant claim | 403 |
| No `File` part, or an empty file | 400 |
| File larger than `MaxBytes` (default 10 MB) | 413 |
| Magic bytes are not PNG, JPEG, GIF or WebP, or the declared type is not `image/*` | 415 |
| ContentService refuses or returns no URL | 502 |
| Success | 201 `{ "contentId": "...", "url": "https://..." }` |

SVG is rejected on purpose (it can carry script). The file is sent upstream with the media type
the bytes prove and a matching extension, never the client's claim.

The caller's `Authorization` header is forwarded unchanged. ContentService takes the tenant from
that token and prefixes the blob path with it, so one tenant cannot write into another's folder.

## Use

```csharp
using Dloizides.Content.Upload.AspNetCore.Extensions;

builder.Services.AddContentUpload(o =>
{
    o.Route = "/admin/uploads/image";
    o.AllowedRoles = ["Organizer", "TenantOwner"];
    o.ContentServiceBaseUrl = new Uri(builder.Configuration["Services:ContentApi"]!);
    // optional: o.MaxBytes, o.TenantClaimType ("tenantId"), o.UploadPath ("api/v1/content/upload")
});
```

`AddContentUpload` validates the options at startup and throws `InvalidOperationException` when
`Route`, `AllowedRoles` or `ContentServiceBaseUrl` is missing.

FastEndpoints finds `ContentUploadEndpoint` by assembly scan. A host that sets
`o.Assemblies` explicitly must include `typeof(ContentUploadEndpoint).Assembly`.

## Publish

First release: `NuGetPackages/publish-first.ps1 -Package Dloizides.Content.Upload.AspNetCore`.
Later releases: `./publish.ps1 -Bump patch`. Commit first; the NuGet path has no dirty-tree gate.
