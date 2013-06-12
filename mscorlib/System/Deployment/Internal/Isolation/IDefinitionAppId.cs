namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("d91e12d8-98ed-47fa-9936-39421283d59b")]
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
        IEnumDefinitionIdentity EnumAppPath();
        [SecurityCritical]
        void SetAppPath([In] uint cIDefinitionIdentity, [In, MarshalAs(UnmanagedType.LPArray)] IDefinitionIdentity[] DefinitionIdentity);
    }
}

