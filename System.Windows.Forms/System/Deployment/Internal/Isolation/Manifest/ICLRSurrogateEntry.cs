namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("1E0422A1-F0D2-44ae-914B-8A2DECCFD22B")]
    internal interface ICLRSurrogateEntry
    {
        System.Deployment.Internal.Isolation.Manifest.CLRSurrogateEntry AllData { [SecurityCritical] get; }
        Guid Clsid { [SecurityCritical] get; }
        string RuntimeVersion { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string ClassName { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

