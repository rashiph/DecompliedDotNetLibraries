namespace System.Runtime.InteropServices
{
    using System;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Obsolete("Use System.Runtime.InteropServices.ComTypes.ITypeLib instead. http://go.microsoft.com/fwlink/?linkid=14202", false), Guid("00020402-0000-0000-C000-000000000046")]
    public interface UCOMITypeLib
    {
        [PreserveSig]
        int GetTypeInfoCount();
        void GetTypeInfo(int index, out UCOMITypeInfo ppTI);
        void GetTypeInfoType(int index, out TYPEKIND pTKind);
        void GetTypeInfoOfGuid(ref Guid guid, out UCOMITypeInfo ppTInfo);
        void GetLibAttr(out IntPtr ppTLibAttr);
        void GetTypeComp(out UCOMITypeComp ppTComp);
        void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsName([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal);
        void FindName([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal, [Out, MarshalAs(UnmanagedType.LPArray)] UCOMITypeInfo[] ppTInfo, [Out, MarshalAs(UnmanagedType.LPArray)] int[] rgMemId, ref short pcFound);
        [PreserveSig]
        void ReleaseTLibAttr(IntPtr pTLibAttr);
    }
}

