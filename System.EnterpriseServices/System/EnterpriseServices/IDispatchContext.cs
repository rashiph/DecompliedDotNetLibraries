namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("74C08646-CEDB-11CF-8B49-00AA00B8A790")]
    internal interface IDispatchContext
    {
        void CreateInstance([In, MarshalAs(UnmanagedType.BStr)] string bstrProgID, out object pObject);
        void SetComplete();
        void SetAbort();
        void EnableCommit();
        void DisableCommit();
        bool IsInTransaction();
        bool IsSecurityEnabled();
        bool IsCallerInRole([In, MarshalAs(UnmanagedType.BStr)] string bstrRole);
        void Count(out int plCount);
        void Item([In, MarshalAs(UnmanagedType.BStr)] string name, out object pItem);
        void _NewEnum([MarshalAs(UnmanagedType.Interface)] out object ppEnum);
        [return: MarshalAs(UnmanagedType.Interface)]
        object Security();
        [return: MarshalAs(UnmanagedType.Interface)]
        object ContextInfo();
    }
}

