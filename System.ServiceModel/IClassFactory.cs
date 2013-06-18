using System;
using System.Runtime.InteropServices;

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(false), Guid("00000001-0000-0000-C000-000000000046")]
internal interface IClassFactory
{
    [return: MarshalAs(UnmanagedType.Interface)]
    object CreateInstance([In, MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);
    void LockServer([In, MarshalAs(UnmanagedType.Bool)] bool fLock);
}

