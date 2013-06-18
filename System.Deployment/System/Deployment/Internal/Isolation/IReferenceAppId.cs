namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("054f0bef-9e45-4363-8f5a-2f8e142d9a3b")]
    internal interface IReferenceAppId
    {
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [SecurityCritical]
        string get_SubscriptionId();
        void put_SubscriptionId([In, MarshalAs(UnmanagedType.LPWStr)] string Subscription);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [SecurityCritical]
        string get_Codebase();
        void put_Codebase([In, MarshalAs(UnmanagedType.LPWStr)] string CodeBase);
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IEnumReferenceIdentity EnumAppPath();
    }
}

