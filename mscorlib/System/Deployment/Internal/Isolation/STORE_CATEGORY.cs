namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct STORE_CATEGORY
    {
        public IDefinitionIdentity DefinitionIdentity;
    }
}

