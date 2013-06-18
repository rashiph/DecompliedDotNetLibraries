namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.IO;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;

    internal abstract class WSUtilitySpecificationVersion
    {
        internal static readonly string[] AcceptedDateTimeFormats = new string[] { "yyyy-MM-ddTHH:mm:ss.fffffffZ", "yyyy-MM-ddTHH:mm:ss.ffffffZ", "yyyy-MM-ddTHH:mm:ss.fffffZ", "yyyy-MM-ddTHH:mm:ss.ffffZ", "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ss.ffZ", "yyyy-MM-ddTHH:mm:ss.fZ", "yyyy-MM-ddTHH:mm:ssZ" };
        private readonly XmlDictionaryString namespaceUri;

        internal WSUtilitySpecificationVersion(XmlDictionaryString namespaceUri)
        {
            this.namespaceUri = namespaceUri;
        }

        internal abstract bool IsReaderAtTimestamp(XmlDictionaryReader reader);
        internal abstract SecurityTimestamp ReadTimestamp(XmlDictionaryReader reader, string digestAlgorithm, SignatureResourcePool resourcePool);
        internal abstract void WriteTimestamp(XmlDictionaryWriter writer, SecurityTimestamp timestamp);
        internal abstract void WriteTimestampCanonicalForm(Stream stream, SecurityTimestamp timestamp, byte[] buffer);

        public static WSUtilitySpecificationVersion Default
        {
            get
            {
                return OneDotZero;
            }
        }

        internal XmlDictionaryString NamespaceUri
        {
            get
            {
                return this.namespaceUri;
            }
        }

        public static WSUtilitySpecificationVersion OneDotZero
        {
            get
            {
                return WSUtilitySpecificationVersionOneDotZero.Instance;
            }
        }

        private sealed class TimestampCanonicalFormWriter : CanonicalFormWriter
        {
            private const string created = "u:Created";
            private const string expires = "u:Expires";
            private readonly byte[] fragment1;
            private readonly byte[] fragment2;
            private readonly byte[] fragment3;
            private readonly byte[] fragment4;
            private const string idAttribute = "u:Id";
            private static readonly WSUtilitySpecificationVersion.TimestampCanonicalFormWriter instance = new WSUtilitySpecificationVersion.TimestampCanonicalFormWriter();
            private const string ns = "xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\"";
            private const string timestamp = "u:Timestamp";
            private const string xml1 = "<u:Timestamp xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" u:Id=\"";
            private const string xml2 = "\"><u:Created>";
            private const string xml3 = "</u:Created><u:Expires>";
            private const string xml4 = "</u:Expires></u:Timestamp>";

            private TimestampCanonicalFormWriter()
            {
                UTF8Encoding encoding = CanonicalFormWriter.Utf8WithoutPreamble;
                this.fragment1 = encoding.GetBytes("<u:Timestamp xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" u:Id=\"");
                this.fragment2 = encoding.GetBytes("\"><u:Created>");
                this.fragment3 = encoding.GetBytes("</u:Created><u:Expires>");
                this.fragment4 = encoding.GetBytes("</u:Expires></u:Timestamp>");
            }

            public void WriteCanonicalForm(Stream stream, string id, char[] created, char[] expires, byte[] workBuffer)
            {
                stream.Write(this.fragment1, 0, this.fragment1.Length);
                CanonicalFormWriter.EncodeAndWrite(stream, workBuffer, id);
                stream.Write(this.fragment2, 0, this.fragment2.Length);
                CanonicalFormWriter.EncodeAndWrite(stream, workBuffer, created);
                stream.Write(this.fragment3, 0, this.fragment3.Length);
                CanonicalFormWriter.EncodeAndWrite(stream, workBuffer, expires);
                stream.Write(this.fragment4, 0, this.fragment4.Length);
            }

            public static WSUtilitySpecificationVersion.TimestampCanonicalFormWriter Instance
            {
                get
                {
                    return instance;
                }
            }
        }

        private sealed class WSUtilitySpecificationVersionOneDotZero : WSUtilitySpecificationVersion
        {
            private static readonly WSUtilitySpecificationVersion.WSUtilitySpecificationVersionOneDotZero instance = new WSUtilitySpecificationVersion.WSUtilitySpecificationVersionOneDotZero();

            private WSUtilitySpecificationVersionOneDotZero() : base(System.ServiceModel.XD.UtilityDictionary.Namespace)
            {
            }

            internal override bool IsReaderAtTimestamp(XmlDictionaryReader reader)
            {
                return reader.IsStartElement(System.ServiceModel.XD.UtilityDictionary.Timestamp, System.ServiceModel.XD.UtilityDictionary.Namespace);
            }

            internal override SecurityTimestamp ReadTimestamp(XmlDictionaryReader reader, string digestAlgorithm, SignatureResourcePool resourcePool)
            {
                DateTime maxUtcDateTime;
                byte[] buffer;
                bool flag = (digestAlgorithm != null) && reader.CanCanonicalize;
                HashStream stream = null;
                reader.MoveToStartElement(System.ServiceModel.XD.UtilityDictionary.Timestamp, System.ServiceModel.XD.UtilityDictionary.Namespace);
                if (flag)
                {
                    stream = resourcePool.TakeHashStream(digestAlgorithm);
                    reader.StartCanonicalization(stream, false, null);
                }
                string attribute = reader.GetAttribute(System.ServiceModel.XD.UtilityDictionary.IdAttribute, System.ServiceModel.XD.UtilityDictionary.Namespace);
                reader.ReadStartElement();
                reader.ReadStartElement(System.ServiceModel.XD.UtilityDictionary.CreatedElement, System.ServiceModel.XD.UtilityDictionary.Namespace);
                DateTime creationTimeUtc = reader.ReadContentAsDateTime().ToUniversalTime();
                reader.ReadEndElement();
                if (reader.IsStartElement(System.ServiceModel.XD.UtilityDictionary.ExpiresElement, System.ServiceModel.XD.UtilityDictionary.Namespace))
                {
                    reader.ReadStartElement();
                    maxUtcDateTime = reader.ReadContentAsDateTime().ToUniversalTime();
                    reader.ReadEndElement();
                }
                else
                {
                    maxUtcDateTime = System.ServiceModel.Security.SecurityUtils.MaxUtcDateTime;
                }
                reader.ReadEndElement();
                if (flag)
                {
                    reader.EndCanonicalization();
                    buffer = stream.FlushHashAndGetValue();
                }
                else
                {
                    buffer = null;
                }
                return new SecurityTimestamp(creationTimeUtc, maxUtcDateTime, attribute, digestAlgorithm, buffer);
            }

            internal override void WriteTimestamp(XmlDictionaryWriter writer, SecurityTimestamp timestamp)
            {
                writer.WriteStartElement(System.ServiceModel.XD.UtilityDictionary.Prefix.Value, System.ServiceModel.XD.UtilityDictionary.Timestamp, System.ServiceModel.XD.UtilityDictionary.Namespace);
                writer.WriteAttributeString(System.ServiceModel.XD.UtilityDictionary.IdAttribute, System.ServiceModel.XD.UtilityDictionary.Namespace, timestamp.Id);
                writer.WriteStartElement(System.ServiceModel.XD.UtilityDictionary.CreatedElement, System.ServiceModel.XD.UtilityDictionary.Namespace);
                char[] creationTimeChars = timestamp.GetCreationTimeChars();
                writer.WriteChars(creationTimeChars, 0, creationTimeChars.Length);
                writer.WriteEndElement();
                writer.WriteStartElement(System.ServiceModel.XD.UtilityDictionary.ExpiresElement, System.ServiceModel.XD.UtilityDictionary.Namespace);
                char[] expiryTimeChars = timestamp.GetExpiryTimeChars();
                writer.WriteChars(expiryTimeChars, 0, expiryTimeChars.Length);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            internal override void WriteTimestampCanonicalForm(Stream stream, SecurityTimestamp timestamp, byte[] workBuffer)
            {
                WSUtilitySpecificationVersion.TimestampCanonicalFormWriter.Instance.WriteCanonicalForm(stream, timestamp.Id, timestamp.GetCreationTimeChars(), timestamp.GetExpiryTimeChars(), workBuffer);
            }

            public static WSUtilitySpecificationVersion.WSUtilitySpecificationVersionOneDotZero Instance
            {
                get
                {
                    return instance;
                }
            }
        }
    }
}

