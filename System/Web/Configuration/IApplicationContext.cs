namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("7c23ff90-33af-11d3-95da-00a024a85b51"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IApplicationContext
    {
        void SetContextNameObject(IAssemblyName pName);
        void GetContextNameObject(out IAssemblyName ppName);
        void Set([MarshalAs(UnmanagedType.LPWStr)] string szName, int pvValue, uint cbValue, uint dwFlags);
        void Get([MarshalAs(UnmanagedType.LPWStr)] string szName, out int pvValue, ref uint pcbValue, uint dwFlags);
        void GetDynamicDirectory(out int wzDynamicDir, ref uint pdwSize);
    }
}

