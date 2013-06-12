namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection.Emit;

    [TypeLibImportClass(typeof(FieldBuilder)), ComVisible(true), Guid("CE1A3BF5-975E-30CC-97C9-1EF70F8F3993"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), CLSCompliant(false)]
    public interface _FieldBuilder
    {
        void GetTypeInfoCount(out uint pcTInfo);
        void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
        void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
        void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
    }
}

