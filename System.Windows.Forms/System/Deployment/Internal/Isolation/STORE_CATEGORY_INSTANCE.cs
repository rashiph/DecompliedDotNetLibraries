namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct STORE_CATEGORY_INSTANCE
    {
        public System.Deployment.Internal.Isolation.IDefinitionAppId DefinitionAppId_Application;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string XMLSnippet;
    }
}

