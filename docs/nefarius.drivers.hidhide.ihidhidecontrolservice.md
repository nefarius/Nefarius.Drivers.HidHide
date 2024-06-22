# IHidHideControlService

Namespace: Nefarius.Drivers.HidHide

Provides a managed wrapper for communicating with HidHide driver.

```csharp
public interface IHidHideControlService
```

## Properties

### <a id="properties-applicationpaths"/>**ApplicationPaths**

Returns list of currently allowed (or blocked, see [IHidHideControlService.IsAppListInverted](./nefarius.drivers.hidhide.ihidhidecontrolservice.md#isapplistinverted)) application paths.

```csharp
public abstract IReadOnlyList<String> ApplicationPaths { get; }
```

#### Property Value

[IReadOnlyList&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

#### Exceptions

[HidHideDriverAccessFailedException](./nefarius.drivers.hidhide.hidhidedriveraccessfailedexception.md)<br>
Failed to open handle to driver. Make sure no other process is
 using the API at the same time.

[HidHideDriverNotFoundException](./nefarius.drivers.hidhide.hidhidedrivernotfoundexception.md)<br>
Failed to locate driver. Make sure HidHide is installed and not in a
 faulty state.

[HidHideRequestFailedException](./nefarius.drivers.hidhide.hidhiderequestfailedexception.md)<br>
Driver communication has failed. See
 [HidHideException.NativeErrorCode](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrorcode) and [HidHideException.NativeErrorMessage](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrormessage) for details.

[HidHideBufferOverflowException](./nefarius.drivers.hidhide.hidhidebufferoverflowexception.md)<br>
Buffer size exceeded maximum allowed characters. Happens when list
 grew out of supported bounds.

### <a id="properties-blockedinstanceids"/>**BlockedInstanceIds**

Returns list of currently blocked instance IDs.

```csharp
public abstract IReadOnlyList<String> BlockedInstanceIds { get; }
```

#### Property Value

[IReadOnlyList&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

#### Exceptions

[HidHideDriverAccessFailedException](./nefarius.drivers.hidhide.hidhidedriveraccessfailedexception.md)<br>
Failed to open handle to driver. Make sure no other process is
 using the API at the same time.

[HidHideDriverNotFoundException](./nefarius.drivers.hidhide.hidhidedrivernotfoundexception.md)<br>
Failed to locate driver. Make sure HidHide is installed and not in a
 faulty state.

[HidHideRequestFailedException](./nefarius.drivers.hidhide.hidhiderequestfailedexception.md)<br>
Driver communication has failed. See
 [HidHideException.NativeErrorCode](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrorcode) and [HidHideException.NativeErrorMessage](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrormessage) for details.

[HidHideBufferOverflowException](./nefarius.drivers.hidhide.hidhidebufferoverflowexception.md)<br>
Buffer size exceeded maximum allowed characters. Happens when list
 grew out of supported bounds.

### <a id="properties-isactive"/>**IsActive**

Gets or sets whether global device hiding is currently active or not.

```csharp
public abstract bool IsActive { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Exceptions

[HidHideRequestFailedException](./nefarius.drivers.hidhide.hidhiderequestfailedexception.md)<br>
Driver communication has failed. See
 [HidHideException.NativeErrorCode](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrorcode) and [HidHideException.NativeErrorMessage](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrormessage) for details.

### <a id="properties-isapplistinverted"/>**IsAppListInverted**

Gets or sets whether the application list is inverted (from block all/allow specific to allow all/block specific).

```csharp
public abstract bool IsAppListInverted { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Exceptions

[HidHideRequestFailedException](./nefarius.drivers.hidhide.hidhiderequestfailedexception.md)<br>
Driver communication has failed. See
 [HidHideException.NativeErrorCode](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrorcode) and [HidHideException.NativeErrorMessage](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrormessage) for details.

**Remarks:**

The default behaviour of the application list is to block all processes by default and only treat listed paths
 as exempted.

### <a id="properties-isdrivernodepresent"/>**IsDriverNodePresent**

Gets whether the virtual root-enumerated software device the driver attaches to is present on the system.

```csharp
public abstract bool IsDriverNodePresent { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Exceptions

T:Nefarius.Utilities.DeviceManagement.Exceptions.ConfigManagerException<br>
An unexpected enumeration error occurred.

### <a id="properties-isinstalled"/>**IsInstalled**

Gets whether the driver is present and operable.

```csharp
public abstract bool IsInstalled { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Exceptions

[HidHideDetectionFailedException](./nefarius.drivers.hidhide.hidhidedetectionfailedexception.md)<br>
Driver lookup has failed. See [HidHideException.NativeErrorCode](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrorcode) and
 [HidHideException.NativeErrorMessage](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrormessage) for details.

### <a id="properties-isoperational"/>**IsOperational**

Gets whether the driver node is present and operational (has its device interface exposed).

```csharp
public abstract bool IsOperational { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Exceptions

[HidHideDetectionFailedException](./nefarius.drivers.hidhide.hidhidedetectionfailedexception.md)<br>
Driver lookup has failed. See [HidHideException.NativeErrorCode](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrorcode) and
 [HidHideException.NativeErrorMessage](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrormessage) for details.

T:Nefarius.Utilities.DeviceManagement.Exceptions.ConfigManagerException<br>
An unexpected enumeration error occurred.

### <a id="properties-localdriverversion"/>**LocalDriverVersion**

Gets the local driver binary version.

```csharp
public abstract Version LocalDriverVersion { get; }
```

#### Property Value

[Version](https://docs.microsoft.com/en-us/dotnet/api/system.version)<br>

#### Exceptions

T:Nefarius.Utilities.DeviceManagement.Exceptions.ConfigManagerException<br>
An unexpected enumeration error occurred.

[HidHideDriverNotFoundException](./nefarius.drivers.hidhide.hidhidedrivernotfoundexception.md)<br>
Failed to locate driver. Make sure HidHide is installed and not in a
 faulty state.

## Methods

### <a id="methods-addapplicationpath"/>**AddApplicationPath(String)**

Submit a new application to allow (or deny if inverse flag is set).

```csharp
void AddApplicationPath(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The absolute application path to allow.

#### Exceptions

[HidHideDriverAccessFailedException](./nefarius.drivers.hidhide.hidhidedriveraccessfailedexception.md)<br>
Failed to open handle to driver. Make sure no other process is
 using the API at the same time.

[HidHideDriverNotFoundException](./nefarius.drivers.hidhide.hidhidedrivernotfoundexception.md)<br>
Failed to locate driver. Make sure HidHide is installed and not in a
 faulty state.

[HidHideRequestFailedException](./nefarius.drivers.hidhide.hidhiderequestfailedexception.md)<br>
Driver communication has failed. See
 [HidHideException.NativeErrorCode](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrorcode) and [HidHideException.NativeErrorMessage](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrormessage) for details.

[HidHideBufferOverflowException](./nefarius.drivers.hidhide.hidhidebufferoverflowexception.md)<br>
Buffer size exceeded maximum allowed characters. Happens when list
 grew out of supported bounds.

**Remarks:**

Use the common local path notation (e.g. "C:\Windows\System32\rundll32.exe").

### <a id="methods-addblockedinstanceid"/>**AddBlockedInstanceId(String)**

Submit a new instance to block.

```csharp
void AddBlockedInstanceId(string instanceId)
```

#### Parameters

`instanceId` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The Instance ID to block.

#### Exceptions

[HidHideDriverAccessFailedException](./nefarius.drivers.hidhide.hidhidedriveraccessfailedexception.md)<br>
Failed to open handle to driver. Make sure no other process is
 using the API at the same time.

[HidHideDriverNotFoundException](./nefarius.drivers.hidhide.hidhidedrivernotfoundexception.md)<br>
Failed to locate driver. Make sure HidHide is installed and not in a
 faulty state.

[HidHideRequestFailedException](./nefarius.drivers.hidhide.hidhiderequestfailedexception.md)<br>
Driver communication has failed. See
 [HidHideException.NativeErrorCode](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrorcode) and [HidHideException.NativeErrorMessage](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrormessage) for details.

[HidHideBufferOverflowException](./nefarius.drivers.hidhide.hidhidebufferoverflowexception.md)<br>
Buffer size exceeded maximum allowed characters. Happens when list
 grew out of supported bounds.

**Remarks:**

To get the instance ID from e.g. a symbolic link (device path) you can use this companion library:
 https://github.com/nefarius/Nefarius.Utilities.DeviceManagement

### <a id="methods-clearapplicationslist"/>**ClearApplicationsList()**

Empties the application list. Useful if [IHidHideControlService.AddApplicationPath(String)](./nefarius.drivers.hidhide.ihidhidecontrolservice.md#addapplicationpathstring) or [IHidHideControlService.ApplicationPaths](./nefarius.drivers.hidhide.ihidhidecontrolservice.md#applicationpaths) throw
 exceptions due to nonexistent entries.

```csharp
void ClearApplicationsList()
```

#### Exceptions

[HidHideDriverAccessFailedException](./nefarius.drivers.hidhide.hidhidedriveraccessfailedexception.md)<br>
Failed to open handle to driver. Make sure no other process is
 using the API at the same time.

[HidHideDriverNotFoundException](./nefarius.drivers.hidhide.hidhidedrivernotfoundexception.md)<br>
Failed to locate driver. Make sure HidHide is installed and not in a
 faulty state.

[HidHideRequestFailedException](./nefarius.drivers.hidhide.hidhiderequestfailedexception.md)<br>
Driver communication has failed. See
 [HidHideException.NativeErrorCode](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrorcode) and [HidHideException.NativeErrorMessage](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrormessage) for details.

[HidHideBufferOverflowException](./nefarius.drivers.hidhide.hidhidebufferoverflowexception.md)<br>
Buffer size exceeded maximum allowed characters. Happens when list
 grew out of supported bounds.

**Remarks:**

Be very conservative in using this call, you might accidentally undo settings different apps have put in
 place.

### <a id="methods-clearblockedinstanceslist"/>**ClearBlockedInstancesList()**

Empties the device instances list. Useful if [IHidHideControlService.AddBlockedInstanceId(String)](./nefarius.drivers.hidhide.ihidhidecontrolservice.md#addblockedinstanceidstring) or
 [IHidHideControlService.BlockedInstanceIds](./nefarius.drivers.hidhide.ihidhidecontrolservice.md#blockedinstanceids) throw exceptions due to nonexistent entries.

```csharp
void ClearBlockedInstancesList()
```

#### Exceptions

[HidHideDriverAccessFailedException](./nefarius.drivers.hidhide.hidhidedriveraccessfailedexception.md)<br>
Failed to open handle to driver. Make sure no other process is
 using the API at the same time.

[HidHideDriverNotFoundException](./nefarius.drivers.hidhide.hidhidedrivernotfoundexception.md)<br>
Failed to locate driver. Make sure HidHide is installed and not in a
 faulty state.

[HidHideRequestFailedException](./nefarius.drivers.hidhide.hidhiderequestfailedexception.md)<br>
Driver communication has failed. See
 [HidHideException.NativeErrorCode](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrorcode) and [HidHideException.NativeErrorMessage](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrormessage) for details.

[HidHideBufferOverflowException](./nefarius.drivers.hidhide.hidhidebufferoverflowexception.md)<br>
Buffer size exceeded maximum allowed characters. Happens when list
 grew out of supported bounds.

**Remarks:**

Be very conservative in using this call, you might accidentally undo settings different apps have put in
 place.

### <a id="methods-removeapplicationpath"/>**RemoveApplicationPath(String)**

Revokes an applications exemption.

```csharp
void RemoveApplicationPath(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The absolute application path to revoke.

#### Exceptions

[HidHideDriverAccessFailedException](./nefarius.drivers.hidhide.hidhidedriveraccessfailedexception.md)<br>
Failed to open handle to driver. Make sure no other process is
 using the API at the same time.

[HidHideDriverNotFoundException](./nefarius.drivers.hidhide.hidhidedrivernotfoundexception.md)<br>
Failed to locate driver. Make sure HidHide is installed and not in a
 faulty state.

[HidHideRequestFailedException](./nefarius.drivers.hidhide.hidhiderequestfailedexception.md)<br>
Driver communication has failed. See
 [HidHideException.NativeErrorCode](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrorcode) and [HidHideException.NativeErrorMessage](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrormessage) for details.

[HidHideBufferOverflowException](./nefarius.drivers.hidhide.hidhidebufferoverflowexception.md)<br>
Buffer size exceeded maximum allowed characters. Happens when list
 grew out of supported bounds.

**Remarks:**

Use the common local path notation (e.g. "C:\Windows\System32\rundll32.exe").

### <a id="methods-removeblockedinstanceid"/>**RemoveBlockedInstanceId(String)**

Remove an instance from being blocked.

```csharp
void RemoveBlockedInstanceId(string instanceId)
```

#### Parameters

`instanceId` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The Instance ID to unblock.

#### Exceptions

[HidHideDriverAccessFailedException](./nefarius.drivers.hidhide.hidhidedriveraccessfailedexception.md)<br>
Failed to open handle to driver. Make sure no other process is
 using the API at the same time.

[HidHideDriverNotFoundException](./nefarius.drivers.hidhide.hidhidedrivernotfoundexception.md)<br>
Failed to locate driver. Make sure HidHide is installed and not in a
 faulty state.

[HidHideRequestFailedException](./nefarius.drivers.hidhide.hidhiderequestfailedexception.md)<br>
Driver communication has failed. See
 [HidHideException.NativeErrorCode](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrorcode) and [HidHideException.NativeErrorMessage](./nefarius.drivers.hidhide.hidhideexception.md#nativeerrormessage) for details.

[HidHideBufferOverflowException](./nefarius.drivers.hidhide.hidhidebufferoverflowexception.md)<br>
Buffer size exceeded maximum allowed characters. Happens when list
 grew out of supported bounds.

**Remarks:**

To get the instance ID from e.g. a symbolic link (device path) you can use this companion library:
 https://github.com/nefarius/Nefarius.Utilities.DeviceManagement
