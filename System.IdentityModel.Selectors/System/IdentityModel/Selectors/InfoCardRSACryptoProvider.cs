namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards;
    using Microsoft.InfoCards.Diagnostics;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    internal class InfoCardRSACryptoProvider : RSA
    {
        private AsymmetricCryptoHandle m_cryptoHandle;
        private RpcAsymmetricCryptoParameters m_params;

        public InfoCardRSACryptoProvider(AsymmetricCryptoHandle cryptoHandle)
        {
            this.m_cryptoHandle = (AsymmetricCryptoHandle) cryptoHandle.Duplicate();
            try
            {
                this.m_params = (RpcAsymmetricCryptoParameters) this.m_cryptoHandle.Parameters;
                int keySize = this.m_params.keySize;
                base.LegalKeySizesValue = new KeySizes[1];
                base.KeySizeValue = keySize;
                base.LegalKeySizesValue[0] = new KeySizes(keySize, keySize, 0);
            }
            catch
            {
                this.m_cryptoHandle.Dispose();
                this.m_cryptoHandle = null;
                throw;
            }
        }

        public byte[] Decrypt(byte[] inData, bool fAOEP)
        {
            GlobalAllocSafeHandle pOutData = null;
            byte[] buffer;
            int pcbOutData = 0;
            InfoCardTrace.ThrowInvalidArgumentConditional(null == inData, "indata");
            using (HGlobalSafeHandle handle2 = HGlobalSafeHandle.Construct(inData.Length))
            {
                Marshal.Copy(inData, 0, handle2.DangerousGetHandle(), inData.Length);
                int status = CardSpaceSelector.GetShim().m_csShimDecrypt(this.m_cryptoHandle.InternalHandle, fAOEP, inData.Length, handle2, out pcbOutData, out pOutData);
                if (status != 0)
                {
                    ExceptionHelper.ThrowIfCardSpaceException(status);
                    throw InfoCardTrace.ThrowHelperError(new Win32Exception(status));
                }
                pOutData.Length = pcbOutData;
                buffer = DiagnosticUtility.Utility.AllocateByteArray(pOutData.Length);
                using (pOutData)
                {
                    Marshal.Copy(pOutData.DangerousGetHandle(), buffer, 0, pOutData.Length);
                }
            }
            return buffer;
        }

        public override byte[] DecryptValue(byte[] rgb)
        {
            throw InfoCardTrace.ThrowHelperError(new NotSupportedException());
        }

        protected override void Dispose(bool disposing)
        {
            this.m_cryptoHandle.Dispose();
        }

        public byte[] Encrypt(byte[] inData, bool fAOEP)
        {
            GlobalAllocSafeHandle pOutData = null;
            byte[] buffer;
            int pcbOutData = 0;
            InfoCardTrace.ThrowInvalidArgumentConditional(null == inData, "indata");
            using (HGlobalSafeHandle handle2 = HGlobalSafeHandle.Construct(inData.Length))
            {
                Marshal.Copy(inData, 0, handle2.DangerousGetHandle(), inData.Length);
                int status = CardSpaceSelector.GetShim().m_csShimEncrypt(this.m_cryptoHandle.InternalHandle, fAOEP, inData.Length, handle2, out pcbOutData, out pOutData);
                if (status != 0)
                {
                    ExceptionHelper.ThrowIfCardSpaceException(status);
                    throw InfoCardTrace.ThrowHelperError(new Win32Exception(status));
                }
                pOutData.Length = pcbOutData;
                buffer = DiagnosticUtility.Utility.AllocateByteArray(pOutData.Length);
                Marshal.Copy(pOutData.DangerousGetHandle(), buffer, 0, pOutData.Length);
            }
            return buffer;
        }

        public override byte[] EncryptValue(byte[] rgb)
        {
            throw InfoCardTrace.ThrowHelperError(new NotSupportedException());
        }

        public override RSAParameters ExportParameters(bool includePrivateParameters)
        {
            throw InfoCardTrace.ThrowHelperError(new NotSupportedException());
        }

        public override void FromXmlString(string xmlString)
        {
            throw InfoCardTrace.ThrowHelperError(new NotSupportedException());
        }

        public override void ImportParameters(RSAParameters parameters)
        {
            throw InfoCardTrace.ThrowHelperError(new NotSupportedException());
        }

        public byte[] SignHash(byte[] hash, string hashAlgOid)
        {
            InfoCardTrace.ThrowInvalidArgumentConditional((hash == null) || (0 == hash.Length), "hash");
            InfoCardTrace.ThrowInvalidArgumentConditional(string.IsNullOrEmpty(hashAlgOid), "hashAlgOid");
            int pcbSig = 0;
            GlobalAllocSafeHandle pSig = null;
            using (HGlobalSafeHandle handle2 = HGlobalSafeHandle.Construct(hash.Length))
            {
                using (HGlobalSafeHandle handle3 = HGlobalSafeHandle.Construct(hashAlgOid))
                {
                    Marshal.Copy(hash, 0, handle2.DangerousGetHandle(), hash.Length);
                    RuntimeHelpers.PrepareConstrainedRegions();
                    int status = CardSpaceSelector.GetShim().m_csShimSignHash(this.m_cryptoHandle.InternalHandle, hash.Length, handle2, handle3, out pcbSig, out pSig);
                    if (status != 0)
                    {
                        ExceptionHelper.ThrowIfCardSpaceException(status);
                        throw InfoCardTrace.ThrowHelperError(new Win32Exception(status));
                    }
                    pSig.Length = pcbSig;
                    byte[] destination = DiagnosticUtility.Utility.AllocateByteArray(pSig.Length);
                    using (pSig)
                    {
                        Marshal.Copy(pSig.DangerousGetHandle(), destination, 0, pSig.Length);
                    }
                    return destination;
                }
            }
        }

        public override string ToXmlString(bool includePrivateParameters)
        {
            throw InfoCardTrace.ThrowHelperError(new NotSupportedException());
        }

        public bool VerifyHash(byte[] hash, string hashAlgOid, byte[] sig)
        {
            InfoCardTrace.ThrowInvalidArgumentConditional((hash == null) || (0 == hash.Length), "hash");
            InfoCardTrace.ThrowInvalidArgumentConditional(string.IsNullOrEmpty(hashAlgOid), "hashAlgOid");
            InfoCardTrace.ThrowInvalidArgumentConditional((sig == null) || (0 == sig.Length), "sig");
            bool verified = false;
            using (HGlobalSafeHandle handle = HGlobalSafeHandle.Construct(hash.Length))
            {
                using (HGlobalSafeHandle handle2 = HGlobalSafeHandle.Construct(hashAlgOid))
                {
                    Marshal.Copy(hash, 0, handle.DangerousGetHandle(), hash.Length);
                    int status = 0;
                    using (HGlobalSafeHandle handle3 = HGlobalSafeHandle.Construct(sig.Length))
                    {
                        Marshal.Copy(sig, 0, handle3.DangerousGetHandle(), sig.Length);
                        status = CardSpaceSelector.GetShim().m_csShimVerifyHash(this.m_cryptoHandle.InternalHandle, hash.Length, handle, handle2, sig.Length, handle3, out verified);
                    }
                    if (status != 0)
                    {
                        ExceptionHelper.ThrowIfCardSpaceException(status);
                        throw InfoCardTrace.ThrowHelperError(new Win32Exception(status));
                    }
                }
            }
            return verified;
        }

        public override string KeyExchangeAlgorithm
        {
            get
            {
                return this.m_params.keyExchangeAlgorithm;
            }
        }

        public override string SignatureAlgorithm
        {
            get
            {
                return this.m_params.signatureAlgorithm;
            }
        }
    }
}

