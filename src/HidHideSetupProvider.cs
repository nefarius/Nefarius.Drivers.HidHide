#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
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
[SuppressMessage("ReSharper", "UnusedMember.Global")]
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
    /// <exception cref="OperationCanceledException">
    ///     The cancellation token was canceled. This exception is stored into the
    ///     returned task.
    /// </exception>
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
    ///     Fetches the latest setup download URL.
    /// </summary>
    /// <exception cref="UpdateResponseMissingException">
    ///     Server didn't respond with a proper reply, see
    ///     <see cref="Exception.InnerException" /> for details.
    /// </exception>
    /// <exception cref="MissingReleasesException">Mandatory releases collection was empty.</exception>
    /// <exception cref="DownloadLocationMissingException">Mandatory release download location was missing.</exception>
    /// <exception cref="MalformedUrlException">Provided download URL was malformed.</exception>
    public async Task<Uri> GetLatestDownloadUrlAsync(CancellationToken ct = default)
    {
        UpdateRelease release = await GetLatestReleaseAsync(ct);

        string location = release.DownloadUrl;

        if (string.IsNullOrEmpty(location))
        {
            throw new DownloadLocationMissingException();
        }

        return Uri.TryCreate(location, UriKind.Absolute, out Uri? uri)
            ? uri
            : throw new MalformedUrlException();
    }

    /// <summary>
    ///     Fetches the latest available version.
    /// </summary>
    /// <exception cref="UpdateResponseMissingException">
    ///     Server didn't respond with a proper reply, see
    ///     <see cref="Exception.InnerException" /> for details.
    /// </exception>
    /// <exception cref="MissingReleasesException">Mandatory releases collection was empty.</exception>
    public async Task<Version> GetLatestVersionAsync(CancellationToken ct = default)
    {
        UpdateRelease release = await GetLatestReleaseAsync(ct);

        return release.Version;
    }

    /// <summary>
    ///     Fetches the latest available release.
    /// </summary>
    /// <param name="ct">Optional <see cref="CancellationToken" />.</param>
    /// <returns> The latest <see cref="UpdateRelease" /> available.</returns>
    /// <exception cref="UpdateResponseMissingException">
    ///     Server didn't respond with a proper reply, see
    ///     <see cref="Exception.InnerException" /> for details.
    /// </exception>
    /// <exception cref="MissingReleasesException">Mandatory releases collection was empty.</exception>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public async Task<UpdateRelease> GetLatestReleaseAsync(CancellationToken ct = default)
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

        UpdateRelease? release = updates.Releases.OrderByDescending(r => r.Version).FirstOrDefault();

        if (release is null)
        {
            throw new MissingReleasesException();
        }

        return release;
    }

    /// <summary>
    ///     Downloads the setup asset of the most recent <see cref="UpdateRelease" />.
    /// </summary>
    /// <param name="ct">Optional <see cref="CancellationToken" />.</param>
    /// <returns>A <see cref="HttpResponseMessage" />.</returns>
    /// <exception cref="UpdateResponseMissingException">
    ///     Server didn't respond with a proper reply, see
    ///     <see cref="Exception.InnerException" /> for details.
    /// </exception>
    /// <exception cref="MissingReleasesException">Mandatory releases collection was empty.</exception>
    /// <exception cref="HttpRequestException">Server communication error occurred.</exception>
    public async Task<HttpResponseMessage> DownloadLatestReleaseAsync(CancellationToken ct = default)
    {
        UpdateRelease release = await GetLatestReleaseAsync(ct);

        return await _client.GetAsync(release.DownloadUrl, ct);
    }

    /// <summary>
    ///     Downloads the setup asset of the provided <see cref="UpdateRelease" />.
    /// </summary>
    /// <param name="release">The <see cref="UpdateRelease" /> who's asset7setup should be downloaded.</param>
    /// <param name="ct">Optional <see cref="CancellationToken" />.</param>
    /// <returns><see cref="HttpResponseMessage" /> on success.</returns>
    /// <exception cref="HttpRequestException">Server communication error occurred.</exception>
    public Task<HttpResponseMessage> DownloadReleaseAsync(UpdateRelease release, CancellationToken ct = default)
    {
        return _client.GetAsync(release.DownloadUrl, ct);
    }
}