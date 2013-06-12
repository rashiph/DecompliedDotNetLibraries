namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct DSAParameters
    {
        public byte[] P;
        public byte[] Q;
        public byte[] G;
        public byte[] Y;
        public byte[] J;
        [NonSerialized]
        public byte[] X;
        public byte[] Seed;
        public int Counter;
    }
}

