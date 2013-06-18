namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("75B52DDB-E8ED-11D1-93AD-00AA00BA3258"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IObjectContextInfo
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsInTransaction();
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetTransaction();
        Guid GetTransactionId();
        Guid GetActivityId();
        Guid GetContextId();
    }
}

