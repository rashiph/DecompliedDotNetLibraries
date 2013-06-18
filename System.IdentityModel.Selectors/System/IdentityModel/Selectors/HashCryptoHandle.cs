namespace System.IdentityModel.Selectors
{
    using System;

    internal class HashCryptoHandle : CryptoHandle
    {
        private HashCryptoHandle(InternalRefCountedHandle internalHandle) : base(internalHandle)
        {
        }

        public HashCryptoHandle(InternalRefCountedHandle nativeHandle, DateTime expiration, IntPtr parameters) : base(nativeHandle, expiration, parameters, typeof(RpcHashCryptoParameters))
        {
        }

        protected override CryptoHandle OnDuplicate()
        {
            return new HashCryptoHandle(base.InternalHandle);
        }
    }
}

