namespace System.Configuration.Install
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("1E233FE7-C16D-4512-8C3B-2E9988F08D38"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IManagedInstaller
    {
        [return: MarshalAs(UnmanagedType.I4)]
        int ManagedInstall([In, MarshalAs(UnmanagedType.BStr)] string commandLine, [In, MarshalAs(UnmanagedType.I4)] int hInstall);
    }
}

