namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;

    [ComVisible(true), CLSCompliant(false), Guid("B42B6AAC-317E-34D5-9FA9-093BB4160C50"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), TypeLibImportClass(typeof(AssemblyName))]
    public interface _AssemblyName
    {
        void GetTypeInfoCount(out uint pcTInfo);
        void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
        void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
        void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
    }
}

