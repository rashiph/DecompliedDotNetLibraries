namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020406-0000-0000-C000-000000000046")]
    internal interface ICreateTypeLib
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object CreateTypeInfo([In, MarshalAs(UnmanagedType.LPStr)] string szName, int tkind);
        void SetName(string szName);
        void SetVersion(short wMajorVerNum, short wMinorVerNum);
        void SetGuid([In, MarshalAs(UnmanagedType.LPStruct)] Guid guid);
        void SetDocString([In, MarshalAs(UnmanagedType.LPStr)] string szDoc);
        void SetHelpFileName([In, MarshalAs(UnmanagedType.LPStr)] string szHelpFileName);
        void SetHelpContext(int dwHelpContext);
        void SetLcid(int lcid);
        void SetLibFlags(int uLibFlags);
        void SaveAllChanges();
    }
}

