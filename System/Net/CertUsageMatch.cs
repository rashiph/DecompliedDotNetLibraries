namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CertUsageMatch
    {
        public CertUsage dwType;
        public CertEnhKeyUse Usage;
    }
}

