namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection.Emit;

    [ComVisible(true), TypeLibImportClass(typeof(MethodBuilder)), Guid("007D8A14-FDF3-363E-9A0B-FEC0618260A2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), CLSCompliant(false)]
    public interface _MethodBuilder
    {
        void GetTypeInfoCount(out uint pcTInfo);
        void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
        void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
        void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
    }
}

