namespace Microsoft.Win32
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("21b8916c-f28e-11d2-a473-00c04f8ef448"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAssemblyEnum
    {
        [PreserveSig]
        int GetNextAssembly(out IApplicationContext ppAppCtx, out IAssemblyName ppName, uint dwFlags);
        [PreserveSig]
        int Reset();
        [PreserveSig]
        int Clone(out IAssemblyEnum ppEnum);
    }
}

