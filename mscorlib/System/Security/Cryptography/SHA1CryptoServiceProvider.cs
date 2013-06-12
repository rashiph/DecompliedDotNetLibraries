namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public sealed class SHA1CryptoServiceProvider : SHA1
    {
        [SecurityCritical]
        private SafeHashHandle _safeHashHandle = Utils.CreateHash(Utils.StaticProvHandle, 0x8004);

        [SecuritySafeCritical]
        protected override void Dispose(bool disposing)
        {
            if ((this._safeHashHandle != null) && !this._safeHashHandle.IsClosed)
            {
                this._safeHashHandle.Dispose();
            }
            base.Dispose(disposing);
        }

        [SecuritySafeCritical]
        protected override void HashCore(byte[] rgb, int ibStart, int cbSize)
        {
            Utils.HashData(this._safeHashHandle, rgb, ibStart, cbSize);
        }

        [SecuritySafeCritical]
        protected override byte[] HashFinal()
        {
            return Utils.EndHash(this._safeHashHandle);
        }

        [SecuritySafeCritical]
        public override void Initialize()
        {
            if ((this._safeHashHandle != null) && !this._safeHashHandle.IsClosed)
            {
                this._safeHashHandle.Dispose();
            }
            this._safeHashHandle = Utils.CreateHash(Utils.StaticProvHandle, 0x8004);
        }
    }
}

