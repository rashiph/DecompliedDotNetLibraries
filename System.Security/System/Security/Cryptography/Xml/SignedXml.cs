namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class SignedXml
    {
        private byte[] _digestedSignedInfo;
        private bool bCacheValid;
        private bool m_bResolverSet;
        private XmlDocument m_containingDocument;
        internal XmlElement m_context;
        private System.Security.Cryptography.Xml.EncryptedXml m_exml;
        private IEnumerator m_keyInfoEnum;
        private int[] m_refLevelCache;
        private bool[] m_refProcessed;
        protected System.Security.Cryptography.Xml.Signature m_signature;
        private Func<SignedXml, bool> m_signatureFormatValidator;
        private AsymmetricAlgorithm m_signingKey;
        protected string m_strSigningKeyName;
        private X509Certificate2Collection m_x509Collection;
        private IEnumerator m_x509Enum;
        internal XmlResolver m_xmlResolver;
        public const string XmlDecryptionTransformUrl = "http://www.w3.org/2002/07/decrypt#XML";
        public const string XmlDsigBase64TransformUrl = "http://www.w3.org/2000/09/xmldsig#base64";
        public const string XmlDsigC14NTransformUrl = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
        public const string XmlDsigC14NWithCommentsTransformUrl = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments";
        public const string XmlDsigCanonicalizationUrl = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
        public const string XmlDsigCanonicalizationWithCommentsUrl = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments";
        public const string XmlDsigDSAUrl = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
        public const string XmlDsigEnvelopedSignatureTransformUrl = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";
        public const string XmlDsigExcC14NTransformUrl = "http://www.w3.org/2001/10/xml-exc-c14n#";
        public const string XmlDsigExcC14NWithCommentsTransformUrl = "http://www.w3.org/2001/10/xml-exc-c14n#WithComments";
        public const string XmlDsigHMACSHA1Url = "http://www.w3.org/2000/09/xmldsig#hmac-sha1";
        public const string XmlDsigMinimalCanonicalizationUrl = "http://www.w3.org/2000/09/xmldsig#minimal";
        private const string XmlDsigMoreHMACMD5Url = "http://www.w3.org/2001/04/xmldsig-more#hmac-md5";
        private const string XmlDsigMoreHMACRIPEMD160Url = "http://www.w3.org/2001/04/xmldsig-more#hmac-ripemd160";
        private const string XmlDsigMoreHMACSHA256Url = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
        private const string XmlDsigMoreHMACSHA384Url = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha384";
        private const string XmlDsigMoreHMACSHA512Url = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha512";
        public const string XmlDsigNamespaceUrl = "http://www.w3.org/2000/09/xmldsig#";
        public const string XmlDsigRSASHA1Url = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
        public const string XmlDsigSHA1Url = "http://www.w3.org/2000/09/xmldsig#sha1";
        public const string XmlDsigXPathTransformUrl = "http://www.w3.org/TR/1999/REC-xpath-19991116";
        public const string XmlDsigXsltTransformUrl = "http://www.w3.org/TR/1999/REC-xslt-19991116";
        public const string XmlLicenseTransformUrl = "urn:mpeg:mpeg21:2003:01-REL-R-NS:licenseTransform";

        public SignedXml()
        {
            this.m_signatureFormatValidator = new Func<SignedXml, bool>(SignedXml.DefaultSignatureFormatValidator);
            this.Initialize(null);
        }

        public SignedXml(XmlDocument document)
        {
            this.m_signatureFormatValidator = new Func<SignedXml, bool>(SignedXml.DefaultSignatureFormatValidator);
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            this.Initialize(document.DocumentElement);
        }

        public SignedXml(XmlElement elem)
        {
            this.m_signatureFormatValidator = new Func<SignedXml, bool>(SignedXml.DefaultSignatureFormatValidator);
            if (elem == null)
            {
                throw new ArgumentNullException("elem");
            }
            this.Initialize(elem);
        }

        public void AddObject(DataObject dataObject)
        {
            this.m_signature.AddObject(dataObject);
        }

        public void AddReference(Reference reference)
        {
            this.m_signature.SignedInfo.AddReference(reference);
        }

        private X509Certificate2Collection BuildBagOfCerts()
        {
            X509Certificate2Collection certificates = new X509Certificate2Collection();
            if (this.KeyInfo != null)
            {
                foreach (KeyInfoClause clause in this.KeyInfo)
                {
                    KeyInfoX509Data data = clause as KeyInfoX509Data;
                    if (data != null)
                    {
                        certificates.AddRange(System.Security.Cryptography.Xml.Utils.BuildBagOfCerts(data, CertUsageType.Verification));
                    }
                }
            }
            return certificates;
        }

        private void BuildDigestedReferences()
        {
            ArrayList references = this.SignedInfo.References;
            this.m_refProcessed = new bool[references.Count];
            this.m_refLevelCache = new int[references.Count];
            ReferenceLevelSortOrder comparer = new ReferenceLevelSortOrder {
                References = references
            };
            ArrayList list2 = new ArrayList();
            foreach (Reference reference in references)
            {
                list2.Add(reference);
            }
            list2.Sort(comparer);
            CanonicalXmlNodeList refList = new CanonicalXmlNodeList();
            foreach (DataObject obj2 in this.m_signature.ObjectList)
            {
                refList.Add(obj2.GetXml());
            }
            foreach (Reference reference2 in list2)
            {
                if (reference2.DigestMethod == null)
                {
                    reference2.DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
                }
                SignedXmlDebugLog.LogSigningReference(this, reference2);
                reference2.UpdateHashValue(this.m_containingDocument, refList);
                if (reference2.Id != null)
                {
                    refList.Add(reference2.GetXml());
                }
            }
        }

        private bool CheckDigestedReferences()
        {
            ArrayList references = this.m_signature.SignedInfo.References;
            for (int i = 0; i < references.Count; i++)
            {
                Reference reference = (Reference) references[i];
                SignedXmlDebugLog.LogVerifyReference(this, reference);
                byte[] actualHash = reference.CalculateHashValue(this.m_containingDocument, this.m_signature.ReferencedItems);
                SignedXmlDebugLog.LogVerifyReferenceHash(this, reference, actualHash, reference.DigestValue);
                if (actualHash.Length != reference.DigestValue.Length)
                {
                    return false;
                }
                byte[] buffer2 = actualHash;
                byte[] digestValue = reference.DigestValue;
                for (int j = 0; j < buffer2.Length; j++)
                {
                    if (buffer2[j] != digestValue[j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool CheckSignature()
        {
            AsymmetricAlgorithm algorithm;
            return this.CheckSignatureReturningKey(out algorithm);
        }

        public bool CheckSignature(AsymmetricAlgorithm key)
        {
            if (!this.CheckSignatureFormat())
            {
                return false;
            }
            if (!this.CheckSignedInfo(key))
            {
                SignedXmlDebugLog.LogVerificationFailure(this, SecurityResources.GetResourceString("Log_VerificationFailed_SignedInfo"));
                return false;
            }
            if (!this.CheckDigestedReferences())
            {
                SignedXmlDebugLog.LogVerificationFailure(this, SecurityResources.GetResourceString("Log_VerificationFailed_References"));
                return false;
            }
            SignedXmlDebugLog.LogVerificationResult(this, key, true);
            return true;
        }

        public bool CheckSignature(KeyedHashAlgorithm macAlg)
        {
            if (!this.CheckSignatureFormat())
            {
                return false;
            }
            if (!this.CheckSignedInfo(macAlg))
            {
                SignedXmlDebugLog.LogVerificationFailure(this, SecurityResources.GetResourceString("Log_VerificationFailed_SignedInfo"));
                return false;
            }
            if (!this.CheckDigestedReferences())
            {
                SignedXmlDebugLog.LogVerificationFailure(this, SecurityResources.GetResourceString("Log_VerificationFailed_References"));
                return false;
            }
            SignedXmlDebugLog.LogVerificationResult(this, macAlg, true);
            return true;
        }

        [SecuritySafeCritical, ComVisible(false)]
        public bool CheckSignature(X509Certificate2 certificate, bool verifySignatureOnly)
        {
            if (!this.CheckSignature(certificate.PublicKey.Key))
            {
                return false;
            }
            if (verifySignatureOnly)
            {
                SignedXmlDebugLog.LogVerificationResult(this, certificate, true);
                return true;
            }
            X509ExtensionEnumerator enumerator = certificate.Extensions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Extension current = enumerator.Current;
                if (string.Compare(current.Oid.Value, "2.5.29.15", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    X509KeyUsageExtension keyUsages = new X509KeyUsageExtension();
                    keyUsages.CopyFrom(current);
                    SignedXmlDebugLog.LogVerifyKeyUsage(this, certificate, keyUsages);
                    if (((keyUsages.KeyUsages & X509KeyUsageFlags.DigitalSignature) != X509KeyUsageFlags.None) || ((keyUsages.KeyUsages & X509KeyUsageFlags.NonRepudiation) != X509KeyUsageFlags.None))
                    {
                        break;
                    }
                    SignedXmlDebugLog.LogVerificationFailure(this, SecurityResources.GetResourceString("Log_VerificationFailed_X509KeyUsage"));
                    return false;
                }
            }
            X509Chain chain = new X509Chain();
            chain.ChainPolicy.ExtraStore.AddRange(this.BuildBagOfCerts());
            bool flag2 = chain.Build(certificate);
            SignedXmlDebugLog.LogVerifyX509Chain(this, chain, certificate);
            if (!flag2)
            {
                SignedXmlDebugLog.LogVerificationFailure(this, SecurityResources.GetResourceString("Log_VerificationFailed_X509Chain"));
                return false;
            }
            SignedXmlDebugLog.LogVerificationResult(this, certificate, true);
            return true;
        }

        private bool CheckSignatureFormat()
        {
            if (this.m_signatureFormatValidator == null)
            {
                return true;
            }
            SignedXmlDebugLog.LogBeginCheckSignatureFormat(this, this.m_signatureFormatValidator);
            bool result = this.m_signatureFormatValidator(this);
            SignedXmlDebugLog.LogFormatValidationResult(this, result);
            return result;
        }

        public bool CheckSignatureReturningKey(out AsymmetricAlgorithm signingKey)
        {
            SignedXmlDebugLog.LogBeginSignatureVerification(this, this.m_context);
            signingKey = null;
            bool verified = false;
            AsymmetricAlgorithm key = null;
            if (!this.CheckSignatureFormat())
            {
                return false;
            }
            do
            {
                key = this.GetPublicKey();
                if (key != null)
                {
                    verified = this.CheckSignature(key);
                    SignedXmlDebugLog.LogVerificationResult(this, key, verified);
                }
            }
            while ((key != null) && !verified);
            signingKey = key;
            return verified;
        }

        private bool CheckSignedInfo(AsymmetricAlgorithm key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            SignedXmlDebugLog.LogBeginCheckSignedInfo(this, this.m_signature.SignedInfo);
            SignatureDescription signatureDescription = CryptoConfig.CreateFromName(this.SignatureMethod) as SignatureDescription;
            if (signatureDescription == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_SignatureDescriptionNotCreated"));
            }
            Type c = Type.GetType(signatureDescription.KeyAlgorithm);
            Type type = key.GetType();
            if (((c != type) && !c.IsSubclassOf(type)) && !type.IsSubclassOf(c))
            {
                return false;
            }
            HashAlgorithm hash = signatureDescription.CreateDigest();
            if (hash == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_CreateHashAlgorithmFailed"));
            }
            byte[] actualHashValue = this.GetC14NDigest(hash);
            AsymmetricSignatureDeformatter asymmetricSignatureDeformatter = signatureDescription.CreateDeformatter(key);
            SignedXmlDebugLog.LogVerifySignedInfo(this, key, signatureDescription, hash, asymmetricSignatureDeformatter, actualHashValue, this.m_signature.SignatureValue);
            return asymmetricSignatureDeformatter.VerifySignature(actualHashValue, this.m_signature.SignatureValue);
        }

        private bool CheckSignedInfo(KeyedHashAlgorithm macAlg)
        {
            int hashSize;
            if (macAlg == null)
            {
                throw new ArgumentNullException("macAlg");
            }
            SignedXmlDebugLog.LogBeginCheckSignedInfo(this, this.m_signature.SignedInfo);
            if (this.m_signature.SignedInfo.SignatureLength == null)
            {
                hashSize = macAlg.HashSize;
            }
            else
            {
                hashSize = Convert.ToInt32(this.m_signature.SignedInfo.SignatureLength, (IFormatProvider) null);
            }
            if ((hashSize < 0) || (hashSize > macAlg.HashSize))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidSignatureLength"));
            }
            if ((hashSize % 8) != 0)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidSignatureLength2"));
            }
            if (this.m_signature.SignatureValue == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_SignatureValueRequired"));
            }
            if (this.m_signature.SignatureValue.Length != (hashSize / 8))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidSignatureLength"));
            }
            byte[] actualHashValue = this.GetC14NDigest(macAlg);
            SignedXmlDebugLog.LogVerifySignedInfo(this, macAlg, actualHashValue, this.m_signature.SignatureValue);
            for (int i = 0; i < this.m_signature.SignatureValue.Length; i++)
            {
                if (this.m_signature.SignatureValue[i] != actualHashValue[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void ComputeSignature()
        {
            SignedXmlDebugLog.LogBeginSignatureComputation(this, this.m_context);
            this.BuildDigestedReferences();
            AsymmetricAlgorithm signingKey = this.SigningKey;
            if (signingKey == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_LoadKeyFailed"));
            }
            if (this.SignedInfo.SignatureMethod == null)
            {
                if (!(signingKey is DSA))
                {
                    if (!(signingKey is RSA))
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_CreatedKeyFailed"));
                    }
                    if (this.SignedInfo.SignatureMethod == null)
                    {
                        this.SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
                    }
                }
                else
                {
                    this.SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
                }
            }
            SignatureDescription signatureDescription = CryptoConfig.CreateFromName(this.SignedInfo.SignatureMethod) as SignatureDescription;
            if (signatureDescription == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_SignatureDescriptionNotCreated"));
            }
            HashAlgorithm hash = signatureDescription.CreateDigest();
            if (hash == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_CreateHashAlgorithmFailed"));
            }
            this.GetC14NDigest(hash);
            AsymmetricSignatureFormatter asymmetricSignatureFormatter = signatureDescription.CreateFormatter(signingKey);
            SignedXmlDebugLog.LogSigning(this, signingKey, signatureDescription, hash, asymmetricSignatureFormatter);
            this.m_signature.SignatureValue = asymmetricSignatureFormatter.CreateSignature(hash);
        }

        public void ComputeSignature(KeyedHashAlgorithm macAlg)
        {
            int hashSize;
            if (macAlg == null)
            {
                throw new ArgumentNullException("macAlg");
            }
            HMAC hash = macAlg as HMAC;
            if (hash == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_SignatureMethodKeyMismatch"));
            }
            if (this.m_signature.SignedInfo.SignatureLength == null)
            {
                hashSize = hash.HashSize;
            }
            else
            {
                hashSize = Convert.ToInt32(this.m_signature.SignedInfo.SignatureLength, (IFormatProvider) null);
            }
            if ((hashSize < 0) || (hashSize > hash.HashSize))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidSignatureLength"));
            }
            if ((hashSize % 8) != 0)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidSignatureLength2"));
            }
            this.BuildDigestedReferences();
            switch (hash.HashName)
            {
                case "SHA1":
                    this.SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#hmac-sha1";
                    break;

                case "SHA256":
                    this.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
                    break;

                case "SHA384":
                    this.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha384";
                    break;

                case "SHA512":
                    this.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha512";
                    break;

                case "MD5":
                    this.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#hmac-md5";
                    break;

                case "RIPEMD160":
                    this.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#hmac-ripemd160";
                    break;

                default:
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_SignatureMethodKeyMismatch"));
            }
            byte[] src = this.GetC14NDigest(hash);
            SignedXmlDebugLog.LogSigning(this, hash);
            this.m_signature.SignatureValue = new byte[hashSize / 8];
            Buffer.BlockCopy(src, 0, this.m_signature.SignatureValue, 0, hashSize / 8);
        }

        private static bool DefaultSignatureFormatValidator(SignedXml signedXml)
        {
            if (signedXml.DoesSignatureUseTruncatedHmac())
            {
                return false;
            }
            return true;
        }

        private bool DoesSignatureUseTruncatedHmac()
        {
            if (this.SignedInfo.SignatureLength == null)
            {
                return false;
            }
            HMAC hmac = CryptoConfig.CreateFromName(this.SignatureMethod) as HMAC;
            if (hmac == null)
            {
                return false;
            }
            int result = 0;
            return (!int.TryParse(this.SignedInfo.SignatureLength, out result) || (result != hmac.HashSize));
        }

        private byte[] GetC14NDigest(HashAlgorithm hash)
        {
            if (!this.bCacheValid || !this.SignedInfo.CacheValid)
            {
                string securityUrl = (this.m_containingDocument == null) ? null : this.m_containingDocument.BaseURI;
                XmlResolver xmlResolver = this.m_bResolverSet ? this.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
                XmlDocument document = System.Security.Cryptography.Xml.Utils.PreProcessElementInput(this.SignedInfo.GetXml(), xmlResolver, securityUrl);
                CanonicalXmlNodeList namespaces = (this.m_context == null) ? null : System.Security.Cryptography.Xml.Utils.GetPropagatedAttributes(this.m_context);
                SignedXmlDebugLog.LogNamespacePropagation(this, namespaces);
                System.Security.Cryptography.Xml.Utils.AddNamespaces(document.DocumentElement, namespaces);
                Transform canonicalizationMethodObject = this.SignedInfo.CanonicalizationMethodObject;
                canonicalizationMethodObject.Resolver = xmlResolver;
                canonicalizationMethodObject.BaseURI = securityUrl;
                SignedXmlDebugLog.LogBeginCanonicalization(this, canonicalizationMethodObject);
                canonicalizationMethodObject.LoadInput(document);
                SignedXmlDebugLog.LogCanonicalizedOutput(this, canonicalizationMethodObject);
                this._digestedSignedInfo = canonicalizationMethodObject.GetDigestedOutput(hash);
                this.bCacheValid = true;
            }
            return this._digestedSignedInfo;
        }

        public virtual XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            if (document == null)
            {
                return null;
            }
            XmlElement elementById = document.GetElementById(idValue);
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

        private AsymmetricAlgorithm GetNextCertificatePublicKey()
        {
            while (this.m_x509Enum.MoveNext())
            {
                X509Certificate2 current = (X509Certificate2) this.m_x509Enum.Current;
                if (current != null)
                {
                    return current.PublicKey.Key;
                }
            }
            return null;
        }

        protected virtual AsymmetricAlgorithm GetPublicKey()
        {
            if (this.KeyInfo == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_KeyInfoRequired"));
            }
            if (this.m_x509Enum != null)
            {
                AsymmetricAlgorithm nextCertificatePublicKey = this.GetNextCertificatePublicKey();
                if (nextCertificatePublicKey != null)
                {
                    return nextCertificatePublicKey;
                }
            }
            if (this.m_keyInfoEnum == null)
            {
                this.m_keyInfoEnum = this.KeyInfo.GetEnumerator();
            }
            while (this.m_keyInfoEnum.MoveNext())
            {
                RSAKeyValue current = this.m_keyInfoEnum.Current as RSAKeyValue;
                if (current != null)
                {
                    return current.Key;
                }
                DSAKeyValue value3 = this.m_keyInfoEnum.Current as DSAKeyValue;
                if (value3 != null)
                {
                    return value3.Key;
                }
                KeyInfoX509Data data = this.m_keyInfoEnum.Current as KeyInfoX509Data;
                if (data != null)
                {
                    this.m_x509Collection = System.Security.Cryptography.Xml.Utils.BuildBagOfCerts(data, CertUsageType.Verification);
                    if (this.m_x509Collection.Count > 0)
                    {
                        this.m_x509Enum = this.m_x509Collection.GetEnumerator();
                        AsymmetricAlgorithm algorithm2 = this.GetNextCertificatePublicKey();
                        if (algorithm2 != null)
                        {
                            return algorithm2;
                        }
                    }
                }
            }
            return null;
        }

        private int GetReferenceLevel(int index, ArrayList references)
        {
            if (this.m_refProcessed[index])
            {
                return this.m_refLevelCache[index];
            }
            this.m_refProcessed[index] = true;
            Reference reference = (Reference) references[index];
            if (((reference.Uri == null) || (reference.Uri.Length == 0)) || ((reference.Uri.Length > 0) && (reference.Uri[0] != '#')))
            {
                this.m_refLevelCache[index] = 0;
                return 0;
            }
            if ((reference.Uri.Length <= 0) || (reference.Uri[0] != '#'))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidReference"));
            }
            string str = System.Security.Cryptography.Xml.Utils.ExtractIdFromLocalUri(reference.Uri);
            if (str == "xpointer(/)")
            {
                this.m_refLevelCache[index] = 0;
                return 0;
            }
            for (int i = 0; i < references.Count; i++)
            {
                if (((Reference) references[i]).Id == str)
                {
                    this.m_refLevelCache[index] = this.GetReferenceLevel(i, references) + 1;
                    return this.m_refLevelCache[index];
                }
            }
            this.m_refLevelCache[index] = 0;
            return 0;
        }

        public XmlElement GetXml()
        {
            if (this.m_containingDocument != null)
            {
                return this.m_signature.GetXml(this.m_containingDocument);
            }
            return this.m_signature.GetXml();
        }

        private void Initialize(XmlElement element)
        {
            this.m_containingDocument = (element == null) ? null : element.OwnerDocument;
            this.m_context = element;
            this.m_signature = new System.Security.Cryptography.Xml.Signature();
            this.m_signature.SignedXml = this;
            this.m_signature.SignedInfo = new System.Security.Cryptography.Xml.SignedInfo();
            this.m_signingKey = null;
        }

        public void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.m_signature.LoadXml(value);
            if (this.m_context == null)
            {
                this.m_context = value;
            }
            this.bCacheValid = false;
        }

        [ComVisible(false)]
        public System.Security.Cryptography.Xml.EncryptedXml EncryptedXml
        {
            get
            {
                if (this.m_exml == null)
                {
                    this.m_exml = new System.Security.Cryptography.Xml.EncryptedXml(this.m_containingDocument);
                }
                return this.m_exml;
            }
            set
            {
                this.m_exml = value;
            }
        }

        public System.Security.Cryptography.Xml.KeyInfo KeyInfo
        {
            get
            {
                return this.m_signature.KeyInfo;
            }
            set
            {
                this.m_signature.KeyInfo = value;
            }
        }

        [ComVisible(false)]
        public XmlResolver Resolver
        {
            set
            {
                this.m_xmlResolver = value;
                this.m_bResolverSet = true;
            }
        }

        internal bool ResolverSet
        {
            get
            {
                return this.m_bResolverSet;
            }
        }

        public System.Security.Cryptography.Xml.Signature Signature
        {
            get
            {
                return this.m_signature;
            }
        }

        public Func<SignedXml, bool> SignatureFormatValidator
        {
            get
            {
                return this.m_signatureFormatValidator;
            }
            set
            {
                this.m_signatureFormatValidator = value;
            }
        }

        public string SignatureLength
        {
            get
            {
                return this.m_signature.SignedInfo.SignatureLength;
            }
        }

        public string SignatureMethod
        {
            get
            {
                return this.m_signature.SignedInfo.SignatureMethod;
            }
        }

        public byte[] SignatureValue
        {
            get
            {
                return this.m_signature.SignatureValue;
            }
        }

        public System.Security.Cryptography.Xml.SignedInfo SignedInfo
        {
            get
            {
                return this.m_signature.SignedInfo;
            }
        }

        public AsymmetricAlgorithm SigningKey
        {
            get
            {
                return this.m_signingKey;
            }
            set
            {
                this.m_signingKey = value;
            }
        }

        public string SigningKeyName
        {
            get
            {
                return this.m_strSigningKeyName;
            }
            set
            {
                this.m_strSigningKeyName = value;
            }
        }

        private class ReferenceLevelSortOrder : IComparer
        {
            private ArrayList m_references;

            public int Compare(object a, object b)
            {
                Reference reference = a as Reference;
                Reference reference2 = b as Reference;
                int index = 0;
                int num2 = 0;
                int num3 = 0;
                foreach (Reference reference3 in this.References)
                {
                    if (reference3 == reference)
                    {
                        index = num3;
                    }
                    if (reference3 == reference2)
                    {
                        num2 = num3;
                    }
                    num3++;
                }
                int referenceLevel = reference.SignedXml.GetReferenceLevel(index, this.References);
                int num5 = reference2.SignedXml.GetReferenceLevel(num2, this.References);
                return referenceLevel.CompareTo(num5);
            }

            public ArrayList References
            {
                get
                {
                    return this.m_references;
                }
                set
                {
                    this.m_references = value;
                }
            }
        }
    }
}

