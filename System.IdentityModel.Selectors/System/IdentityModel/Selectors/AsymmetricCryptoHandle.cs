namespace System.IdentityModel.Selectors
{
    using System;

    internal class AsymmetricCryptoHandle : ProofTokenCryptoHandle
    {
        private AsymmetricCryptoHandle(InternalRefCountedHandle internalHandle) : base(internalHandle)
        {
        }

        public AsymmetricCryptoHandle(InternalRefCountedHandle nativeHandle, DateTime expiration, IntPtr parameters) : base(nativeHandle, expiration, parameters, typeof(RpcAsymmetricCryptoParameters))
        {
        }

        protected override InfoCardProofToken OnCreateProofToken()
        {
            return new InfoCardProofToken(this, base.Expiration);
        }

        protected override CryptoHandle OnDuplicate()
        {
            return new AsymmetricCryptoHandle(base.InternalHandle);
        }
    }
}

