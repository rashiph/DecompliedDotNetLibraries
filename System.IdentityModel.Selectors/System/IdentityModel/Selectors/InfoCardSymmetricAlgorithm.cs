namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards;
    using Microsoft.InfoCards.Diagnostics;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    internal class InfoCardSymmetricAlgorithm : SymmetricAlgorithm, IDisposable
    {
        private SymmetricCryptoHandle m_cryptoHandle;
        private RpcSymmetricCryptoParameters m_parameters;
        private static readonly RandomNumberGenerator random = new RNGCryptoServiceProvider();

        public InfoCardSymmetricAlgorithm(SymmetricCryptoHandle cryptoHandle)
        {
            this.m_cryptoHandle = (SymmetricCryptoHandle) cryptoHandle.Duplicate();
            try
            {
                this.m_parameters = (RpcSymmetricCryptoParameters) this.m_cryptoHandle.Parameters;
                base.KeySizeValue = this.m_parameters.keySize;
                base.BlockSizeValue = this.m_parameters.blockSize;
                base.FeedbackSizeValue = this.m_parameters.feedbackSize;
                base.LegalBlockSizesValue = new KeySizes[] { new KeySizes(base.BlockSizeValue, base.BlockSizeValue, 0) };
                base.LegalKeySizesValue = new KeySizes[] { new KeySizes(base.KeySizeValue, base.KeySizeValue, 0) };
            }
            catch
            {
                this.m_cryptoHandle.Dispose();
                throw;
            }
        }

        public override ICryptoTransform CreateDecryptor()
        {
            return new CryptoTransform(this, CryptoTransform.Direction.Decrypt);
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw InfoCardTrace.ThrowHelperError(new NotImplementedException());
        }

        public override ICryptoTransform CreateEncryptor()
        {
            return new CryptoTransform(this, CryptoTransform.Direction.Encrypt);
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw InfoCardTrace.ThrowHelperError(new NotImplementedException());
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override void GenerateIV()
        {
            byte[] data = new byte[this.BlockSize / 8];
            random.GetBytes(data);
            base.IVValue = data;
        }

        public override void GenerateKey()
        {
            throw InfoCardTrace.ThrowHelperError(new NotImplementedException());
        }

        public override byte[] Key
        {
            get
            {
                throw InfoCardTrace.ThrowHelperError(new NotImplementedException());
            }
            set
            {
                throw InfoCardTrace.ThrowHelperError(new NotImplementedException());
            }
        }

        private class CryptoTransform : ICryptoTransform, IDisposable
        {
            private RpcTransformCryptoParameters m_param;
            private TransformCryptoHandle m_transCryptoHandle;

            public CryptoTransform(InfoCardSymmetricAlgorithm symAlgo, Direction cryptoDirection)
            {
                InternalRefCountedHandle nativeTransformHandle = null;
                byte[] iV = symAlgo.IV;
                using (HGlobalSafeHandle handle2 = HGlobalSafeHandle.Construct(iV.Length))
                {
                    Marshal.Copy(iV, 0, handle2.DangerousGetHandle(), iV.Length);
                    int status = CardSpaceSelector.GetShim().m_csShimGetCryptoTransform(symAlgo.m_cryptoHandle.InternalHandle, (int) symAlgo.Mode, (int) symAlgo.Padding, symAlgo.FeedbackSize, (int) cryptoDirection, iV.Length, handle2, out nativeTransformHandle);
                    if (status != 0)
                    {
                        InfoCardTrace.CloseInvalidOutSafeHandle(nativeTransformHandle);
                        ExceptionHelper.ThrowIfCardSpaceException(status);
                        throw InfoCardTrace.ThrowHelperError(new Win32Exception(status));
                    }
                    this.m_transCryptoHandle = (TransformCryptoHandle) CryptoHandle.Create(nativeTransformHandle);
                    this.m_param = (RpcTransformCryptoParameters) this.m_transCryptoHandle.Parameters;
                }
            }

            public void Dispose()
            {
                if (this.m_transCryptoHandle != null)
                {
                    this.m_transCryptoHandle.Dispose();
                    this.m_transCryptoHandle = null;
                }
            }

            public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                GlobalAllocSafeHandle pOutData = null;
                int cbOutData = 0;
                using (HGlobalSafeHandle handle2 = HGlobalSafeHandle.Construct(inputCount))
                {
                    Marshal.Copy(inputBuffer, inputOffset, handle2.DangerousGetHandle(), inputCount);
                    int status = CardSpaceSelector.GetShim().m_csShimTransformBlock(this.m_transCryptoHandle.InternalHandle, inputCount, handle2, out cbOutData, out pOutData);
                    if (status != 0)
                    {
                        ExceptionHelper.ThrowIfCardSpaceException(status);
                        throw InfoCardTrace.ThrowHelperError(new Win32Exception(status));
                    }
                    pOutData.Length = cbOutData;
                    using (pOutData)
                    {
                        Marshal.Copy(pOutData.DangerousGetHandle(), outputBuffer, outputOffset, pOutData.Length);
                    }
                }
                return cbOutData;
            }

            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                GlobalAllocSafeHandle pOutData = null;
                byte[] buffer;
                int cbOutData = 0;
                using (HGlobalSafeHandle handle2 = HGlobalSafeHandle.Construct(inputCount))
                {
                    Marshal.Copy(inputBuffer, inputOffset, handle2.DangerousGetHandle(), inputCount);
                    int status = CardSpaceSelector.GetShim().m_csShimTransformFinalBlock(this.m_transCryptoHandle.InternalHandle, inputCount, handle2, out cbOutData, out pOutData);
                    if (status != 0)
                    {
                        ExceptionHelper.ThrowIfCardSpaceException(status);
                        throw InfoCardTrace.ThrowHelperError(new Win32Exception(status));
                    }
                    pOutData.Length = cbOutData;
                    buffer = DiagnosticUtility.Utility.AllocateByteArray(pOutData.Length);
                    using (pOutData)
                    {
                        Marshal.Copy(pOutData.DangerousGetHandle(), buffer, 0, pOutData.Length);
                    }
                }
                return buffer;
            }

            public bool CanReuseTransform
            {
                get
                {
                    return this.m_param.canReuseTransform;
                }
            }

            public bool CanTransformMultipleBlocks
            {
                get
                {
                    return this.m_param.canTransformMultipleBlocks;
                }
            }

            public int InputBlockSize
            {
                get
                {
                    return this.m_param.inputBlockSize;
                }
            }

            public int OutputBlockSize
            {
                get
                {
                    return this.m_param.outputBlockSize;
                }
            }

            public enum Direction
            {
                Decrypt = 2,
                Encrypt = 1
            }
        }
    }
}

