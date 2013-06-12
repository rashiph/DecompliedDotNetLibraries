namespace System.Runtime.InteropServices
{
    using System;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("917B14D0-2D9E-38B8-92A9-381ACF52F7C0"), CLSCompliant(false), TypeLibImportClass(typeof(Attribute)), ComVisible(true)]
    public interface _Attribute
    {
        void GetTypeInfoCount(out uint pcTInfo);
        void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
        void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
        void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
    }
}

