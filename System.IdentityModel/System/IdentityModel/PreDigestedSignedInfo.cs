namespace System.IdentityModel
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    internal sealed class PreDigestedSignedInfo : SignedInfo
    {
        private bool addEnvelopedSignatureTransform;
        private int count;
        private string digestMethod;
        private XmlDictionaryString digestMethodDictionaryString;
        private const int InitialReferenceArraySize = 8;
        private ReferenceEntry[] references;

        public PreDigestedSignedInfo(DictionaryManager dictionaryManager) : base(dictionaryManager)
        {
            this.references = new ReferenceEntry[8];
        }

        public PreDigestedSignedInfo(DictionaryManager dictionaryManager, string canonicalizationMethod, XmlDictionaryString canonicalizationMethodDictionaryString, string digestMethod, XmlDictionaryString digestMethodDictionaryString, string signatureMethod, XmlDictionaryString signatureMethodDictionaryString) : base(dictionaryManager)
        {
            this.references = new ReferenceEntry[8];
            base.CanonicalizationMethod = canonicalizationMethod;
            base.CanonicalizationMethodDictionaryString = canonicalizationMethodDictionaryString;
            this.DigestMethod = digestMethod;
            this.digestMethodDictionaryString = digestMethodDictionaryString;
            base.SignatureMethod = signatureMethod;
            base.SignatureMethodDictionaryString = signatureMethodDictionaryString;
        }

        public void AddReference(string id, byte[] digest)
        {
            if (this.count == this.references.Length)
            {
                ReferenceEntry[] destinationArray = new ReferenceEntry[this.references.Length * 2];
                Array.Copy(this.references, 0, destinationArray, 0, this.count);
                this.references = destinationArray;
            }
            this.references[this.count++].Set(id, digest);
        }

        protected override void ComputeHash(HashStream hashStream)
        {
            if (this.AddEnvelopedSignatureTransform)
            {
                base.ComputeHash(hashStream);
            }
            else
            {
                SignedInfoCanonicalFormWriter.Instance.WriteSignedInfoCanonicalForm(hashStream, base.SignatureMethod, this.DigestMethod, this.references, this.count, base.ResourcePool.TakeEncodingBuffer(), base.ResourcePool.TakeBase64Buffer());
            }
        }

        public override void ComputeReferenceDigests()
        {
        }

        public override void EnsureAllReferencesVerified()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public override bool EnsureDigestValidityIfIdMatches(string id, object resolvedXmlSource)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public override void ReadFrom(XmlDictionaryReader reader, TransformFactory transformFactory, DictionaryManager dictionaryManager)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public override void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            string prefix = "";
            XmlDictionaryString namespaceUri = dictionaryManager.XmlSignatureDictionary.Namespace;
            writer.WriteStartElement(prefix, dictionaryManager.XmlSignatureDictionary.SignedInfo, namespaceUri);
            if (base.Id != null)
            {
                writer.WriteAttributeString(dictionaryManager.UtilityDictionary.IdAttribute, null, base.Id);
            }
            base.WriteCanonicalizationMethod(writer, dictionaryManager);
            base.WriteSignatureMethod(writer, dictionaryManager);
            for (int i = 0; i < this.count; i++)
            {
                writer.WriteStartElement(prefix, dictionaryManager.XmlSignatureDictionary.Reference, namespaceUri);
                writer.WriteStartAttribute(dictionaryManager.XmlSignatureDictionary.URI, null);
                writer.WriteString("#");
                writer.WriteString(this.references[i].id);
                writer.WriteEndAttribute();
                writer.WriteStartElement(prefix, dictionaryManager.XmlSignatureDictionary.Transforms, namespaceUri);
                if (this.addEnvelopedSignatureTransform)
                {
                    writer.WriteStartElement(prefix, dictionaryManager.XmlSignatureDictionary.Transform, namespaceUri);
                    writer.WriteStartAttribute(dictionaryManager.XmlSignatureDictionary.Algorithm, null);
                    writer.WriteString(dictionaryManager.XmlSignatureDictionary.EnvelopedSignature);
                    writer.WriteEndAttribute();
                    writer.WriteEndElement();
                }
                writer.WriteStartElement(prefix, dictionaryManager.XmlSignatureDictionary.Transform, namespaceUri);
                writer.WriteStartAttribute(dictionaryManager.XmlSignatureDictionary.Algorithm, null);
                writer.WriteString(dictionaryManager.SecurityAlgorithmDictionary.ExclusiveC14n);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteStartElement(prefix, dictionaryManager.XmlSignatureDictionary.DigestMethod, namespaceUri);
                writer.WriteStartAttribute(dictionaryManager.XmlSignatureDictionary.Algorithm, null);
                if (this.digestMethodDictionaryString != null)
                {
                    writer.WriteString(this.digestMethodDictionaryString);
                }
                else
                {
                    writer.WriteString(this.digestMethod);
                }
                writer.WriteEndAttribute();
                writer.WriteEndElement();
                byte[] digest = this.references[i].digest;
                writer.WriteStartElement(prefix, dictionaryManager.XmlSignatureDictionary.DigestValue, namespaceUri);
                writer.WriteBase64(digest, 0, digest.Length);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public bool AddEnvelopedSignatureTransform
        {
            get
            {
                return this.addEnvelopedSignatureTransform;
            }
            set
            {
                this.addEnvelopedSignatureTransform = value;
            }
        }

        public string DigestMethod
        {
            get
            {
                return this.digestMethod;
            }
            set
            {
                this.digestMethod = value;
            }
        }

        public override int ReferenceCount
        {
            get
            {
                return this.count;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ReferenceEntry
        {
            internal string id;
            internal byte[] digest;
            public void Set(string id, byte[] digest)
            {
                this.id = id;
                this.digest = digest;
            }
        }

        private sealed class SignedInfoCanonicalFormWriter : CanonicalFormWriter
        {
            private readonly byte[] fragment1;
            private readonly byte[] fragment2;
            private readonly byte[] fragment3;
            private readonly byte[] fragment4;
            private readonly byte[] fragment5;
            private readonly byte[] fragment6;
            private readonly byte[] fragment7;
            private readonly byte[] hmacSha1Signature;
            private static readonly PreDigestedSignedInfo.SignedInfoCanonicalFormWriter instance = new PreDigestedSignedInfo.SignedInfoCanonicalFormWriter();
            private readonly byte[] rsaSha1Signature;
            private readonly byte[] sha1Digest;
            private readonly byte[] sha256Digest;
            private const string xml1 = "<SignedInfo xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><CanonicalizationMethod Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\"></CanonicalizationMethod><SignatureMethod Algorithm=\"";
            private const string xml2 = "\"></SignatureMethod>";
            private const string xml3 = "<Reference URI=\"#";
            private const string xml4 = "\"><Transforms><Transform Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\"></Transform></Transforms><DigestMethod Algorithm=\"";
            private const string xml5 = "\"></DigestMethod><DigestValue>";
            private const string xml6 = "</DigestValue></Reference>";
            private const string xml7 = "</SignedInfo>";

            private SignedInfoCanonicalFormWriter()
            {
                UTF8Encoding encoding = CanonicalFormWriter.Utf8WithoutPreamble;
                this.fragment1 = encoding.GetBytes("<SignedInfo xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><CanonicalizationMethod Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\"></CanonicalizationMethod><SignatureMethod Algorithm=\"");
                this.fragment2 = encoding.GetBytes("\"></SignatureMethod>");
                this.fragment3 = encoding.GetBytes("<Reference URI=\"#");
                this.fragment4 = encoding.GetBytes("\"><Transforms><Transform Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\"></Transform></Transforms><DigestMethod Algorithm=\"");
                this.fragment5 = encoding.GetBytes("\"></DigestMethod><DigestValue>");
                this.fragment6 = encoding.GetBytes("</DigestValue></Reference>");
                this.fragment7 = encoding.GetBytes("</SignedInfo>");
                this.sha1Digest = encoding.GetBytes("http://www.w3.org/2000/09/xmldsig#sha1");
                this.sha256Digest = encoding.GetBytes("http://www.w3.org/2001/04/xmlenc#sha256");
                this.hmacSha1Signature = encoding.GetBytes("http://www.w3.org/2000/09/xmldsig#hmac-sha1");
                this.rsaSha1Signature = encoding.GetBytes("http://www.w3.org/2000/09/xmldsig#rsa-sha1");
            }

            private byte[] EncodeDigestAlgorithm(string algorithm)
            {
                if (algorithm == "http://www.w3.org/2000/09/xmldsig#sha1")
                {
                    return this.sha1Digest;
                }
                if (algorithm == "http://www.w3.org/2001/04/xmlenc#sha256")
                {
                    return this.sha256Digest;
                }
                return CanonicalFormWriter.Utf8WithoutPreamble.GetBytes(algorithm);
            }

            private byte[] EncodeSignatureAlgorithm(string algorithm)
            {
                if (algorithm == "http://www.w3.org/2000/09/xmldsig#hmac-sha1")
                {
                    return this.hmacSha1Signature;
                }
                if (algorithm == "http://www.w3.org/2000/09/xmldsig#rsa-sha1")
                {
                    return this.rsaSha1Signature;
                }
                return CanonicalFormWriter.Utf8WithoutPreamble.GetBytes(algorithm);
            }

            public void WriteSignedInfoCanonicalForm(Stream stream, string signatureMethod, string digestMethod, PreDigestedSignedInfo.ReferenceEntry[] references, int referenceCount, byte[] workBuffer, char[] base64WorkBuffer)
            {
                stream.Write(this.fragment1, 0, this.fragment1.Length);
                byte[] buffer = this.EncodeSignatureAlgorithm(signatureMethod);
                stream.Write(buffer, 0, buffer.Length);
                stream.Write(this.fragment2, 0, this.fragment2.Length);
                byte[] buffer2 = this.EncodeDigestAlgorithm(digestMethod);
                for (int i = 0; i < referenceCount; i++)
                {
                    stream.Write(this.fragment3, 0, this.fragment3.Length);
                    CanonicalFormWriter.EncodeAndWrite(stream, workBuffer, references[i].id);
                    stream.Write(this.fragment4, 0, this.fragment4.Length);
                    stream.Write(buffer2, 0, buffer2.Length);
                    stream.Write(this.fragment5, 0, this.fragment5.Length);
                    CanonicalFormWriter.Base64EncodeAndWrite(stream, workBuffer, base64WorkBuffer, references[i].digest);
                    stream.Write(this.fragment6, 0, this.fragment6.Length);
                }
                stream.Write(this.fragment7, 0, this.fragment7.Length);
            }

            public static PreDigestedSignedInfo.SignedInfoCanonicalFormWriter Instance
            {
                get
                {
                    return instance;
                }
            }
        }
    }
}

