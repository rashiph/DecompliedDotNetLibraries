namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ChainPolicyParameter
    {
        public uint cbSize;
        public uint dwFlags;
        public unsafe SSL_EXTRA_CERT_CHAIN_POLICY_PARA* pvExtraPolicyPara;
        public static readonly uint StructSize;
        static ChainPolicyParameter()
        {
            StructSize = (uint) Marshal.SizeOf(typeof(ChainPolicyParameter));
        }
    }
}

