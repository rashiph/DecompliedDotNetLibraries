namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct RSAParameters
    {
        public byte[] Exponent;
        public byte[] Modulus;
        [NonSerialized]
        public byte[] P;
        [NonSerialized]
        public byte[] Q;
        [NonSerialized]
        public byte[] DP;
        [NonSerialized]
        public byte[] DQ;
        [NonSerialized]
        public byte[] InverseQ;
        [NonSerialized]
        public byte[] D;
    }
}

