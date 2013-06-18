namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Xml;

    internal sealed class Reference
    {
        private DictionaryManager dictionaryManager;
        private ElementWithAlgorithmAttribute digestMethodElement;
        private DigestValueElement digestValueElement;
        private string id;
        private string prefix;
        private string referredId;
        private object resolvedXmlSource;
        private SignatureResourcePool resourcePool;
        private readonly System.IdentityModel.TransformChain transformChain;
        private string type;
        private string uri;
        private bool verified;

        public Reference(DictionaryManager dictionaryManager) : this(dictionaryManager, null)
        {
        }

        public Reference(DictionaryManager dictionaryManager, string uri) : this(dictionaryManager, uri, null)
        {
        }

        public Reference(DictionaryManager dictionaryManager, string uri, object resolvedXmlSource)
        {
            this.digestValueElement = new DigestValueElement();
            this.prefix = "";
            this.transformChain = new System.IdentityModel.TransformChain();
            if (dictionaryManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryManager");
            }
            this.dictionaryManager = dictionaryManager;
            this.digestMethodElement = new ElementWithAlgorithmAttribute(dictionaryManager.XmlSignatureDictionary.DigestMethod);
            this.uri = uri;
            this.resolvedXmlSource = resolvedXmlSource;
        }

        public void AddTransform(Transform transform)
        {
            this.transformChain.Add(transform);
        }

        public bool CheckDigest()
        {
            return CryptoHelper.IsEqual(this.ComputeDigest(), this.GetDigestValue());
        }

        public void ComputeAndSetDigest()
        {
            this.digestValueElement.Value = this.ComputeDigest();
        }

        public byte[] ComputeDigest()
        {
            if (this.transformChain.TransformCount == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("EmptyTransformChainNotSupported")));
            }
            if (this.resolvedXmlSource == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnableToResolveReferenceUriForSignature", new object[] { this.uri })));
            }
            return this.transformChain.TransformToDigest(this.resolvedXmlSource, this.ResourcePool, this.DigestMethod, this.dictionaryManager);
        }

        public void EnsureDigestValidity(string id, byte[] computedDigest)
        {
            if (!this.EnsureDigestValidityIfIdMatches(id, computedDigest))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("RequiredTargetNotSigned", new object[] { id })));
            }
        }

        public void EnsureDigestValidity(string id, object resolvedXmlSource)
        {
            if (!this.EnsureDigestValidityIfIdMatches(id, resolvedXmlSource))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("RequiredTargetNotSigned", new object[] { id })));
            }
        }

        public bool EnsureDigestValidityIfIdMatches(string id, byte[] computedDigest)
        {
            if (this.verified || (id != this.ExtractReferredId()))
            {
                return false;
            }
            if (!CryptoHelper.IsEqual(computedDigest, this.GetDigestValue()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("DigestVerificationFailedForReference", new object[] { this.uri })));
            }
            this.verified = true;
            return true;
        }

        public bool EnsureDigestValidityIfIdMatches(string id, object resolvedXmlSource)
        {
            if (this.verified || (id != this.ExtractReferredId()))
            {
                return false;
            }
            this.resolvedXmlSource = resolvedXmlSource;
            if (!this.CheckDigest())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("DigestVerificationFailedForReference", new object[] { this.uri })));
            }
            this.verified = true;
            return true;
        }

        public string ExtractReferredId()
        {
            if (this.referredId == null)
            {
                if (((this.uri == null) || (this.uri.Length < 2)) || (this.uri[0] != '#'))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnableToResolveReferenceUriForSignature", new object[] { this.uri })));
                }
                this.referredId = this.uri.Substring(1);
            }
            return this.referredId;
        }

        public byte[] GetDigestValue()
        {
            return this.digestValueElement.Value;
        }

        public void ReadFrom(XmlDictionaryReader reader, TransformFactory transformFactory, DictionaryManager dictionaryManager)
        {
            reader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.Reference, dictionaryManager.XmlSignatureDictionary.Namespace);
            this.prefix = reader.Prefix;
            this.Id = reader.GetAttribute("Id", null);
            this.Uri = reader.GetAttribute(dictionaryManager.XmlSignatureDictionary.URI, null);
            this.Type = reader.GetAttribute(dictionaryManager.XmlSignatureDictionary.Type, null);
            reader.Read();
            if (reader.IsStartElement(dictionaryManager.XmlSignatureDictionary.Transforms, dictionaryManager.XmlSignatureDictionary.Namespace))
            {
                this.transformChain.ReadFrom(reader, transformFactory, dictionaryManager);
            }
            this.digestMethodElement.ReadFrom(reader, dictionaryManager);
            this.digestValueElement.ReadFrom(reader, dictionaryManager);
            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public void SetResolvedXmlSource(object resolvedXmlSource)
        {
            this.resolvedXmlSource = resolvedXmlSource;
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            writer.WriteStartElement(this.prefix, dictionaryManager.XmlSignatureDictionary.Reference, dictionaryManager.XmlSignatureDictionary.Namespace);
            if (this.id != null)
            {
                writer.WriteAttributeString(dictionaryManager.UtilityDictionary.IdAttribute, null, this.id);
            }
            if (this.uri != null)
            {
                writer.WriteAttributeString(dictionaryManager.XmlSignatureDictionary.URI, null, this.uri);
            }
            if (this.type != null)
            {
                writer.WriteAttributeString(dictionaryManager.XmlSignatureDictionary.Type, null, this.type);
            }
            if (this.transformChain.TransformCount > 0)
            {
                this.transformChain.WriteTo(writer, dictionaryManager);
            }
            this.digestMethodElement.WriteTo(writer, dictionaryManager);
            this.digestValueElement.WriteTo(writer, dictionaryManager);
            writer.WriteEndElement();
        }

        public string DigestMethod
        {
            get
            {
                return this.digestMethodElement.Algorithm;
            }
            set
            {
                this.digestMethodElement.Algorithm = value;
            }
        }

        public XmlDictionaryString DigestMethodDictionaryString
        {
            get
            {
                return this.digestMethodElement.AlgorithmDictionaryString;
            }
            set
            {
                this.digestMethodElement.AlgorithmDictionaryString = value;
            }
        }

        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public SignatureResourcePool ResourcePool
        {
            get
            {
                return this.resourcePool;
            }
            set
            {
                this.resourcePool = value;
            }
        }

        public System.IdentityModel.TransformChain TransformChain
        {
            get
            {
                return this.transformChain;
            }
        }

        public int TransformCount
        {
            get
            {
                return this.transformChain.TransformCount;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public string Uri
        {
            get
            {
                return this.uri;
            }
            set
            {
                this.uri = value;
            }
        }

        public bool Verified
        {
            get
            {
                return this.verified;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DigestValueElement
        {
            private byte[] digestValue;
            private string digestText;
            private string prefix;
            internal byte[] Value
            {
                get
                {
                    return this.digestValue;
                }
                set
                {
                    this.digestValue = value;
                    this.digestText = null;
                }
            }
            public void ReadFrom(XmlDictionaryReader reader, DictionaryManager dictionaryManager)
            {
                reader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.DigestValue, dictionaryManager.XmlSignatureDictionary.Namespace);
                this.prefix = reader.Prefix;
                reader.Read();
                reader.MoveToContent();
                this.digestText = reader.ReadString();
                this.digestValue = Convert.FromBase64String(this.digestText.Trim());
                reader.MoveToContent();
                reader.ReadEndElement();
            }

            public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
            {
                writer.WriteStartElement(this.prefix ?? "", dictionaryManager.XmlSignatureDictionary.DigestValue, dictionaryManager.XmlSignatureDictionary.Namespace);
                if (this.digestText != null)
                {
                    writer.WriteString(this.digestText);
                }
                else
                {
                    writer.WriteBase64(this.digestValue, 0, this.digestValue.Length);
                }
                writer.WriteEndElement();
            }
        }
    }
}

