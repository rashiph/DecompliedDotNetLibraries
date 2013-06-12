namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public abstract class KeyedHashAlgorithm : HashAlgorithm
    {
        protected byte[] KeyValue;

        protected KeyedHashAlgorithm()
        {
        }

        [SecuritySafeCritical]
        public static KeyedHashAlgorithm Create()
        {
            return Create("System.Security.Cryptography.KeyedHashAlgorithm");
        }

        [SecuritySafeCritical]
        public static KeyedHashAlgorithm Create(string algName)
        {
            return (KeyedHashAlgorithm) CryptoConfig.CreateFromName(algName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.KeyValue != null)
                {
                    Array.Clear(this.KeyValue, 0, this.KeyValue.Length);
                }
                this.KeyValue = null;
            }
            base.Dispose(disposing);
        }

        public virtual byte[] Key
        {
            get
            {
                return (byte[]) this.KeyValue.Clone();
            }
            set
            {
                if (base.State != 0)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_HashKeySet"));
                }
                this.KeyValue = (byte[]) value.Clone();
            }
        }
    }
}

