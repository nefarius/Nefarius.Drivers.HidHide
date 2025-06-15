# HidHideControlService

Namespace: Nefarius.Drivers.HidHide

Provides a managed wrapper for communicating with HidHide driver.

```csharp
public sealed class HidHideControlService : IHidHideControlService
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [HidHideControlService](./nefarius.drivers.hidhide.hidhidecontrolservice.md)<br>
Implements [IHidHideControlService](./nefarius.drivers.hidhide.ihidhidecontrolservice.md)

## Properties

### <a id="properties-applicationpaths"/>**ApplicationPaths**

```csharp
public IReadOnlyList<String> ApplicationPaths { get; }
```

#### Property Value

[IReadOnlyList&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### <a id="properties-blockedinstanceids"/>**BlockedInstanceIds**

```csharp
public IReadOnlyList<String> BlockedInstanceIds { get; }
```

#### Property Value

[IReadOnlyList&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### <a id="properties-deviceinterface"/>**DeviceInterface**

Interface GUID to enumerate HidHide devices.

```csharp
public static Guid DeviceInterface { get; }
```

#### Property Value

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

### <a id="properties-hardwareid"/>**HardwareId**

Hardware ID of the root-enumerated software node the driver attaches to.

```csharp
public static string HardwareId { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### <a id="properties-isactive"/>**IsActive**

```csharp
public bool IsActive { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### <a id="properties-isapplistinverted"/>**IsAppListInverted**

```csharp
public bool IsAppListInverted { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### <a id="properties-isdrivernodepresent"/>**IsDriverNodePresent**

```csharp
public bool IsDriverNodePresent { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### <a id="properties-isinstalled"/>**IsInstalled**

```csharp
public bool IsInstalled { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### <a id="properties-isoperational"/>**IsOperational**

```csharp
public bool IsOperational { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### <a id="properties-localdriverversion"/>**LocalDriverVersion**

```csharp
public Version LocalDriverVersion { get; }
```

#### Property Value

[Version](https://docs.microsoft.com/en-us/dotnet/api/system.version)<br>

## Constructors

### <a id="constructors-.ctor"/>**HidHideControlService(ILoggerFactory)**

Creates a new instance of [HidHideControlService](./nefarius.drivers.hidhide.hidhidecontrolservice.md) that is DI-aware.

```csharp
public HidHideControlService(ILoggerFactory loggerFactory)
```

#### Parameters

`loggerFactory` ILoggerFactory<br>
Injects a logging factory.

### <a id="constructors-.ctor"/>**HidHideControlService()**

Creates a new instance of [HidHideControlService](./nefarius.drivers.hidhide.hidhidecontrolservice.md) that is not DI-aware.

```csharp
public HidHideControlService()
```

**Remarks:**

If the caller uses a dependency injection framework, do not instantiate this class directly. Use
 [ServiceCollectionExtensions.AddHidHide(IServiceCollection, Action&lt;HidHideServiceOptions&gt;, Action&lt;IHttpClientBuilder&gt;)](./nefarius.drivers.hidhide.servicecollectionextensions.md#addhidhideiservicecollection-actionhidhideserviceoptions-actionihttpclientbuilder) instead.

## Methods

### <a id="methods-addapplicationpath"/>**AddApplicationPath(String, Boolean)**

```csharp
public void AddApplicationPath(string path, bool throwIfInvalid)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`throwIfInvalid` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### <a id="methods-addblockedinstanceid"/>**AddBlockedInstanceId(String)**

```csharp
public void AddBlockedInstanceId(string instanceId)
```

#### Parameters

`instanceId` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### <a id="methods-clearapplicationslist"/>**ClearApplicationsList()**

```csharp
public void ClearApplicationsList()
```

### <a id="methods-clearblockedinstanceslist"/>**ClearBlockedInstancesList()**

```csharp
public void ClearBlockedInstancesList()
```

### <a id="methods-removeapplicationpath"/>**RemoveApplicationPath(String)**

```csharp
public void RemoveApplicationPath(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### <a id="methods-removeblockedinstanceid"/>**RemoveBlockedInstanceId(String)**

```csharp
public void RemoveBlockedInstanceId(string instanceId)
```

#### Parameters

`instanceId` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
