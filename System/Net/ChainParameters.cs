namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ChainParameters
    {
        public uint cbSize;
        public CertUsageMatch RequestedUsage;
        public CertUsageMatch RequestedIssuancePolicy;
        public uint UrlRetrievalTimeout;
        public int BoolCheckRevocationFreshnessTime;
        public uint RevocationFreshnessTime;
        public static readonly uint StructSize;
        static ChainParameters()
        {
            StructSize = (uint) Marshal.SizeOf(typeof(ChainParameters));
        }
    }
}

