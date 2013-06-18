namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [ComImport, Guid("CAFC823E-B441-11D1-B82B-0000F8757E2A")]
    internal interface ISecurityCallContext
    {
        int Count { [DispId(0x60020005)] get; }
        [DispId(0)]
        object GetItem([In, MarshalAs(UnmanagedType.BStr)] string name);
        [DispId(-4)]
        void GetEnumerator(out IEnumerator pEnum);
        [DispId(0x60020006)]
        bool IsCallerInRole([In, MarshalAs(UnmanagedType.BStr)] string role);
        [DispId(0x60020007)]
        bool IsSecurityEnabled();
        [DispId(0x60020008)]
        bool IsUserInRole([In, MarshalAs(UnmanagedType.Struct)] ref object pUser, [In, MarshalAs(UnmanagedType.BStr)] string role);
    }
}

