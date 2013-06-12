namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public abstract class DeriveBytes : IDisposable
    {
        protected DeriveBytes()
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public abstract byte[] GetBytes(int cb);
        public abstract void Reset();
    }
}

