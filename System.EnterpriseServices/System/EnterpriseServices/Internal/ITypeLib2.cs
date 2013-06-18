namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020411-0000-0000-C000-000000000046")]
    internal interface ITypeLib2
    {
        int GetTypeInfoCount();
        int GetTypeInfo(int index, out ITypeInfo ti);
        int GetTypeInfoType(int index, out System.Runtime.InteropServices.ComTypes.TYPEKIND tkind);
        int GetTypeInfoOfGuid(ref Guid guid, ITypeInfo ti);
        int GetLibAttr(out System.Runtime.InteropServices.ComTypes.TYPELIBATTR tlibattr);
        int GetTypeComp(out ITypeComp tcomp);
        int GetDocumentation(int index, [MarshalAs(UnmanagedType.BStr)] out string name, [MarshalAs(UnmanagedType.BStr)] out string docString, out int helpContext, [MarshalAs(UnmanagedType.BStr)] out string helpFile);
        int IsName([MarshalAs(UnmanagedType.LPWStr)] ref string nameBuf, int hashVal, out int isName);
        int FindName([MarshalAs(UnmanagedType.LPWStr)] ref string szNameBuf, int hashVal, [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.Interface, SizeParamIndex=4)] out ITypeInfo[] tis, [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.I4, SizeParamIndex=4)] out int[] memIds, ref int foundCount);
        void ReleaseTLibAttr(System.Runtime.InteropServices.ComTypes.TYPELIBATTR libattr);
        int GetCustData(ref Guid guid, out object value);
        int GetLibStatistics(out int uniqueNames, out int chUniqueNames);
        int GetDocumentation2(int index, int lcid, [MarshalAs(UnmanagedType.BStr)] out string helpString, out int helpStringContext, [MarshalAs(UnmanagedType.BStr)] string helpStringDll);
        int GetAllCustData(out IntPtr custdata);
    }
}

