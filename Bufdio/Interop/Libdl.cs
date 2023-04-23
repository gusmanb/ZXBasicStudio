using System;
using System.Runtime.InteropServices;

namespace Bufdio.Interop;

internal static class Libdl
{
    private const string LibraryName = "libdl";

    [DllImport(LibraryName)]
    public static extern IntPtr dlopen(string fileName, int flags);

    [DllImport(LibraryName)]
    public static extern IntPtr dlsym(IntPtr handle, string symbol);

    [DllImport(LibraryName)]
    public static extern int dlclose(IntPtr handle);
}
