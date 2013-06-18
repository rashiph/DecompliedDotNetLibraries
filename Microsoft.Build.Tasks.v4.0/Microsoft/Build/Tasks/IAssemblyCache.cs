namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("E707DCDE-D1CD-11D2-BAB9-00C04F8ECEAE")]
    internal interface IAssemblyCache
    {
        int UninstallAssembly();
        [PreserveSig]
        uint QueryAssemblyInfo(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, ref ASSEMBLY_INFO pAsmInfo);
        int CreateAssemblyCacheItem();
        int CreateAssemblyScavenger();
        int InstallAssembly();
    }
}

