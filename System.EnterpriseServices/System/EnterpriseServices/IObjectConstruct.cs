namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("41C4F8B3-7439-11D2-98CB-00C04F8EE1C4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IObjectConstruct
    {
        void Construct([In, MarshalAs(UnmanagedType.Interface)] object obj);
    }
}

