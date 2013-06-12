namespace System.Runtime.InteropServices
{
    using System;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Obsolete("Use System.Runtime.InteropServices.ComTypes.IBindCtx instead. http://go.microsoft.com/fwlink/?linkid=14202", false), Guid("0000000e-0000-0000-C000-000000000046")]
    public interface UCOMIBindCtx
    {
        void RegisterObjectBound([MarshalAs(UnmanagedType.Interface)] object punk);
        void RevokeObjectBound([MarshalAs(UnmanagedType.Interface)] object punk);
        void ReleaseBoundObjects();
        void SetBindOptions([In] ref BIND_OPTS pbindopts);
        void GetBindOptions(ref BIND_OPTS pbindopts);
        void GetRunningObjectTable(out UCOMIRunningObjectTable pprot);
        void RegisterObjectParam([MarshalAs(UnmanagedType.LPWStr)] string pszKey, [MarshalAs(UnmanagedType.Interface)] object punk);
        void GetObjectParam([MarshalAs(UnmanagedType.LPWStr)] string pszKey, [MarshalAs(UnmanagedType.Interface)] out object ppunk);
        void EnumObjectParam(out UCOMIEnumString ppenum);
        void RevokeObjectParam([MarshalAs(UnmanagedType.LPWStr)] string pszKey);
    }
}

