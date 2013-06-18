namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("41C4F8B2-7439-11D2-98CB-00C04F8EE1C4"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface IObjectConstructString
    {
        string ConstructString { [return: MarshalAs(UnmanagedType.BStr)] get; }
    }
}

