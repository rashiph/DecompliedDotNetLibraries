namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public abstract class RandomNumberGenerator : IDisposable
    {
        protected RandomNumberGenerator()
        {
        }

        [SecuritySafeCritical]
        public static RandomNumberGenerator Create()
        {
            return Create("System.Security.Cryptography.RandomNumberGenerator");
        }

        [SecuritySafeCritical]
        public static RandomNumberGenerator Create(string rngName)
        {
            return (RandomNumberGenerator) CryptoConfig.CreateFromName(rngName);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public abstract void GetBytes(byte[] data);
        public abstract void GetNonZeroBytes(byte[] data);
    }
}

