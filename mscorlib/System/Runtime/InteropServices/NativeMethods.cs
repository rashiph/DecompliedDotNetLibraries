namespace System.Runtime.InteropServices
{
    using System;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;

    internal static class NativeMethods
    {
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("oleaut32.dll", PreserveSig=false)]
        internal static extern void VariantClear(IntPtr variant);

        [ComImport, SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020400-0000-0000-C000-000000000046")]
        internal interface IDispatch
        {
            [SecurityCritical]
            void GetTypeInfoCount(out uint pctinfo);
            [SecurityCritical]
            void GetTypeInfo(uint iTInfo, int lcid, out IntPtr info);
            [SecurityCritical]
            void GetIDsOfNames(ref Guid iid, [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=2)] string[] names, uint cNames, int lcid, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.I4, SizeParamIndex=2)] int[] rgDispId);
            [SecurityCritical]
            void Invoke(int dispIdMember, ref Guid riid, int lcid, System.Runtime.InteropServices.ComTypes.INVOKEKIND wFlags, ref System.Runtime.InteropServices.ComTypes.DISPPARAMS pDispParams, IntPtr pvarResult, IntPtr pExcepInfo, IntPtr puArgErr);
        }
    }
}

