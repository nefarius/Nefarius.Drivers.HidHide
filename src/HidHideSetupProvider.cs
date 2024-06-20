#nullable enable

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Nefarius.Vicius.Abstractions.Converters;
using Nefarius.Vicius.Abstractions.Models;

namespace Nefarius.Drivers.HidHide;

/// <summary>
///     Service to locate the latest HidHide setup resources, update information etc.
/// </summary>
public sealed class HidHideSetupProvider
{
    private readonly HttpClient _client;

    /// <summary>
    ///     Creates new instance of HidHide Setup Provider.
    /// </summary>
    public HidHideSetupProvider(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    ///     Fetches the <see cref="UpdateResponse" /> from the HidHide CDN server.
    /// </summary>
    public Task<UpdateResponse?> GetUpdateInformationAsync(CancellationToken ct = default)
    {
        JsonSerializerOptions opts = new()
        {
            // the client can handle missing fields that are optional, no need to transmit null values
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // server supplies camelCase
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        // we exchange timestamps as ISO 8601 string (UTC)
        opts.Converters.Add(new DateTimeOffsetConverter());
        // we use the enum value names (strings) instead of numerical values
        opts.Converters.Add(new JsonStringEnumConverter());

        return _client.GetFromJsonAsync<UpdateResponse>("/api/nefarius/HidHide/updates.json", opts, ct);
    }

    /// <summary>
    ///     Fetches the latest setup download URL or null if not found.
    /// </summary>
    public async Task<Uri> GetLatestDownloadUrl(CancellationToken ct = default)
    {
        UpdateResponse? updates;

        try
        {
            updates = await GetUpdateInformationAsync(ct);
        }
        catch (Exception ex)
        {
            throw new UpdateResponseMissingException(ex);
        }

        if (updates is null)
        {
            throw new UpdateResponseMissingException();
        }

        UpdateRelease? release = updates.Releases.FirstOrDefault();

        if (release is null)
        {
            throw new MissingReleasesException();
        }

        string? location = release.DownloadUrl;

        if (string.IsNullOrEmpty(location))
        {
            throw new DownloadLocationMissingException();
        }

        return Uri.TryCreate(location, UriKind.Absolute, out Uri? uri)
            ? uri
            : throw new MalformedUrlException();
    }
}