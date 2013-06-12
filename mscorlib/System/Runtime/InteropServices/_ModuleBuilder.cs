namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection.Emit;

    [ComVisible(true), Guid("D05FFA9A-04AF-3519-8EE1-8D93AD73430B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), CLSCompliant(false), TypeLibImportClass(typeof(ModuleBuilder))]
    public interface _ModuleBuilder
    {
        void GetTypeInfoCount(out uint pcTInfo);
        void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
        void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
        void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
    }
}

