# ServiceCollectionExtensions

Namespace: Nefarius.Drivers.HidHide

Service collection extension methods.

```csharp
public static class ServiceCollectionExtensions
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ServiceCollectionExtensions](./nefarius.drivers.hidhide.servicecollectionextensions.md)

## Methods

### <a id="methods-addhidhide"/>**AddHidHide(IServiceCollection, Action&lt;HidHideServiceOptions&gt;, Action&lt;IHttpClientBuilder&gt;)**

Registers [HidHideControlService](./nefarius.drivers.hidhide.hidhidecontrolservice.md) and [HidHideSetupProvider](./nefarius.drivers.hidhide.hidhidesetupprovider.md) with DI.

```csharp
public static IServiceCollection AddHidHide(IServiceCollection services, Action<HidHideServiceOptions> options, Action<IHttpClientBuilder> builder)
```

#### Parameters

`services` IServiceCollection<br>
The  to modify.

`options` [Action&lt;HidHideServiceOptions&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.action-1)<br>
Optional options to customize the registered HidHide services.

`builder` [Action&lt;IHttpClientBuilder&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.action-1)<br>
Optional  to e.g. add resiliency policies or further customize
 the named HTTP client.

#### Returns

IServiceCollection
