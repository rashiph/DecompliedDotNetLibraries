namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CertEnhKeyUse
    {
        public uint cUsageIdentifier;
        public unsafe void* rgpszUsageIdentifier;
    }
}

