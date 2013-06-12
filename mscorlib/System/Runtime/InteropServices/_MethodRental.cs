namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection.Emit;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true), Guid("C2323C25-F57F-3880-8A4D-12EBEA7A5852"), CLSCompliant(false), TypeLibImportClass(typeof(MethodRental))]
    public interface _MethodRental
    {
        void GetTypeInfoCount(out uint pcTInfo);
        void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
        void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
        void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
    }
}

