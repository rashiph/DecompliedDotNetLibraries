namespace System.IdentityModel
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Xml;

    internal abstract class SignedInfo : ISecurityElement
    {
        private readonly ExclusiveCanonicalizationTransform canonicalizationMethodElement = new ExclusiveCanonicalizationTransform(true);
        private MemoryStream canonicalStream;
        private System.IdentityModel.DictionaryManager dictionaryManager;
        private string id;
        private ISignatureReaderProvider readerProvider;
        private SignatureResourcePool resourcePool;
        private bool sendSide = true;
        private ElementWithAlgorithmAttribute signatureMethodElement;
        private object signatureReaderProviderCallbackContext;

        protected SignedInfo(System.IdentityModel.DictionaryManager dictionaryManager)
        {
            if (dictionaryManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryManager");
            }
            this.signatureMethodElement = new ElementWithAlgorithmAttribute(dictionaryManager.XmlSignatureDictionary.SignatureMethod);
            this.dictionaryManager = dictionaryManager;
        }

        protected virtual void ComputeHash(HashStream hashStream)
        {
            if (this.sendSide)
            {
                XmlDictionaryWriter writer = this.ResourcePool.TakeUtf8Writer();
                writer.StartCanonicalization(hashStream, false, null);
                this.WriteTo(writer, this.dictionaryManager);
                writer.EndCanonicalization();
            }
            else if (this.canonicalStream != null)
            {
                this.canonicalStream.WriteTo(hashStream);
            }
            else
            {
                if (this.readerProvider == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("InclusiveNamespacePrefixRequiresSignatureReader")));
                }
                XmlDictionaryReader reader = this.readerProvider.GetReader(this.signatureReaderProviderCallbackContext);
                if (!reader.CanCanonicalize)
                {
                    MemoryStream stream = new MemoryStream();
                    XmlDictionaryWriter writer2 = XmlDictionaryWriter.CreateBinaryWriter(stream, this.DictionaryManager.ParentDictionary);
                    string[] inclusivePrefixes = this.GetInclusivePrefixes();
                    if (inclusivePrefixes != null)
                    {
                        writer2.WriteStartElement("a");
                        for (int i = 0; i < inclusivePrefixes.Length; i++)
                        {
                            string namespaceForInclusivePrefix = this.GetNamespaceForInclusivePrefix(inclusivePrefixes[i]);
                            if (namespaceForInclusivePrefix != null)
                            {
                                writer2.WriteXmlnsAttribute(inclusivePrefixes[i], namespaceForInclusivePrefix);
                            }
                        }
                    }
                    reader.MoveToContent();
                    writer2.WriteNode(reader, false);
                    if (inclusivePrefixes != null)
                    {
                        writer2.WriteEndElement();
                    }
                    writer2.Flush();
                    byte[] buffer = stream.ToArray();
                    int length = (int) stream.Length;
                    writer2.Close();
                    reader.Close();
                    reader = XmlDictionaryReader.CreateBinaryReader(buffer, 0, length, this.DictionaryManager.ParentDictionary, XmlDictionaryReaderQuotas.Max);
                    if (inclusivePrefixes != null)
                    {
                        reader.ReadStartElement("a");
                    }
                }
                reader.ReadStartElement(this.dictionaryManager.XmlSignatureDictionary.Signature, this.dictionaryManager.XmlSignatureDictionary.Namespace);
                reader.MoveToStartElement(this.dictionaryManager.XmlSignatureDictionary.SignedInfo, this.dictionaryManager.XmlSignatureDictionary.Namespace);
                reader.StartCanonicalization(hashStream, false, this.GetInclusivePrefixes());
                reader.Skip();
                reader.EndCanonicalization();
                reader.Close();
            }
        }

        public void ComputeHash(HashAlgorithm algorithm)
        {
            if (this.CanonicalizationMethod != "http://www.w3.org/2001/10/xml-exc-c14n#")
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedTransformAlgorithm")));
            }
            HashStream hashStream = this.ResourcePool.TakeHashStream(algorithm);
            this.ComputeHash(hashStream);
            hashStream.FlushHash();
        }

        public abstract void ComputeReferenceDigests();
        public abstract void EnsureAllReferencesVerified();
        public void EnsureDigestValidity(string id, object resolvedXmlSource)
        {
            if (!this.EnsureDigestValidityIfIdMatches(id, resolvedXmlSource))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("RequiredTargetNotSigned", new object[] { id })));
            }
        }

        public abstract bool EnsureDigestValidityIfIdMatches(string id, object resolvedXmlSource);
        protected string[] GetInclusivePrefixes()
        {
            return this.canonicalizationMethodElement.GetInclusivePrefixes();
        }

        protected virtual string GetNamespaceForInclusivePrefix(string prefix)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public virtual bool HasUnverifiedReference(string id)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        protected void ReadCanonicalizationMethod(XmlDictionaryReader reader, System.IdentityModel.DictionaryManager dictionaryManager)
        {
            this.canonicalizationMethodElement.ReadFrom(reader, dictionaryManager);
        }

        public abstract void ReadFrom(XmlDictionaryReader reader, TransformFactory transformFactory, System.IdentityModel.DictionaryManager dictionaryManager);
        protected void ReadSignatureMethod(XmlDictionaryReader reader, System.IdentityModel.DictionaryManager dictionaryManager)
        {
            this.signatureMethodElement.ReadFrom(reader, dictionaryManager);
        }

        protected void WriteCanonicalizationMethod(XmlDictionaryWriter writer, System.IdentityModel.DictionaryManager dictionaryManager)
        {
            this.canonicalizationMethodElement.WriteTo(writer, dictionaryManager);
        }

        protected void WriteSignatureMethod(XmlDictionaryWriter writer, System.IdentityModel.DictionaryManager dictionaryManager)
        {
            this.signatureMethodElement.WriteTo(writer, dictionaryManager);
        }

        public abstract void WriteTo(XmlDictionaryWriter writer, System.IdentityModel.DictionaryManager dictionaryManager);

        public string CanonicalizationMethod
        {
            get
            {
                return this.canonicalizationMethodElement.Algorithm;
            }
            set
            {
                if (value != this.canonicalizationMethodElement.Algorithm)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("UnsupportedTransformAlgorithm")));
                }
            }
        }

        public XmlDictionaryString CanonicalizationMethodDictionaryString
        {
            set
            {
                if ((value != null) && (value.Value != this.canonicalizationMethodElement.Algorithm))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("UnsupportedTransformAlgorithm")));
                }
            }
        }

        protected MemoryStream CanonicalStream
        {
            get
            {
                return this.canonicalStream;
            }
            set
            {
                this.canonicalStream = value;
            }
        }

        protected System.IdentityModel.DictionaryManager DictionaryManager
        {
            get
            {
                return this.dictionaryManager;
            }
        }

        public bool HasId
        {
            get
            {
                return true;
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

        public ISignatureReaderProvider ReaderProvider
        {
            get
            {
                return this.readerProvider;
            }
            set
            {
                this.readerProvider = value;
            }
        }

        public abstract int ReferenceCount { get; }

        public SignatureResourcePool ResourcePool
        {
            get
            {
                if (this.resourcePool == null)
                {
                    this.resourcePool = new SignatureResourcePool();
                }
                return this.resourcePool;
            }
            set
            {
                this.resourcePool = value;
            }
        }

        protected bool SendSide
        {
            get
            {
                return this.sendSide;
            }
            set
            {
                this.sendSide = value;
            }
        }

        public string SignatureMethod
        {
            get
            {
                return this.signatureMethodElement.Algorithm;
            }
            set
            {
                this.signatureMethodElement.Algorithm = value;
            }
        }

        public XmlDictionaryString SignatureMethodDictionaryString
        {
            get
            {
                return this.signatureMethodElement.AlgorithmDictionaryString;
            }
            set
            {
                this.signatureMethodElement.AlgorithmDictionaryString = value;
            }
        }

        public object SignatureReaderProviderCallbackContext
        {
            get
            {
                return this.signatureReaderProviderCallbackContext;
            }
            set
            {
                this.signatureReaderProviderCallbackContext = value;
            }
        }
    }
}

