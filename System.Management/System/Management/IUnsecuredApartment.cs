namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("1CFABA8C-1523-11D1-AD79-00C04FD8FDFF"), TypeLibType((short) 0x200), InterfaceType((short) 1)]
    internal interface IUnsecuredApartment
    {
        [PreserveSig]
        int CreateObjectStub_([In, MarshalAs(UnmanagedType.IUnknown)] object pObject, [MarshalAs(UnmanagedType.IUnknown)] out object ppStub);
    }
}

