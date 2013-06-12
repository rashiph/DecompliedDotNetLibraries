namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("00000100-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumUnknown
    {
        [PreserveSig]
        int Next(uint celt, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.IUnknown)] object[] rgelt, ref uint celtFetched);
        [PreserveSig]
        int Skip(uint celt);
        [PreserveSig]
        int Reset();
        [PreserveSig]
        int Clone(out IEnumUnknown enumUnknown);
    }
}

