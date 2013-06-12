namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("00020412-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITypeInfo2 : ITypeInfo
    {
        void GetTypeAttr(out IntPtr ppTypeAttr);
        void GetTypeComp(out ITypeComp ppTComp);
        void GetFuncDesc(int index, out IntPtr ppFuncDesc);
        void GetVarDesc(int index, out IntPtr ppVarDesc);
        void GetNames(int memid, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] string[] rgBstrNames, int cMaxNames, out int pcNames);
        void GetRefTypeOfImplType(int index, out int href);
        void GetImplTypeFlags(int index, out System.Runtime.InteropServices.ComTypes.IMPLTYPEFLAGS pImplTypeFlags);
        void GetIDsOfNames([In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=1)] string[] rgszNames, int cNames, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] int[] pMemId);
        void Invoke([MarshalAs(UnmanagedType.IUnknown)] object pvInstance, int memid, short wFlags, ref System.Runtime.InteropServices.ComTypes.DISPPARAMS pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, out int puArgErr);
        void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);
        void GetDllEntry(int memid, System.Runtime.InteropServices.ComTypes.INVOKEKIND invKind, IntPtr pBstrDllName, IntPtr pBstrName, IntPtr pwOrdinal);
        void GetRefTypeInfo(int hRef, out ITypeInfo ppTI);
        void AddressOfMember(int memid, System.Runtime.InteropServices.ComTypes.INVOKEKIND invKind, out IntPtr ppv);
        void CreateInstance([MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppvObj);
        void GetMops(int memid, out string pBstrMops);
        void GetContainingTypeLib(out ITypeLib ppTLB, out int pIndex);
        [PreserveSig]
        void ReleaseTypeAttr(IntPtr pTypeAttr);
        [PreserveSig]
        void ReleaseFuncDesc(IntPtr pFuncDesc);
        [PreserveSig]
        void ReleaseVarDesc(IntPtr pVarDesc);
        void GetTypeKind(out System.Runtime.InteropServices.ComTypes.TYPEKIND pTypeKind);
        void GetTypeFlags(out int pTypeFlags);
        void GetFuncIndexOfMemId(int memid, System.Runtime.InteropServices.ComTypes.INVOKEKIND invKind, out int pFuncIndex);
        void GetVarIndexOfMemId(int memid, out int pVarIndex);
        void GetCustData(ref Guid guid, out object pVarVal);
        void GetFuncCustData(int index, ref Guid guid, out object pVarVal);
        void GetParamCustData(int indexFunc, int indexParam, ref Guid guid, out object pVarVal);
        void GetVarCustData(int index, ref Guid guid, out object pVarVal);
        void GetImplTypeCustData(int index, ref Guid guid, out object pVarVal);
        [LCIDConversion(1)]
        void GetDocumentation2(int memid, out string pbstrHelpString, out int pdwHelpStringContext, out string pbstrHelpStringDll);
        void GetAllCustData(IntPtr pCustData);
        void GetAllFuncCustData(int index, IntPtr pCustData);
        void GetAllParamCustData(int indexFunc, int indexParam, IntPtr pCustData);
        void GetAllVarCustData(int index, IntPtr pCustData);
        void GetAllImplTypeCustData(int index, IntPtr pCustData);
    }
}

