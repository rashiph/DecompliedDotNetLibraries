namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("2A005C07-A5DE-11CF-9E66-00AA00A3F464")]
    internal interface ISharedPropertyGroup
    {
        ISharedProperty CreatePropertyByPosition([In, MarshalAs(UnmanagedType.I4)] int position, out bool fExists);
        ISharedProperty PropertyByPosition(int position);
        ISharedProperty CreateProperty([In, MarshalAs(UnmanagedType.BStr)] string name, out bool fExists);
        ISharedProperty Property([In, MarshalAs(UnmanagedType.BStr)] string name);
    }
}

