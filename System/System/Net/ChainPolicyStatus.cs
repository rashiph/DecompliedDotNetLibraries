namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ChainPolicyStatus
    {
        public uint cbSize;
        public uint dwError;
        public uint lChainIndex;
        public uint lElementIndex;
        public unsafe void* pvExtraPolicyStatus;
        public static readonly uint StructSize;
        static ChainPolicyStatus()
        {
            StructSize = (uint) Marshal.SizeOf(typeof(ChainPolicyStatus));
        }
    }
}

