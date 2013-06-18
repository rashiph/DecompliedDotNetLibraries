namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards;
    using Microsoft.InfoCards.Diagnostics;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    internal class InfoCardKeyedHashAlgorithm : KeyedHashAlgorithm
    {
        private byte[] m_cachedBlock;
        private HashCryptoHandle m_cryptoHandle;
        private RpcHashCryptoParameters m_param;

        public InfoCardKeyedHashAlgorithm(SymmetricCryptoHandle cryptoHandle)
        {
            InternalRefCountedHandle nativeHashHandle = null;
            try
            {
                int status = CardSpaceSelector.GetShim().m_csShimGetKeyedHash(cryptoHandle.InternalHandle, out nativeHashHandle);
                if (status != 0)
                {
                    InfoCardTrace.CloseInvalidOutSafeHandle(nativeHashHandle);
                    ExceptionHelper.ThrowIfCardSpaceException(status);
                    throw InfoCardTrace.ThrowHelperError(new Win32Exception(status));
                }
                this.m_cryptoHandle = (HashCryptoHandle) CryptoHandle.Create(nativeHashHandle);
                this.m_param = (RpcHashCryptoParameters) this.m_cryptoHandle.Parameters;
            }
            catch
            {
                if (this.m_cryptoHandle != null)
                {
                    this.m_cryptoHandle.Dispose();
                }
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.m_cachedBlock != null)
                {
                    Array.Clear(this.m_cachedBlock, 0, this.m_cachedBlock.Length);
                }
                this.m_cryptoHandle.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (this.m_cachedBlock != null)
            {
                using (HGlobalSafeHandle handle = null)
                {
                    if (this.m_cachedBlock.Length != 0)
                    {
                        handle = HGlobalSafeHandle.Construct(this.m_cachedBlock.Length);
                        Marshal.Copy(this.m_cachedBlock, 0, handle.DangerousGetHandle(), this.m_cachedBlock.Length);
                    }
                    int status = CardSpaceSelector.GetShim().m_csShimHashCore(this.m_cryptoHandle.InternalHandle, this.m_cachedBlock.Length, (handle != null) ? handle : HGlobalSafeHandle.Construct());
                    if (status != 0)
                    {
                        ExceptionHelper.ThrowIfCardSpaceException(status);
                        throw InfoCardTrace.ThrowHelperError(new Win32Exception(status));
                    }
                }
            }
            if (this.m_cachedBlock != null)
            {
                Array.Clear(this.m_cachedBlock, 0, this.m_cachedBlock.Length);
            }
            this.m_cachedBlock = DiagnosticUtility.Utility.AllocateByteArray(cbSize);
            Array.Copy(array, ibStart, this.m_cachedBlock, 0, cbSize);
        }

        protected override byte[] HashFinal()
        {
            byte[] destination = null;
            int cbOutData = 0;
            HGlobalSafeHandle handle = null;
            GlobalAllocSafeHandle pOutData = null;
            try
            {
                if (this.m_cachedBlock == null)
                {
                    return destination;
                }
                if (this.m_cachedBlock.Length != 0)
                {
                    handle = HGlobalSafeHandle.Construct(this.m_cachedBlock.Length);
                    Marshal.Copy(this.m_cachedBlock, 0, handle.DangerousGetHandle(), this.m_cachedBlock.Length);
                }
                int status = CardSpaceSelector.GetShim().m_csShimHashFinal(this.m_cryptoHandle.InternalHandle, this.m_cachedBlock.Length, (handle != null) ? handle : HGlobalSafeHandle.Construct(), out cbOutData, out pOutData);
                if (status != 0)
                {
                    ExceptionHelper.ThrowIfCardSpaceException(status);
                    throw InfoCardTrace.ThrowHelperError(new Win32Exception(status));
                }
                pOutData.Length = cbOutData;
                destination = DiagnosticUtility.Utility.AllocateByteArray(pOutData.Length);
                using (pOutData)
                {
                    Marshal.Copy(pOutData.DangerousGetHandle(), destination, 0, pOutData.Length);
                }
            }
            finally
            {
                if (handle != null)
                {
                    handle.Dispose();
                }
                Array.Clear(this.m_cachedBlock, 0, this.m_cachedBlock.Length);
                this.m_cachedBlock = null;
            }
            return destination;
        }

        public override void Initialize()
        {
        }

        public override bool CanReuseTransform
        {
            get
            {
                return this.m_param.transform.canReuseTransform;
            }
        }

        public override bool CanTransformMultipleBlocks
        {
            get
            {
                return this.m_param.transform.canTransformMultipleBlocks;
            }
        }

        public override int HashSize
        {
            get
            {
                return this.m_param.hashSize;
            }
        }

        public override int InputBlockSize
        {
            get
            {
                return this.m_param.transform.inputBlockSize;
            }
        }

        public override byte[] Key
        {
            get
            {
                throw InfoCardTrace.ThrowHelperError(new NotImplementedException());
            }
        }

        public override int OutputBlockSize
        {
            get
            {
                return this.m_param.transform.outputBlockSize;
            }
        }
    }
}

