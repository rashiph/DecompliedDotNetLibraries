namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards;
    using Microsoft.InfoCards.Diagnostics;
    using System;
    using System.ComponentModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    internal class InfoCardSymmetricCrypto : SymmetricSecurityKey, IDisposable
    {
        private SymmetricCryptoHandle m_cryptoHandle;
        private RpcSymmetricCryptoParameters m_params;

        public InfoCardSymmetricCrypto(SymmetricCryptoHandle cryptoHandle)
        {
            this.m_cryptoHandle = (SymmetricCryptoHandle) cryptoHandle.Duplicate();
            try
            {
                this.m_params = (RpcSymmetricCryptoParameters) this.m_cryptoHandle.Parameters;
            }
            catch
            {
                if (this.m_cryptoHandle != null)
                {
                    this.m_cryptoHandle.Dispose();
                    this.m_cryptoHandle = null;
                }
                throw;
            }
        }

        public override byte[] DecryptKey(string algorithmUri, byte[] keyData)
        {
            throw InfoCardTrace.ThrowHelperError(new NotImplementedException());
        }

        public void Dispose()
        {
            if (this.m_cryptoHandle != null)
            {
                this.m_cryptoHandle.Dispose();
                this.m_cryptoHandle = null;
            }
        }

        public override byte[] EncryptKey(string algorithmUri, byte[] keyData)
        {
            throw InfoCardTrace.ThrowHelperError(new NotImplementedException());
        }

        public override byte[] GenerateDerivedKey(string algorithmUri, byte[] label, byte[] nonce, int derivedKeyLength, int offset)
        {
            if (!this.IsSupportedAlgorithm(algorithmUri))
            {
                throw InfoCardTrace.ThrowHelperError(new NotSupportedException(Microsoft.InfoCards.SR.GetString("ClientUnsupportedCryptoAlgorithm", new object[] { algorithmUri })));
            }
            byte[] destination = null;
            using (HGlobalSafeHandle handle = HGlobalSafeHandle.Construct(label.Length))
            {
                using (HGlobalSafeHandle handle2 = HGlobalSafeHandle.Construct(nonce.Length))
                {
                    GlobalAllocSafeHandle pDerivedKey = null;
                    int cbDerivedKey = 0;
                    Marshal.Copy(label, 0, handle.DangerousGetHandle(), label.Length);
                    Marshal.Copy(nonce, 0, handle2.DangerousGetHandle(), nonce.Length);
                    int error = CardSpaceSelector.GetShim().m_csShimGenerateDerivedKey(this.m_cryptoHandle.InternalHandle, label.Length, handle, nonce.Length, handle2, derivedKeyLength, offset, algorithmUri, out cbDerivedKey, out pDerivedKey);
                    if (error != 0)
                    {
                        throw InfoCardTrace.ThrowHelperError(new Win32Exception(error));
                    }
                    pDerivedKey.Length = cbDerivedKey;
                    destination = new byte[pDerivedKey.Length];
                    using (pDerivedKey)
                    {
                        Marshal.Copy(pDerivedKey.DangerousGetHandle(), destination, 0, pDerivedKey.Length);
                    }
                    return destination;
                }
            }
        }

        public override ICryptoTransform GetDecryptionTransform(string algorithmUri, byte[] iv)
        {
            string str;
            if (((str = algorithmUri) != null) && (str == "http://www.w3.org/2001/04/xmlenc#aes128-cbc"))
            {
                using (InfoCardSymmetricAlgorithm algorithm = new InfoCardSymmetricAlgorithm(this.m_cryptoHandle))
                {
                    algorithm.IV = iv;
                    return algorithm.CreateDecryptor();
                }
            }
            throw InfoCardTrace.ThrowHelperError(new NotSupportedException(Microsoft.InfoCards.SR.GetString("ClientUnsupportedCryptoAlgorithm", new object[] { algorithmUri })));
        }

        public override ICryptoTransform GetEncryptionTransform(string algorithmUri, byte[] iv)
        {
            string str;
            if (((str = algorithmUri) != null) && (str == "http://www.w3.org/2001/04/xmlenc#aes128-cbc"))
            {
                using (InfoCardSymmetricAlgorithm algorithm = new InfoCardSymmetricAlgorithm(this.m_cryptoHandle))
                {
                    algorithm.IV = iv;
                    return algorithm.CreateEncryptor();
                }
            }
            throw InfoCardTrace.ThrowHelperError(new NotSupportedException(Microsoft.InfoCards.SR.GetString("ClientUnsupportedCryptoAlgorithm", new object[] { algorithmUri })));
        }

        public override int GetIVSize(string algorithmUri)
        {
            string str;
            if (((str = algorithmUri) == null) || (str != "http://www.w3.org/2001/04/xmlenc#aes128-cbc"))
            {
                throw InfoCardTrace.ThrowHelperError(new NotSupportedException(Microsoft.InfoCards.SR.GetString("ClientUnsupportedCryptoAlgorithm", new object[] { algorithmUri })));
            }
            RpcSymmetricCryptoParameters parameters = (RpcSymmetricCryptoParameters) this.m_cryptoHandle.Parameters;
            return parameters.blockSize;
        }

        public override KeyedHashAlgorithm GetKeyedHashAlgorithm(string algorithmUri)
        {
            string str;
            if (((str = algorithmUri) == null) || (str != "http://www.w3.org/2000/09/xmldsig#hmac-sha1"))
            {
                throw InfoCardTrace.ThrowHelperError(new NotSupportedException(Microsoft.InfoCards.SR.GetString("ClientUnsupportedCryptoAlgorithm", new object[] { algorithmUri })));
            }
            return new InfoCardKeyedHashAlgorithm(this.m_cryptoHandle);
        }

        public override SymmetricAlgorithm GetSymmetricAlgorithm(string algorithmUri)
        {
            string str;
            if (((str = algorithmUri) == null) || (str != "http://www.w3.org/2001/04/xmlenc#aes128-cbc"))
            {
                throw InfoCardTrace.ThrowHelperError(new NotSupportedException(Microsoft.InfoCards.SR.GetString("ClientUnsupportedCryptoAlgorithm", new object[] { algorithmUri })));
            }
            return new InfoCardSymmetricAlgorithm(this.m_cryptoHandle);
        }

        public override byte[] GetSymmetricKey()
        {
            throw InfoCardTrace.ThrowHelperError(new NotImplementedException());
        }

        public override bool IsAsymmetricAlgorithm(string algorithmUri)
        {
            return InfoCardCryptoHelper.IsAsymmetricAlgorithm(algorithmUri);
        }

        public override bool IsSupportedAlgorithm(string algorithmUri)
        {
            string str;
            if (((str = algorithmUri) == null) || ((!(str == "http://www.w3.org/2001/04/xmlenc#aes128-cbc") && !(str == "http://www.w3.org/2000/09/xmldsig#hmac-sha1")) && (!(str == "http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1") && !(str == "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk/p_sha1"))))
            {
                return false;
            }
            return true;
        }

        public override bool IsSymmetricAlgorithm(string algorithmUri)
        {
            return this.IsSupportedAlgorithm(algorithmUri);
        }

        public override int KeySize
        {
            get
            {
                return this.m_params.keySize;
            }
        }
    }
}

