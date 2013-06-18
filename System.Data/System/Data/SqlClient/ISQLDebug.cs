namespace System.Data.SqlClient
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComImport, BestFitMapping(false, ThrowOnUnmappableChar=true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true), Guid("6cb925bf-c3c0-45b3-9f44-5dd67c7b7fe8")]
    internal interface ISQLDebug
    {
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        bool SQLDebug(int dwpidDebugger, int dwpidDebuggee, [MarshalAs(UnmanagedType.LPStr)] string pszMachineName, [MarshalAs(UnmanagedType.LPStr)] string pszSDIDLLName, int dwOption, int cbData, byte[] rgbData);
    }
}

