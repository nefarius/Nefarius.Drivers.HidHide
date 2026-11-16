using System.Runtime.InteropServices;

using Nefarius.Drivers.HidHide.Util;

namespace Tests;

internal sealed class MultiSzHelperTests
{
    [Test]
    [Category("Unit")]
    public void RoundTrip_EmptyList_YieldsNoStrings()
    {
        string[] result = RoundTrip([]);

        Assert.That(result, Is.Empty);
    }

    [Test]
    [Category("Unit")]
    public void RoundTrip_SingleString()
    {
        string[] result = RoundTrip(["HidHide"]);

        Assert.That(result, Is.EqualTo(new[] { "HidHide" }));
    }

    [Test]
    [Category("Unit")]
    public void RoundTrip_MultipleStrings_KeepsOrder()
    {
        string[] result = RoundTrip(["one", "two", "three"]);

        Assert.That(result, Is.EqualTo(new[] { "one", "two", "three" }));
    }

    [Test]
    [Category("Unit")]
    public void RoundTrip_UnicodeStrings()
    {
        string[] result = RoundTrip(["Gerät", "デバイス", "устройство"]);

        Assert.That(result, Is.EqualTo(new[] { "Gerät", "デバイス", "устройство" }));
    }

    [Test]
    [Category("Unit")]
    public void Conversion_SkipsBlankEntries()
    {
        string[] result = RoundTrip(["keep", "", "   ", "also"]);

        Assert.That(result, Is.EqualTo(new[] { "keep", "also" }));
    }

    [Test]
    [Category("Unit")]
    public void Conversion_ReportsBufferLengthIncludingTerminators()
    {
        IntPtr buffer = new[] { "ab" }.StringArrayToMultiSzPointer(out int length);

        try
        {
            // "ab" + separator + final terminator, UTF-16, plus the extra terminator the helper appends
            Assert.That(length, Is.EqualTo(4 * sizeof(char)));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static string[] RoundTrip(IEnumerable<string> values)
    {
        IntPtr buffer = values.StringArrayToMultiSzPointer(out int length);

        try
        {
            return buffer.MultiSzPointerToStringArray(length).ToArray();
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
