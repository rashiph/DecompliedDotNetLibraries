namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection.Emit;

    [Guid("4E6350D1-A08B-3DEC-9A3E-C465F9AEEC0C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), TypeLibImportClass(typeof(LocalBuilder)), ComVisible(true), CLSCompliant(false)]
    public interface _LocalBuilder
    {
        void GetTypeInfoCount(out uint pcTInfo);
        void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
        void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
        void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
    }
}

