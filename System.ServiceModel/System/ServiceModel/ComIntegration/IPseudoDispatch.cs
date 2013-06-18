namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("16BFA998-CA5B-4f29-B64F-123293EB159D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPseudoDispatch
    {
        void GetIDsOfNames(uint cNames, [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] rgszNames, IntPtr pDispID);
        [PreserveSig]
        int Invoke(uint dispIdMember, uint cArgs, uint cNamedArgs, IntPtr rgvarg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rgdispidNamedArgs, IntPtr pVarResult, IntPtr pExcepInfo, out uint pArgErr);
    }
}

