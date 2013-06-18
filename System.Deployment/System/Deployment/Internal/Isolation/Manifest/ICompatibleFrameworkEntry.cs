namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("C98BFE2A-62C9-40AD-ADCE-A9037BE2BE6C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICompatibleFrameworkEntry
    {
        System.Deployment.Internal.Isolation.Manifest.CompatibleFrameworkEntry AllData { [SecurityCritical] get; }
        uint index { [SecurityCritical] get; }
        string TargetVersion { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string Profile { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string SupportedRuntime { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

