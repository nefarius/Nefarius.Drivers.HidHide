# HidHideSetupProvider

Namespace: Nefarius.Drivers.HidHide

Service to locate the latest HidHide setup resources, update information etc.

```csharp
public sealed class HidHideSetupProvider
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [HidHideSetupProvider](./nefarius.drivers.hidhide.hidhidesetupprovider.md)

## Constructors

### <a id="constructors-.ctor"/>**HidHideSetupProvider(HttpClient)**

Creates new instance of HidHide Setup Provider.

```csharp
public HidHideSetupProvider(HttpClient client)
```

#### Parameters

`client` HttpClient<br>

## Methods

### <a id="methods-getlatestdownloadurlasync"/>**GetLatestDownloadUrlAsync(CancellationToken)**

Fetches the latest setup download URL.

```csharp
public Task<Uri> GetLatestDownloadUrlAsync(CancellationToken ct)
```

#### Parameters

`ct` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task&lt;Uri&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1)

#### Exceptions

[UpdateResponseMissingException](./nefarius.drivers.hidhide.updateresponsemissingexception.md)<br>
Server didn't respond with a proper reply, see
 [Exception.InnerException](https://docs.microsoft.com/en-us/dotnet/api/system.exception.innerexception) for details.

[MissingReleasesException](./nefarius.drivers.hidhide.missingreleasesexception.md)<br>
Mandatory releases collection was empty.

[DownloadLocationMissingException](./nefarius.drivers.hidhide.downloadlocationmissingexception.md)<br>
Mandatory release download location was missing.

[MalformedUrlException](./nefarius.drivers.hidhide.malformedurlexception.md)<br>
Provided download URL was malformed.

### <a id="methods-getlatestreleaseasync"/>**GetLatestReleaseAsync(CancellationToken)**

Fetches the latest available release.

```csharp
public Task<UpdateRelease> GetLatestReleaseAsync(CancellationToken ct)
```

#### Parameters

`ct` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>
Optional [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken).

#### Returns

The latest  available.

#### Exceptions

[UpdateResponseMissingException](./nefarius.drivers.hidhide.updateresponsemissingexception.md)<br>
Server didn't respond with a proper reply, see
 [Exception.InnerException](https://docs.microsoft.com/en-us/dotnet/api/system.exception.innerexception) for details.

[MissingReleasesException](./nefarius.drivers.hidhide.missingreleasesexception.md)<br>
Mandatory releases collection was empty.

### <a id="methods-getlatestversionasync"/>**GetLatestVersionAsync(CancellationToken)**

Fetches the latest available version.

```csharp
public Task<Version> GetLatestVersionAsync(CancellationToken ct)
```

#### Parameters

`ct` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task&lt;Version&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1)

#### Exceptions

[UpdateResponseMissingException](./nefarius.drivers.hidhide.updateresponsemissingexception.md)<br>
Server didn't respond with a proper reply, see
 [Exception.InnerException](https://docs.microsoft.com/en-us/dotnet/api/system.exception.innerexception) for details.

[MissingReleasesException](./nefarius.drivers.hidhide.missingreleasesexception.md)<br>
Mandatory releases collection was empty.

### <a id="methods-getupdateinformationasync"/>**GetUpdateInformationAsync(CancellationToken)**

Fetches the  from the HidHide CDN server.

```csharp
public Task<UpdateResponse> GetUpdateInformationAsync(CancellationToken ct)
```

#### Parameters

`ct` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task&lt;UpdateResponse&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1)

#### Exceptions

[OperationCanceledException](https://docs.microsoft.com/en-us/dotnet/api/system.operationcanceledexception)<br>
The cancellation token was canceled. This exception is stored into the
 returned task.
