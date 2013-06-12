namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("0000000e-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBindCtx
    {
        void RegisterObjectBound([MarshalAs(UnmanagedType.Interface)] object punk);
        void RevokeObjectBound([MarshalAs(UnmanagedType.Interface)] object punk);
        void ReleaseBoundObjects();
        void SetBindOptions([In] ref System.Runtime.InteropServices.ComTypes.BIND_OPTS pbindopts);
        void GetBindOptions(ref System.Runtime.InteropServices.ComTypes.BIND_OPTS pbindopts);
        void GetRunningObjectTable(out IRunningObjectTable pprot);
        void RegisterObjectParam([MarshalAs(UnmanagedType.LPWStr)] string pszKey, [MarshalAs(UnmanagedType.Interface)] object punk);
        void GetObjectParam([MarshalAs(UnmanagedType.LPWStr)] string pszKey, [MarshalAs(UnmanagedType.Interface)] out object ppunk);
        void EnumObjectParam(out IEnumString ppenum);
        [PreserveSig]
        int RevokeObjectParam([MarshalAs(UnmanagedType.LPWStr)] string pszKey);
    }
}

