namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class StrongNamePublicKeyBlob
    {
        internal byte[] PublicKey;

        internal StrongNamePublicKeyBlob()
        {
        }

        public StrongNamePublicKeyBlob(byte[] publicKey)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException("PublicKey");
            }
            this.PublicKey = new byte[publicKey.Length];
            Array.Copy(publicKey, 0, this.PublicKey, 0, publicKey.Length);
        }

        internal StrongNamePublicKeyBlob(string publicKey)
        {
            this.PublicKey = Hex.DecodeHexString(publicKey);
        }

        private static bool CompareArrays(byte[] first, byte[] second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }
            int length = first.Length;
            for (int i = 0; i < length; i++)
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return (((obj != null) && (obj is StrongNamePublicKeyBlob)) && this.Equals((StrongNamePublicKeyBlob) obj));
        }

        internal bool Equals(StrongNamePublicKeyBlob blob)
        {
            if (blob == null)
            {
                return false;
            }
            return CompareArrays(this.PublicKey, blob.PublicKey);
        }

        private static int GetByteArrayHashCode(byte[] baData)
        {
            if (baData == null)
            {
                return 0;
            }
            int num = 0;
            for (int i = 0; i < baData.Length; i++)
            {
                num = ((num << 8) ^ baData[i]) ^ (num >> 0x18);
            }
            return num;
        }

        public override int GetHashCode()
        {
            return GetByteArrayHashCode(this.PublicKey);
        }

        public override string ToString()
        {
            return Hex.EncodeHexString(this.PublicKey);
        }
    }
}

