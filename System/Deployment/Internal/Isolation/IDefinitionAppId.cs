namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("d91e12d8-98ed-47fa-9936-39421283d59b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDefinitionAppId
    {
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [SecurityCritical]
        string get_SubscriptionId();
        void put_SubscriptionId([In, MarshalAs(UnmanagedType.LPWStr)] string Subscription);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [SecurityCritical]
        string get_Codebase();
        [SecurityCritical]
        void put_Codebase([In, MarshalAs(UnmanagedType.LPWStr)] string CodeBase);
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IEnumDefinitionIdentity EnumAppPath();
        [SecurityCritical]
        void SetAppPath([In] uint cIDefinitionIdentity, [In, MarshalAs(UnmanagedType.LPArray)] System.Deployment.Internal.Isolation.IDefinitionIdentity[] DefinitionIdentity);
    }
}

