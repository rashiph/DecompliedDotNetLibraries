namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards;
    using Microsoft.InfoCards.Diagnostics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal abstract class CryptoHandle : IDisposable
    {
        private InternalRefCountedHandle m_internalHandle;
        private bool m_isDisposed;

        protected CryptoHandle(InternalRefCountedHandle internalHandle)
        {
            this.m_internalHandle = internalHandle;
            this.m_internalHandle.AddRef();
        }

        protected CryptoHandle(InternalRefCountedHandle nativeHandle, DateTime expiration, IntPtr nativeParameters, Type paramType)
        {
            this.m_internalHandle = nativeHandle;
            this.m_internalHandle.Initialize(expiration, Marshal.PtrToStructure(nativeParameters, paramType));
        }

        internal static CryptoHandle Create(InternalRefCountedHandle nativeHandle)
        {
            CryptoHandle handle = null;
            CryptoHandle handle3;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                nativeHandle.DangerousAddRef(ref success);
                RpcInfoCardCryptoHandle handle2 = (RpcInfoCardCryptoHandle) Marshal.PtrToStructure(nativeHandle.DangerousGetHandle(), typeof(RpcInfoCardCryptoHandle));
                DateTime expiration = DateTime.FromFileTimeUtc(handle2.expiration);
                switch (handle2.type)
                {
                    case RpcInfoCardCryptoHandle.HandleType.Asymmetric:
                        handle = new AsymmetricCryptoHandle(nativeHandle, expiration, handle2.cryptoParameters);
                        break;

                    case RpcInfoCardCryptoHandle.HandleType.Symmetric:
                        handle = new SymmetricCryptoHandle(nativeHandle, expiration, handle2.cryptoParameters);
                        break;

                    case RpcInfoCardCryptoHandle.HandleType.Transform:
                        handle = new TransformCryptoHandle(nativeHandle, expiration, handle2.cryptoParameters);
                        break;

                    case RpcInfoCardCryptoHandle.HandleType.Hash:
                        handle = new HashCryptoHandle(nativeHandle, expiration, handle2.cryptoParameters);
                        break;

                    default:
                        throw InfoCardTrace.ThrowHelperError(new InvalidOperationException(Microsoft.InfoCards.SR.GetString("GeneralExceptionMessage")));
                }
                handle3 = handle;
            }
            finally
            {
                if (success)
                {
                    nativeHandle.DangerousRelease();
                }
            }
            return handle3;
        }

        public void Dispose()
        {
            if (!this.m_isDisposed)
            {
                this.m_internalHandle.Release();
                this.m_internalHandle = null;
                this.m_isDisposed = true;
            }
        }

        public CryptoHandle Duplicate()
        {
            this.ThrowIfDisposed();
            return this.OnDuplicate();
        }

        protected abstract CryptoHandle OnDuplicate();
        protected void ThrowIfDisposed()
        {
            if (this.m_isDisposed)
            {
                throw InfoCardTrace.ThrowHelperError(new ObjectDisposedException(Microsoft.InfoCards.SR.GetString("ClientCryptoSessionDisposed")));
            }
        }

        public DateTime Expiration
        {
            get
            {
                this.ThrowIfDisposed();
                return this.m_internalHandle.Expiration;
            }
        }

        public InternalRefCountedHandle InternalHandle
        {
            get
            {
                this.ThrowIfDisposed();
                return this.m_internalHandle;
            }
        }

        public object Parameters
        {
            get
            {
                this.ThrowIfDisposed();
                return this.m_internalHandle.Parameters;
            }
        }
    }
}

