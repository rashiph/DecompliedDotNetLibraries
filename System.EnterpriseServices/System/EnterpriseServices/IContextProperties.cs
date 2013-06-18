namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [ComImport, Guid("D396DA85-BF8F-11d1-BBAE-00C04FC2FA5F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IContextProperties
    {
        int Count { get; }
        object GetProperty([In, MarshalAs(UnmanagedType.BStr)] string name);
        IEnumerator Enumerate { get; }
        void SetProperty([In, MarshalAs(UnmanagedType.BStr)] string name, [In, MarshalAs(UnmanagedType.Struct)] object value);
        void RemoveProperty([In, MarshalAs(UnmanagedType.BStr)] string name);
    }
}

