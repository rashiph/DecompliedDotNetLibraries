namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime;

    internal abstract class ProofTokenCryptoHandle : CryptoHandle
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ProofTokenCryptoHandle(InternalRefCountedHandle internalHandle) : base(internalHandle)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ProofTokenCryptoHandle(InternalRefCountedHandle nativeHandle, DateTime expiration, IntPtr nativeParameters, Type paramType) : base(nativeHandle, expiration, nativeParameters, paramType)
        {
        }

        public InfoCardProofToken CreateProofToken()
        {
            base.ThrowIfDisposed();
            return this.OnCreateProofToken();
        }

        protected abstract InfoCardProofToken OnCreateProofToken();
    }
}

