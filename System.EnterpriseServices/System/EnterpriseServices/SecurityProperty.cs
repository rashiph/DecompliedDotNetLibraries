namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("E74A7215-014D-11D1-A63C-00A0C911B4E0")]
    internal interface SecurityProperty
    {
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetDirectCallerName();
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetDirectCreatorName();
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetOriginalCallerName();
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetOriginalCreatorName();
    }
}

