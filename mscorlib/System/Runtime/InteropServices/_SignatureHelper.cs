namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection.Emit;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7D13DD37-5A04-393C-BBCA-A5FEA802893D"), ComVisible(true), CLSCompliant(false), TypeLibImportClass(typeof(SignatureHelper))]
    public interface _SignatureHelper
    {
        void GetTypeInfoCount(out uint pcTInfo);
        void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
        void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
        void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);
    }
}

