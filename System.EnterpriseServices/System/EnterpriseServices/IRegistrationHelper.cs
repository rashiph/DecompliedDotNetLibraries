namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("55e3ea25-55cb-4650-8887-18e8d30bb4bc"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IRegistrationHelper
    {
        void InstallAssembly([In, MarshalAs(UnmanagedType.BStr)] string assembly, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string application, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string tlb, [In] InstallationFlags installFlags);
        void UninstallAssembly([In, MarshalAs(UnmanagedType.BStr)] string assembly, [In, MarshalAs(UnmanagedType.BStr)] string application);
    }
}

