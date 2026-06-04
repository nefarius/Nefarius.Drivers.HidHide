# ServiceCollectionExtensions

Namespace: Nefarius.Drivers.HidHide

Service collection extension methods.

```csharp
public static class ServiceCollectionExtensions
```

Inheritance [Object](https://learn.microsoft.com/dotnet/api/system.object) → [ServiceCollectionExtensions](./nefarius.drivers.hidhide.servicecollectionextensions.md)<br>
Attributes [ExtensionAttribute](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.extensionattribute)

## Methods

### <a id="methods-addhidhide"/>**AddHidHide(IServiceCollection, Action&lt;HidHideServiceOptions&gt;, Action&lt;IHttpClientBuilder&gt;)**

Registers [HidHideControlService](./nefarius.drivers.hidhide.hidhidecontrolservice.md) and [HidHideSetupProvider](./nefarius.drivers.hidhide.hidhidesetupprovider.md) with DI.

```csharp
public static IServiceCollection AddHidHide(IServiceCollection services, Action<HidHideServiceOptions> options, Action<IHttpClientBuilder> builder)
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)<br>
The [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) to modify.

`options` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[HidHideServiceOptions](./nefarius.drivers.hidhide.hidhideserviceoptions.md)><br>
Optional options to customize the registered HidHide services.

`builder` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[IHttpClientBuilder](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.ihttpclientbuilder)><br>
Optional [IHttpClientBuilder](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.ihttpclientbuilder) to e.g., add resiliency policies or further customize
 the named HTTP client.

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)
