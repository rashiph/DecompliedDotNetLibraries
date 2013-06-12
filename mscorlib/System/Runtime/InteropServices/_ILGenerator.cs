namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection.Emit;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), TypeLibImportClass(typeof(ILGenerator)), Guid("A4924B27-6E3B-37F7-9B83-A4501955E6A7"), ComVisible(true), CLSCompliant(false)]
    public interface _ILGenerator
    {
        void GetTypeInfoCount(out uint pcTInfo);
        void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
        void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
        void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
    }
}

