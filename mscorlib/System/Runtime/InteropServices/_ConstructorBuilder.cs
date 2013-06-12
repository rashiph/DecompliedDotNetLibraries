namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection.Emit;

    [Guid("ED3E4384-D7E2-3FA7-8FFD-8940D330519A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), CLSCompliant(false), ComVisible(true), TypeLibImportClass(typeof(ConstructorBuilder))]
    public interface _ConstructorBuilder
    {
        void GetTypeInfoCount(out uint pcTInfo);
        void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
        void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
        void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
    }
}

