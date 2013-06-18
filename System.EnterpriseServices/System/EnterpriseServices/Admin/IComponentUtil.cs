namespace System.EnterpriseServices.Admin
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("6EB22873-8A19-11D0-81B6-00A0C9231C29")]
    internal interface IComponentUtil
    {
        [DispId(1)]
        void InstallComponent([In, MarshalAs(UnmanagedType.BStr)] string bstrDLLFile, [In, MarshalAs(UnmanagedType.BStr)] string bstrTypelibFile, [In, MarshalAs(UnmanagedType.BStr)] string bstrProxyStubDLLFile);
        [DispId(2)]
        void ImportComponent([In, MarshalAs(UnmanagedType.BStr)] string bstrCLSID);
        [DispId(3)]
        void ImportComponentByName([In, MarshalAs(UnmanagedType.BStr)] string bstrProgID);
        [DispId(4)]
        void GetCLSIDs([In, MarshalAs(UnmanagedType.BStr)] string bstrDLLFile, [In, MarshalAs(UnmanagedType.BStr)] string bstrTypelibFile, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] out object[] CLSIDS);
    }
}

