# HidHideSetupProvider

Namespace: Nefarius.Drivers.HidHide

Service to locate the latest HidHide setup resources, update information etc.

```csharp
public sealed class HidHideSetupProvider
```

Inheritance [Object](https://learn.microsoft.com/dotnet/api/system.object) → [HidHideSetupProvider](./nefarius.drivers.hidhide.hidhidesetupprovider.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md)

## Constructors

### <a id="constructors-.ctor"/>**HidHideSetupProvider(HttpClient)**

Creates new instance of HidHide Setup Provider.

```csharp
public HidHideSetupProvider(HttpClient client)
```

#### Parameters

`client` [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)<br>

## Methods

### <a id="methods-downloadlatestreleaseasync"/>**DownloadLatestReleaseAsync(CancellationToken)**

Downloads the setup asset of the most recent UpdateRelease.

```csharp
public Task<HttpResponseMessage> DownloadLatestReleaseAsync(CancellationToken ct)
```

#### Parameters

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)<br>
Optional [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken).

#### Returns

A [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage).

#### Exceptions

[UpdateResponseMissingException](./nefarius.drivers.hidhide.exceptions.updateresponsemissingexception.md)<br>
Server didn't respond with a proper reply, see
 [Exception.InnerException](https://learn.microsoft.com/dotnet/api/system.exception.innerexception) for details.

[MissingReleasesException](./nefarius.drivers.hidhide.exceptions.missingreleasesexception.md)<br>
Mandatory releases collection was empty.

[HttpRequestException](https://learn.microsoft.com/dotnet/api/system.net.http.httprequestexception)<br>
Server communication error occurred.

### <a id="methods-downloadreleaseasync"/>**DownloadReleaseAsync(UpdateRelease, CancellationToken)**

Downloads the setup asset of the provided UpdateRelease.

```csharp
public Task<HttpResponseMessage> DownloadReleaseAsync(UpdateRelease release, CancellationToken ct)
```

#### Parameters

`release` UpdateRelease<br>
The UpdateRelease who's asset setup should be downloaded.

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)<br>
Optional [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken).

#### Returns

[HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) on success.

#### Exceptions

[HttpRequestException](https://learn.microsoft.com/dotnet/api/system.net.http.httprequestexception)<br>
Server communication error occurred.

### <a id="methods-getlatestdownloadurlasync"/>**GetLatestDownloadUrlAsync(CancellationToken)**

Fetches the latest setup download URL.

```csharp
public Task<Uri> GetLatestDownloadUrlAsync(CancellationToken ct)
```

#### Parameters

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)<[Uri](https://learn.microsoft.com/dotnet/api/system.uri)>

#### Exceptions

[UpdateResponseMissingException](./nefarius.drivers.hidhide.exceptions.updateresponsemissingexception.md)<br>
Server didn't respond with a proper reply, see
 [Exception.InnerException](https://learn.microsoft.com/dotnet/api/system.exception.innerexception) for details.

[MissingReleasesException](./nefarius.drivers.hidhide.exceptions.missingreleasesexception.md)<br>
Mandatory releases collection was empty.

[DownloadLocationMissingException](./nefarius.drivers.hidhide.exceptions.downloadlocationmissingexception.md)<br>
Mandatory release download location was missing.

[MalformedUrlException](./nefarius.drivers.hidhide.exceptions.malformedurlexception.md)<br>
Provided download URL was malformed.

### <a id="methods-getlatestreleaseasync"/>**GetLatestReleaseAsync(CancellationToken)**

Fetches the latest available release.

```csharp
public Task<UpdateRelease> GetLatestReleaseAsync(CancellationToken ct)
```

#### Parameters

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)<br>
Optional [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken).

#### Returns

The latest UpdateRelease available.

#### Exceptions

[UpdateResponseMissingException](./nefarius.drivers.hidhide.exceptions.updateresponsemissingexception.md)<br>
Server didn't respond with a proper reply, see
 [Exception.InnerException](https://learn.microsoft.com/dotnet/api/system.exception.innerexception) for details.

[MissingReleasesException](./nefarius.drivers.hidhide.exceptions.missingreleasesexception.md)<br>
Mandatory releases collection was empty.

### <a id="methods-getlatestversionasync"/>**GetLatestVersionAsync(CancellationToken)**

Fetches the latest available version.

```csharp
public Task<Version> GetLatestVersionAsync(CancellationToken ct)
```

#### Parameters

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)<[Version](https://learn.microsoft.com/dotnet/api/system.version)>

#### Exceptions

[UpdateResponseMissingException](./nefarius.drivers.hidhide.exceptions.updateresponsemissingexception.md)<br>
Server didn't respond with a proper reply, see
 [Exception.InnerException](https://learn.microsoft.com/dotnet/api/system.exception.innerexception) for details.

[MissingReleasesException](./nefarius.drivers.hidhide.exceptions.missingreleasesexception.md)<br>
Mandatory releases collection was empty.

### <a id="methods-getupdateinformationasync"/>**GetUpdateInformationAsync(CancellationToken)**

Fetches the UpdateResponse from the HidHide CDN server.

```csharp
public Task<UpdateResponse> GetUpdateInformationAsync(CancellationToken ct)
```

#### Parameters

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)<UpdateResponse>

#### Exceptions

[OperationCanceledException](https://learn.microsoft.com/dotnet/api/system.operationcanceledexception)<br>
The cancellation token was canceled. This exception is stored into the
 returned task.
