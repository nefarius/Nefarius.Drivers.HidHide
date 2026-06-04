# MultiSzHelper

Namespace: Nefarius.Drivers.HidHide.Util

String manipulation helper methods.

```csharp
public static class MultiSzHelper
```

Inheritance [Object](https://learn.microsoft.com/dotnet/api/system.object) → [MultiSzHelper](./nefarius.drivers.hidhide.util.multiszhelper.md)<br>
Attributes [ExtensionAttribute](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.extensionattribute)

## Methods

### <a id="methods-multiszpointertostringarray"/>**MultiSzPointerToStringArray(IntPtr, Int32)**

Converts a double-null-terminated multi-byte character memory block into a string array.

```csharp
public static IEnumerable<String> MultiSzPointerToStringArray(IntPtr buffer, int length)
```

#### Parameters

`buffer` [IntPtr](https://learn.microsoft.com/dotnet/api/system.intptr)<br>
The memory buffer.

`length` [Int32](https://learn.microsoft.com/dotnet/api/system.int32)<br>
The size in bytes of the memory buffer.

#### Returns

The extracted string array.

### <a id="methods-stringarraytomultiszpointer"/>**StringArrayToMultiSzPointer(IEnumerable&lt;String&gt;, ref Int32)**

Converts an array of [String](https://learn.microsoft.com/dotnet/api/system.string) into a double-null-terminated multi-byte character memory block.

```csharp
public static IntPtr StringArrayToMultiSzPointer(IEnumerable<String> instances, ref Int32 length)
```

#### Parameters

`instances` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)<[String](https://learn.microsoft.com/dotnet/api/system.string)><br>
Source array of strings.

`length` [Int32&](https://learn.microsoft.com/dotnet/api/system.int32&)<br>
The length of the resulting byte array.

#### Returns

The allocated memory buffer.
