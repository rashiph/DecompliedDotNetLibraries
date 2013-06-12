namespace Microsoft.Win32
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, ComVisible(false), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("79eac9ee-baf9-11ce-8c82-00aa004ba90b")]
    internal interface IInternetSecurityManager
    {
        unsafe void SetSecuritySite(void* pSite);
        unsafe void GetSecuritySite(void** ppSite);
        [SuppressUnmanagedCodeSecurity]
        void MapUrlToZone([In, MarshalAs(UnmanagedType.BStr)] string pwszUrl, out int pdwZone, [In] int dwFlags);
        unsafe void GetSecurityId(string pwszUrl, byte* pbSecurityId, int* pcbSecurityId, int dwReserved);
        unsafe void ProcessUrlAction(string pwszUrl, int dwAction, byte* pPolicy, int cbPolicy, byte* pContext, int cbContext, int dwFlags, int dwReserved);
        unsafe void QueryCustomPolicy(string pwszUrl, void* guidKey, byte** ppPolicy, int* pcbPolicy, byte* pContext, int cbContext, int dwReserved);
        void SetZoneMapping(int dwZone, string lpszPattern, int dwFlags);
        unsafe void GetZoneMappings(int dwZone, void** ppenumString, int dwFlags);
    }
}

