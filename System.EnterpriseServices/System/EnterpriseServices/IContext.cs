namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("000001c0-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IContext
    {
        void SetProperty([In, MarshalAs(UnmanagedType.LPStruct)] Guid policyId, [In] int flags, [In, MarshalAs(UnmanagedType.Interface)] object punk);
        void RemoveProperty([In, MarshalAs(UnmanagedType.LPStruct)] Guid policyId);
        void GetProperty([In, MarshalAs(UnmanagedType.LPStruct)] Guid policyId, out int flags, [MarshalAs(UnmanagedType.Interface)] out object pUnk);
    }
}

