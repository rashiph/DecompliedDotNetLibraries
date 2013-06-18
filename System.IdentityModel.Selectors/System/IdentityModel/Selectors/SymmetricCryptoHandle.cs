namespace System.IdentityModel.Selectors
{
    using System;

    internal class SymmetricCryptoHandle : ProofTokenCryptoHandle
    {
        private SymmetricCryptoHandle(InternalRefCountedHandle internalHandle) : base(internalHandle)
        {
        }

        public SymmetricCryptoHandle(InternalRefCountedHandle nativeHandle, DateTime expiration, IntPtr parameters) : base(nativeHandle, expiration, parameters, typeof(RpcSymmetricCryptoParameters))
        {
        }

        protected override InfoCardProofToken OnCreateProofToken()
        {
            return new InfoCardProofToken(this, base.Expiration);
        }

        protected override CryptoHandle OnDuplicate()
        {
            return new SymmetricCryptoHandle(base.InternalHandle);
        }
    }
}

