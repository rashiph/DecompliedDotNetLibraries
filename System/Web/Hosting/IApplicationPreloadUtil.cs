namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("940D8ADD-9E40-4475-9A67-2CDCDF57995C")]
    public interface IApplicationPreloadUtil
    {
        void GetApplicationPreloadInfo([In, MarshalAs(UnmanagedType.LPWStr)] string context, [MarshalAs(UnmanagedType.Bool)] out bool enabled, [MarshalAs(UnmanagedType.BStr)] out string startupObjType, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_BSTR)] out string[] parametersForStartupObj);
        void ReportApplicationPreloadFailure([In, MarshalAs(UnmanagedType.LPWStr)] string context, [In, MarshalAs(UnmanagedType.U4)] int errorCode, [In, MarshalAs(UnmanagedType.LPWStr)] string errorMessage);
    }
}

