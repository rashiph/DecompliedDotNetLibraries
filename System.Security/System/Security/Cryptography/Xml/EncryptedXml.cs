namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EncryptedXml
    {
        private const int m_capacity = 4;
        private XmlDocument m_document;
        private System.Text.Encoding m_encoding;
        private Evidence m_evidence;
        private Hashtable m_keyNameMapping;
        private CipherMode m_mode;
        private PaddingMode m_padding;
        private string m_recipient;
        private XmlResolver m_xmlResolver;
        public const string XmlEncAES128KeyWrapUrl = "http://www.w3.org/2001/04/xmlenc#kw-aes128";
        public const string XmlEncAES128Url = "http://www.w3.org/2001/04/xmlenc#aes128-cbc";
        public const string XmlEncAES192KeyWrapUrl = "http://www.w3.org/2001/04/xmlenc#kw-aes192";
        public const string XmlEncAES192Url = "http://www.w3.org/2001/04/xmlenc#aes192-cbc";
        public const string XmlEncAES256KeyWrapUrl = "http://www.w3.org/2001/04/xmlenc#kw-aes256";
        public const string XmlEncAES256Url = "http://www.w3.org/2001/04/xmlenc#aes256-cbc";
        public const string XmlEncDESUrl = "http://www.w3.org/2001/04/xmlenc#des-cbc";
        public const string XmlEncElementContentUrl = "http://www.w3.org/2001/04/xmlenc#Content";
        public const string XmlEncElementUrl = "http://www.w3.org/2001/04/xmlenc#Element";
        public const string XmlEncEncryptedKeyUrl = "http://www.w3.org/2001/04/xmlenc#EncryptedKey";
        public const string XmlEncNamespaceUrl = "http://www.w3.org/2001/04/xmlenc#";
        public const string XmlEncRSA15Url = "http://www.w3.org/2001/04/xmlenc#rsa-1_5";
        public const string XmlEncRSAOAEPUrl = "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p";
        public const string XmlEncSHA256Url = "http://www.w3.org/2001/04/xmlenc#sha256";
        public const string XmlEncSHA512Url = "http://www.w3.org/2001/04/xmlenc#sha512";
        public const string XmlEncTripleDESKeyWrapUrl = "http://www.w3.org/2001/04/xmlenc#kw-tripledes";
        public const string XmlEncTripleDESUrl = "http://www.w3.org/2001/04/xmlenc#tripledes-cbc";

        public EncryptedXml() : this(new XmlDocument())
        {
        }

        public EncryptedXml(XmlDocument document) : this(document, null)
        {
        }

        public EncryptedXml(XmlDocument document, Evidence evidence)
        {
            this.m_document = document;
            this.m_evidence = evidence;
            this.m_xmlResolver = null;
            this.m_padding = PaddingMode.ISO10126;
            this.m_mode = CipherMode.CBC;
            this.m_encoding = System.Text.Encoding.UTF8;
            this.m_keyNameMapping = new Hashtable(4);
        }

        public void AddKeyNameMapping(string keyName, object keyObject)
        {
            if (keyName == null)
            {
                throw new ArgumentNullException("keyName");
            }
            if (keyObject == null)
            {
                throw new ArgumentNullException("keyObject");
            }
            if (!(keyObject is SymmetricAlgorithm) && !(keyObject is RSA))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_NotSupportedCryptographicTransform"));
            }
            this.m_keyNameMapping.Add(keyName, keyObject);
        }

        public void ClearKeyNameMappings()
        {
            this.m_keyNameMapping.Clear();
        }

        public byte[] DecryptData(EncryptedData encryptedData, SymmetricAlgorithm symmetricAlgorithm)
        {
            if (encryptedData == null)
            {
                throw new ArgumentNullException("encryptedData");
            }
            if (symmetricAlgorithm == null)
            {
                throw new ArgumentNullException("symmetricAlgorithm");
            }
            byte[] cipherValue = this.GetCipherValue(encryptedData.CipherData);
            CipherMode mode = symmetricAlgorithm.Mode;
            PaddingMode padding = symmetricAlgorithm.Padding;
            byte[] iV = symmetricAlgorithm.IV;
            byte[] decryptionIV = null;
            if (this.m_mode != CipherMode.ECB)
            {
                decryptionIV = this.GetDecryptionIV(encryptedData, null);
            }
            byte[] buffer4 = null;
            try
            {
                int inputOffset = 0;
                if (decryptionIV != null)
                {
                    symmetricAlgorithm.IV = decryptionIV;
                    inputOffset = decryptionIV.Length;
                }
                symmetricAlgorithm.Mode = this.m_mode;
                symmetricAlgorithm.Padding = this.m_padding;
                buffer4 = symmetricAlgorithm.CreateDecryptor().TransformFinalBlock(cipherValue, inputOffset, cipherValue.Length - inputOffset);
            }
            finally
            {
                symmetricAlgorithm.Mode = mode;
                symmetricAlgorithm.Padding = padding;
                symmetricAlgorithm.IV = iV;
            }
            return buffer4;
        }

        public void DecryptDocument()
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(this.m_document.NameTable);
            nsmgr.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
            XmlNodeList list = this.m_document.SelectNodes("//enc:EncryptedData", nsmgr);
            if (list != null)
            {
                foreach (XmlNode node in list)
                {
                    XmlElement element = node as XmlElement;
                    EncryptedData encryptedData = new EncryptedData();
                    encryptedData.LoadXml(element);
                    SymmetricAlgorithm decryptionKey = this.GetDecryptionKey(encryptedData, null);
                    if (decryptionKey == null)
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_MissingDecryptionKey"));
                    }
                    byte[] decryptedData = this.DecryptData(encryptedData, decryptionKey);
                    this.ReplaceData(element, decryptedData);
                }
            }
        }

        public virtual byte[] DecryptEncryptedKey(EncryptedKey encryptedKey)
        {
            if (encryptedKey == null)
            {
                throw new ArgumentNullException("encryptedKey");
            }
            if (encryptedKey.KeyInfo != null)
            {
                IEnumerator enumerator = encryptedKey.KeyInfo.GetEnumerator();
                EncryptedKey key2 = null;
                bool useOAEP = false;
                while (enumerator.MoveNext())
                {
                    KeyInfoName current = enumerator.Current as KeyInfoName;
                    if (current != null)
                    {
                        string str = current.Value;
                        object obj2 = this.m_keyNameMapping[str];
                        if (obj2 != null)
                        {
                            if (obj2 is SymmetricAlgorithm)
                            {
                                return DecryptKey(encryptedKey.CipherData.CipherValue, (SymmetricAlgorithm) obj2);
                            }
                            useOAEP = (encryptedKey.EncryptionMethod != null) && (encryptedKey.EncryptionMethod.KeyAlgorithm == "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p");
                            return DecryptKey(encryptedKey.CipherData.CipherValue, (RSA) obj2, useOAEP);
                        }
                        break;
                    }
                    KeyInfoX509Data data = enumerator.Current as KeyInfoX509Data;
                    if (data != null)
                    {
                        X509Certificate2Enumerator enumerator2 = System.Security.Cryptography.Xml.Utils.BuildBagOfCerts(data, CertUsageType.Decryption).GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            RSA privateKey = enumerator2.Current.PrivateKey as RSA;
                            if (privateKey != null)
                            {
                                useOAEP = (encryptedKey.EncryptionMethod != null) && (encryptedKey.EncryptionMethod.KeyAlgorithm == "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p");
                                return DecryptKey(encryptedKey.CipherData.CipherValue, privateKey, useOAEP);
                            }
                        }
                        break;
                    }
                    KeyInfoRetrievalMethod method = enumerator.Current as KeyInfoRetrievalMethod;
                    if (method != null)
                    {
                        string idValue = System.Security.Cryptography.Xml.Utils.ExtractIdFromLocalUri(method.Uri);
                        key2 = new EncryptedKey();
                        key2.LoadXml(this.GetIdElement(this.m_document, idValue));
                        return this.DecryptEncryptedKey(key2);
                    }
                    KeyInfoEncryptedKey key = enumerator.Current as KeyInfoEncryptedKey;
                    if (key != null)
                    {
                        key2 = key.EncryptedKey;
                        byte[] buffer = this.DecryptEncryptedKey(key2);
                        if (buffer != null)
                        {
                            SymmetricAlgorithm symmetricAlgorithm = (SymmetricAlgorithm) CryptoConfig.CreateFromName(encryptedKey.EncryptionMethod.KeyAlgorithm);
                            symmetricAlgorithm.Key = buffer;
                            return DecryptKey(encryptedKey.CipherData.CipherValue, symmetricAlgorithm);
                        }
                    }
                }
            }
            return null;
        }

        public static byte[] DecryptKey(byte[] keyData, SymmetricAlgorithm symmetricAlgorithm)
        {
            if (keyData == null)
            {
                throw new ArgumentNullException("keyData");
            }
            if (symmetricAlgorithm == null)
            {
                throw new ArgumentNullException("symmetricAlgorithm");
            }
            if (symmetricAlgorithm is TripleDES)
            {
                return SymmetricKeyWrap.TripleDESKeyWrapDecrypt(symmetricAlgorithm.Key, keyData);
            }
            if (!(symmetricAlgorithm is Rijndael) && !(symmetricAlgorithm is Aes))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_NotSupportedCryptographicTransform"));
            }
            return SymmetricKeyWrap.AESKeyWrapDecrypt(symmetricAlgorithm.Key, keyData);
        }

        public static byte[] DecryptKey(byte[] keyData, RSA rsa, bool useOAEP)
        {
            if (keyData == null)
            {
                throw new ArgumentNullException("keyData");
            }
            if (rsa == null)
            {
                throw new ArgumentNullException("rsa");
            }
            if (useOAEP)
            {
                RSAOAEPKeyExchangeDeformatter deformatter = new RSAOAEPKeyExchangeDeformatter(rsa);
                return deformatter.DecryptKeyExchange(keyData);
            }
            RSAPKCS1KeyExchangeDeformatter deformatter2 = new RSAPKCS1KeyExchangeDeformatter(rsa);
            return deformatter2.DecryptKeyExchange(keyData);
        }

        private void DownloadCipherValue(CipherData cipherData, out Stream inputStream, out Stream decInputStream, out WebResponse response)
        {
            SecurityManager.GetStandardSandbox(this.m_evidence).PermitOnly();
            WebRequest request = WebRequest.Create(cipherData.CipherReference.Uri);
            if (request == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UriNotResolved"), cipherData.CipherReference.Uri);
            }
            response = request.GetResponse();
            if (response == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UriNotResolved"), cipherData.CipherReference.Uri);
            }
            inputStream = response.GetResponseStream();
            if (inputStream == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UriNotResolved"), cipherData.CipherReference.Uri);
            }
            decInputStream = cipherData.CipherReference.TransformChain.TransformToOctetStream(inputStream, this.m_xmlResolver, cipherData.CipherReference.Uri);
        }

        public EncryptedData Encrypt(XmlElement inputElement, X509Certificate2 certificate)
        {
            if (inputElement == null)
            {
                throw new ArgumentNullException("inputElement");
            }
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            if (System.Security.Cryptography.X509Certificates.X509Utils.OidToAlgId(certificate.PublicKey.Oid.Value) != 0xa400)
            {
                throw new NotSupportedException(SecurityResources.GetResourceString("NotSupported_KeyAlgorithm"));
            }
            EncryptedData data = new EncryptedData {
                Type = "http://www.w3.org/2001/04/xmlenc#Element",
                EncryptionMethod = new EncryptionMethod("http://www.w3.org/2001/04/xmlenc#aes256-cbc")
            };
            EncryptedKey encryptedKey = new EncryptedKey {
                EncryptionMethod = new EncryptionMethod("http://www.w3.org/2001/04/xmlenc#rsa-1_5")
            };
            encryptedKey.KeyInfo.AddClause(new KeyInfoX509Data(certificate));
            RijndaelManaged symmetricAlgorithm = new RijndaelManaged();
            encryptedKey.CipherData.CipherValue = EncryptKey(symmetricAlgorithm.Key, certificate.PublicKey.Key as RSA, false);
            KeyInfoEncryptedKey clause = new KeyInfoEncryptedKey(encryptedKey);
            data.KeyInfo.AddClause(clause);
            data.CipherData.CipherValue = this.EncryptData(inputElement, symmetricAlgorithm, false);
            return data;
        }

        public EncryptedData Encrypt(XmlElement inputElement, string keyName)
        {
            if (inputElement == null)
            {
                throw new ArgumentNullException("inputElement");
            }
            if (keyName == null)
            {
                throw new ArgumentNullException("keyName");
            }
            object obj2 = null;
            if (this.m_keyNameMapping != null)
            {
                obj2 = this.m_keyNameMapping[keyName];
            }
            if (obj2 == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_MissingEncryptionKey"));
            }
            SymmetricAlgorithm symmetricAlgorithm = obj2 as SymmetricAlgorithm;
            RSA rsa = obj2 as RSA;
            EncryptedData data = new EncryptedData {
                Type = "http://www.w3.org/2001/04/xmlenc#Element",
                EncryptionMethod = new EncryptionMethod("http://www.w3.org/2001/04/xmlenc#aes256-cbc")
            };
            string algorithm = null;
            if (symmetricAlgorithm == null)
            {
                algorithm = "http://www.w3.org/2001/04/xmlenc#rsa-1_5";
            }
            else if (symmetricAlgorithm is TripleDES)
            {
                algorithm = "http://www.w3.org/2001/04/xmlenc#kw-tripledes";
            }
            else
            {
                if (!(symmetricAlgorithm is Rijndael) && !(symmetricAlgorithm is Aes))
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_NotSupportedCryptographicTransform"));
                }
                switch (symmetricAlgorithm.KeySize)
                {
                    case 0x80:
                        algorithm = "http://www.w3.org/2001/04/xmlenc#kw-aes128";
                        break;

                    case 0xc0:
                        algorithm = "http://www.w3.org/2001/04/xmlenc#kw-aes192";
                        break;

                    case 0x100:
                        algorithm = "http://www.w3.org/2001/04/xmlenc#kw-aes256";
                        break;
                }
            }
            EncryptedKey encryptedKey = new EncryptedKey {
                EncryptionMethod = new EncryptionMethod(algorithm)
            };
            encryptedKey.KeyInfo.AddClause(new KeyInfoName(keyName));
            RijndaelManaged managed = new RijndaelManaged();
            encryptedKey.CipherData.CipherValue = (symmetricAlgorithm == null) ? EncryptKey(managed.Key, rsa, false) : EncryptKey(managed.Key, symmetricAlgorithm);
            KeyInfoEncryptedKey clause = new KeyInfoEncryptedKey(encryptedKey);
            data.KeyInfo.AddClause(clause);
            data.CipherData.CipherValue = this.EncryptData(inputElement, managed, false);
            return data;
        }

        public byte[] EncryptData(byte[] plaintext, SymmetricAlgorithm symmetricAlgorithm)
        {
            if (plaintext == null)
            {
                throw new ArgumentNullException("plaintext");
            }
            if (symmetricAlgorithm == null)
            {
                throw new ArgumentNullException("symmetricAlgorithm");
            }
            CipherMode mode = symmetricAlgorithm.Mode;
            PaddingMode padding = symmetricAlgorithm.Padding;
            byte[] src = null;
            try
            {
                symmetricAlgorithm.Mode = this.m_mode;
                symmetricAlgorithm.Padding = this.m_padding;
                src = symmetricAlgorithm.CreateEncryptor().TransformFinalBlock(plaintext, 0, plaintext.Length);
            }
            finally
            {
                symmetricAlgorithm.Mode = mode;
                symmetricAlgorithm.Padding = padding;
            }
            byte[] dst = null;
            if (this.m_mode == CipherMode.ECB)
            {
                return src;
            }
            byte[] iV = symmetricAlgorithm.IV;
            dst = new byte[src.Length + iV.Length];
            Buffer.BlockCopy(iV, 0, dst, 0, iV.Length);
            Buffer.BlockCopy(src, 0, dst, iV.Length, src.Length);
            return dst;
        }

        public byte[] EncryptData(XmlElement inputElement, SymmetricAlgorithm symmetricAlgorithm, bool content)
        {
            if (inputElement == null)
            {
                throw new ArgumentNullException("inputElement");
            }
            if (symmetricAlgorithm == null)
            {
                throw new ArgumentNullException("symmetricAlgorithm");
            }
            byte[] plaintext = content ? this.m_encoding.GetBytes(inputElement.InnerXml) : this.m_encoding.GetBytes(inputElement.OuterXml);
            return this.EncryptData(plaintext, symmetricAlgorithm);
        }

        public static byte[] EncryptKey(byte[] keyData, SymmetricAlgorithm symmetricAlgorithm)
        {
            if (keyData == null)
            {
                throw new ArgumentNullException("keyData");
            }
            if (symmetricAlgorithm == null)
            {
                throw new ArgumentNullException("symmetricAlgorithm");
            }
            if (symmetricAlgorithm is TripleDES)
            {
                return SymmetricKeyWrap.TripleDESKeyWrapEncrypt(symmetricAlgorithm.Key, keyData);
            }
            if (!(symmetricAlgorithm is Rijndael) && !(symmetricAlgorithm is Aes))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_NotSupportedCryptographicTransform"));
            }
            return SymmetricKeyWrap.AESKeyWrapEncrypt(symmetricAlgorithm.Key, keyData);
        }

        public static byte[] EncryptKey(byte[] keyData, RSA rsa, bool useOAEP)
        {
            if (keyData == null)
            {
                throw new ArgumentNullException("keyData");
            }
            if (rsa == null)
            {
                throw new ArgumentNullException("rsa");
            }
            if (useOAEP)
            {
                RSAOAEPKeyExchangeFormatter formatter = new RSAOAEPKeyExchangeFormatter(rsa);
                return formatter.CreateKeyExchange(keyData);
            }
            RSAPKCS1KeyExchangeFormatter formatter2 = new RSAPKCS1KeyExchangeFormatter(rsa);
            return formatter2.CreateKeyExchange(keyData);
        }

        private byte[] GetCipherValue(CipherData cipherData)
        {
            if (cipherData == null)
            {
                throw new ArgumentNullException("cipherData");
            }
            WebResponse response = null;
            Stream input = null;
            if (cipherData.CipherValue != null)
            {
                return cipherData.CipherValue;
            }
            if (cipherData.CipherReference == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_MissingCipherData"));
            }
            if (cipherData.CipherReference.CipherValue != null)
            {
                return cipherData.CipherReference.CipherValue;
            }
            Stream decInputStream = null;
            if (cipherData.CipherReference.Uri.Length == 0)
            {
                string baseUri = (this.m_document == null) ? null : this.m_document.BaseURI;
                decInputStream = cipherData.CipherReference.TransformChain.TransformToOctetStream(this.m_document, this.m_xmlResolver, baseUri);
            }
            else if (cipherData.CipherReference.Uri[0] == '#')
            {
                string idValue = System.Security.Cryptography.Xml.Utils.ExtractIdFromLocalUri(cipherData.CipherReference.Uri);
                input = new MemoryStream(this.m_encoding.GetBytes(this.GetIdElement(this.m_document, idValue).OuterXml));
                string str3 = (this.m_document == null) ? null : this.m_document.BaseURI;
                decInputStream = cipherData.CipherReference.TransformChain.TransformToOctetStream(input, this.m_xmlResolver, str3);
            }
            else
            {
                this.DownloadCipherValue(cipherData, out input, out decInputStream, out response);
            }
            byte[] buffer = null;
            using (MemoryStream stream3 = new MemoryStream())
            {
                System.Security.Cryptography.Xml.Utils.Pump(decInputStream, stream3);
                buffer = stream3.ToArray();
                if (response != null)
                {
                    response.Close();
                }
                if (input != null)
                {
                    input.Close();
                }
                decInputStream.Close();
            }
            cipherData.CipherReference.CipherValue = buffer;
            return buffer;
        }

        public virtual byte[] GetDecryptionIV(EncryptedData encryptedData, string symmetricAlgorithmUri)
        {
            byte[] buffer;
            if (encryptedData == null)
            {
                throw new ArgumentNullException("encryptedData");
            }
            int num = 0;
            if (symmetricAlgorithmUri == null)
            {
                if (encryptedData.EncryptionMethod == null)
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_MissingAlgorithm"));
                }
                symmetricAlgorithmUri = encryptedData.EncryptionMethod.KeyAlgorithm;
            }
            string str = symmetricAlgorithmUri;
            if (str != null)
            {
                if (!(str == "http://www.w3.org/2001/04/xmlenc#des-cbc") && !(str == "http://www.w3.org/2001/04/xmlenc#tripledes-cbc"))
                {
                    if (((str == "http://www.w3.org/2001/04/xmlenc#aes128-cbc") || (str == "http://www.w3.org/2001/04/xmlenc#aes192-cbc")) || (str == "http://www.w3.org/2001/04/xmlenc#aes256-cbc"))
                    {
                        num = 0x10;
                        goto Label_0099;
                    }
                }
                else
                {
                    num = 8;
                    goto Label_0099;
                }
            }
            throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UriNotSupported"));
        Label_0099:
            buffer = new byte[num];
            Buffer.BlockCopy(this.GetCipherValue(encryptedData.CipherData), 0, buffer, 0, buffer.Length);
            return buffer;
        }

        public virtual SymmetricAlgorithm GetDecryptionKey(EncryptedData encryptedData, string symmetricAlgorithmUri)
        {
            if (encryptedData == null)
            {
                throw new ArgumentNullException("encryptedData");
            }
            if (encryptedData.KeyInfo == null)
            {
                return null;
            }
            IEnumerator enumerator = encryptedData.KeyInfo.GetEnumerator();
            EncryptedKey encryptedKey = null;
            while (enumerator.MoveNext())
            {
                KeyInfoName current = enumerator.Current as KeyInfoName;
                if (current != null)
                {
                    string str = current.Value;
                    if (((SymmetricAlgorithm) this.m_keyNameMapping[str]) != null)
                    {
                        return (SymmetricAlgorithm) this.m_keyNameMapping[str];
                    }
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(this.m_document.NameTable);
                    nsmgr.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
                    XmlNodeList list = this.m_document.SelectNodes("//enc:EncryptedKey", nsmgr);
                    if (list != null)
                    {
                        foreach (XmlNode node in list)
                        {
                            XmlElement element = node as XmlElement;
                            EncryptedKey key3 = new EncryptedKey();
                            key3.LoadXml(element);
                            if ((key3.CarriedKeyName == str) && (key3.Recipient == this.Recipient))
                            {
                                encryptedKey = key3;
                                break;
                            }
                        }
                    }
                    break;
                }
                KeyInfoRetrievalMethod method = enumerator.Current as KeyInfoRetrievalMethod;
                if (method != null)
                {
                    string idValue = System.Security.Cryptography.Xml.Utils.ExtractIdFromLocalUri(method.Uri);
                    encryptedKey = new EncryptedKey();
                    encryptedKey.LoadXml(this.GetIdElement(this.m_document, idValue));
                    break;
                }
                KeyInfoEncryptedKey key = enumerator.Current as KeyInfoEncryptedKey;
                if (key != null)
                {
                    encryptedKey = key.EncryptedKey;
                    break;
                }
            }
            if (encryptedKey == null)
            {
                return null;
            }
            if (symmetricAlgorithmUri == null)
            {
                if (encryptedData.EncryptionMethod == null)
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_MissingAlgorithm"));
                }
                symmetricAlgorithmUri = encryptedData.EncryptionMethod.KeyAlgorithm;
            }
            byte[] buffer = this.DecryptEncryptedKey(encryptedKey);
            if (buffer == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_MissingDecryptionKey"));
            }
            SymmetricAlgorithm algorithm = (SymmetricAlgorithm) CryptoConfig.CreateFromName(symmetricAlgorithmUri);
            algorithm.Key = buffer;
            return algorithm;
        }

        public virtual XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            if (document == null)
            {
                return null;
            }
            XmlElement elementById = null;
            elementById = document.GetElementById(idValue);
            if (elementById != null)
            {
                return elementById;
            }
            elementById = document.SelectSingleNode("//*[@Id=\"" + idValue + "\"]") as XmlElement;
            if (elementById != null)
            {
                return elementById;
            }
            elementById = document.SelectSingleNode("//*[@id=\"" + idValue + "\"]") as XmlElement;
            if (elementById != null)
            {
                return elementById;
            }
            return (document.SelectSingleNode("//*[@ID=\"" + idValue + "\"]") as XmlElement);
        }

        public void ReplaceData(XmlElement inputElement, byte[] decryptedData)
        {
            if (inputElement == null)
            {
                throw new ArgumentNullException("inputElement");
            }
            if (decryptedData == null)
            {
                throw new ArgumentNullException("decryptedData");
            }
            XmlNode parentNode = inputElement.ParentNode;
            if (parentNode.NodeType == XmlNodeType.Document)
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(this.m_encoding.GetString(decryptedData));
                XmlNode newChild = inputElement.OwnerDocument.ImportNode(document.DocumentElement, true);
                parentNode.RemoveChild(inputElement);
                parentNode.AppendChild(newChild);
            }
            else
            {
                XmlNode node3 = parentNode.OwnerDocument.CreateElement(parentNode.Prefix, parentNode.LocalName, parentNode.NamespaceURI);
                try
                {
                    parentNode.AppendChild(node3);
                    node3.InnerXml = this.m_encoding.GetString(decryptedData);
                    XmlNode firstChild = node3.FirstChild;
                    XmlNode nextSibling = inputElement.NextSibling;
                    XmlNode node6 = null;
                    while (firstChild != null)
                    {
                        node6 = firstChild.NextSibling;
                        parentNode.InsertBefore(firstChild, nextSibling);
                        firstChild = node6;
                    }
                }
                finally
                {
                    parentNode.RemoveChild(node3);
                }
                parentNode.RemoveChild(inputElement);
            }
        }

        public static void ReplaceElement(XmlElement inputElement, EncryptedData encryptedData, bool content)
        {
            if (inputElement == null)
            {
                throw new ArgumentNullException("inputElement");
            }
            if (encryptedData == null)
            {
                throw new ArgumentNullException("encryptedData");
            }
            XmlElement xml = encryptedData.GetXml(inputElement.OwnerDocument);
            switch (content)
            {
                case false:
                    inputElement.ParentNode.ReplaceChild(xml, inputElement);
                    return;

                case true:
                    System.Security.Cryptography.Xml.Utils.RemoveAllChildren(inputElement);
                    inputElement.AppendChild(xml);
                    return;
            }
        }

        public Evidence DocumentEvidence
        {
            get
            {
                return this.m_evidence;
            }
            set
            {
                this.m_evidence = value;
            }
        }

        public System.Text.Encoding Encoding
        {
            get
            {
                return this.m_encoding;
            }
            set
            {
                this.m_encoding = value;
            }
        }

        public CipherMode Mode
        {
            get
            {
                return this.m_mode;
            }
            set
            {
                this.m_mode = value;
            }
        }

        public PaddingMode Padding
        {
            get
            {
                return this.m_padding;
            }
            set
            {
                this.m_padding = value;
            }
        }

        public string Recipient
        {
            get
            {
                if (this.m_recipient == null)
                {
                    this.m_recipient = string.Empty;
                }
                return this.m_recipient;
            }
            set
            {
                this.m_recipient = value;
            }
        }

        public XmlResolver Resolver
        {
            get
            {
                return this.m_xmlResolver;
            }
            set
            {
                this.m_xmlResolver = value;
            }
        }
    }
}

