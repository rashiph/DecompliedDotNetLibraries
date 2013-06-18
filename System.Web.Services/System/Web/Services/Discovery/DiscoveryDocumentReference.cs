namespace System.Web.Services.Discovery
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Text;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlRoot("discoveryRef", Namespace="http://schemas.xmlsoap.org/disco/")]
    public sealed class DiscoveryDocumentReference : DiscoveryReference
    {
        private string reference;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DiscoveryDocumentReference()
        {
        }

        public DiscoveryDocumentReference(string href)
        {
            this.Ref = href;
        }

        private static DiscoveryDocument GetDocumentNoParse(ref string url, DiscoveryClientProtocol client)
        {
            DiscoveryDocument document2;
            DiscoveryDocument document = (DiscoveryDocument) client.Documents[url];
            if (document != null)
            {
                return document;
            }
            string contentType = null;
            Stream stream = client.Download(ref url, ref contentType);
            try
            {
                XmlTextReader xmlReader = new XmlTextReader(new StreamReader(stream, RequestResponseUtils.GetEncoding(contentType))) {
                    WhitespaceHandling = WhitespaceHandling.Significant,
                    XmlResolver = null,
                    DtdProcessing = DtdProcessing.Prohibit
                };
                if (!DiscoveryDocument.CanRead(xmlReader))
                {
                    ArgumentException innerException = new ArgumentException(System.Web.Services.Res.GetString("WebInvalidFormat"));
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebMissingDocument", new object[] { url }), innerException);
                }
                document2 = DiscoveryDocument.Read(xmlReader);
            }
            finally
            {
                stream.Close();
            }
            return document2;
        }

        public override object ReadDocument(Stream stream)
        {
            return WebServicesSection.Current.DiscoveryDocumentSerializer.Deserialize(stream);
        }

        protected internal override void Resolve(string contentType, Stream stream)
        {
            DiscoveryDocument documentNoParse = null;
            if (ContentType.IsHtml(contentType))
            {
                string relUrl = LinkGrep.SearchForLink(stream);
                if (relUrl == null)
                {
                    throw new InvalidContentTypeException(System.Web.Services.Res.GetString("WebInvalidContentType", new object[] { contentType }), contentType);
                }
                string url = DiscoveryReference.UriToString(this.Url, relUrl);
                documentNoParse = GetDocumentNoParse(ref url, base.ClientProtocol);
                this.Url = url;
            }
            if (documentNoParse == null)
            {
                XmlTextReader xmlReader = new XmlTextReader(new StreamReader(stream, RequestResponseUtils.GetEncoding(contentType))) {
                    XmlResolver = null,
                    WhitespaceHandling = WhitespaceHandling.Significant,
                    DtdProcessing = DtdProcessing.Prohibit
                };
                if (DiscoveryDocument.CanRead(xmlReader))
                {
                    documentNoParse = DiscoveryDocument.Read(xmlReader);
                }
                else
                {
                    stream.Position = 0L;
                    XmlTextReader reader2 = new XmlTextReader(new StreamReader(stream, RequestResponseUtils.GetEncoding(contentType))) {
                        XmlResolver = null,
                        DtdProcessing = DtdProcessing.Prohibit
                    };
                    while (reader2.NodeType != XmlNodeType.Element)
                    {
                        if (reader2.NodeType == XmlNodeType.ProcessingInstruction)
                        {
                            StringBuilder builder = new StringBuilder("<pi ");
                            builder.Append(reader2.Value);
                            builder.Append("/>");
                            XmlTextReader reader3 = new XmlTextReader(new StringReader(builder.ToString())) {
                                XmlResolver = null,
                                DtdProcessing = DtdProcessing.Prohibit
                            };
                            reader3.Read();
                            string str3 = reader3["type"];
                            string strA = reader3["alternate"];
                            string str5 = reader3["href"];
                            if ((((str3 != null) && ContentType.MatchesBase(str3, "text/xml")) && ((strA != null) && (string.Compare(strA, "yes", StringComparison.OrdinalIgnoreCase) == 0))) && (str5 != null))
                            {
                                string str6 = DiscoveryReference.UriToString(this.Url, str5);
                                documentNoParse = GetDocumentNoParse(ref str6, base.ClientProtocol);
                                this.Url = str6;
                                break;
                            }
                        }
                        reader2.Read();
                    }
                }
            }
            if (documentNoParse == null)
            {
                Exception exception;
                if (ContentType.IsXml(contentType))
                {
                    exception = new ArgumentException(System.Web.Services.Res.GetString("WebInvalidFormat"));
                }
                else
                {
                    exception = new InvalidContentTypeException(System.Web.Services.Res.GetString("WebInvalidContentType", new object[] { contentType }), contentType);
                }
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebMissingDocument", new object[] { this.Url }), exception);
            }
            base.ClientProtocol.References[this.Url] = this;
            base.ClientProtocol.Documents[this.Url] = documentNoParse;
            foreach (object obj2 in documentNoParse.References)
            {
                if (obj2 is DiscoveryReference)
                {
                    DiscoveryReference reference = (DiscoveryReference) obj2;
                    if (reference.Url.Length == 0)
                    {
                        throw new InvalidOperationException(System.Web.Services.Res.GetString("WebEmptyRef", new object[] { reference.GetType().FullName, this.Url }));
                    }
                    reference.Url = DiscoveryReference.UriToString(this.Url, reference.Url);
                    ContractReference reference2 = reference as ContractReference;
                    if ((reference2 != null) && (reference2.DocRef != null))
                    {
                        reference2.DocRef = DiscoveryReference.UriToString(this.Url, reference2.DocRef);
                    }
                    reference.ClientProtocol = base.ClientProtocol;
                    base.ClientProtocol.References[reference.Url] = reference;
                }
                else
                {
                    base.ClientProtocol.AdditionalInformation.Add(obj2);
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void ResolveAll()
        {
            this.ResolveAll(true);
        }

        internal void ResolveAll(bool throwOnError)
        {
            try
            {
                base.Resolve();
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (throwOnError)
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, this, "ResolveAll", exception);
                }
                return;
            }
            foreach (object obj2 in this.Document.References)
            {
                DiscoveryDocumentReference reference = obj2 as DiscoveryDocumentReference;
                if ((reference != null) && (base.ClientProtocol.Documents[reference.Url] == null))
                {
                    reference.ClientProtocol = base.ClientProtocol;
                    reference.ResolveAll(throwOnError);
                }
            }
        }

        public override void WriteDocument(object document, Stream stream)
        {
            WebServicesSection.Current.DiscoveryDocumentSerializer.Serialize((TextWriter) new StreamWriter(stream, new UTF8Encoding(false)), document);
        }

        [XmlIgnore]
        public override string DefaultFilename
        {
            get
            {
                return Path.ChangeExtension(DiscoveryReference.FilenameFromUrl(this.Url), ".disco");
            }
        }

        [XmlIgnore]
        public DiscoveryDocument Document
        {
            get
            {
                if (base.ClientProtocol == null)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebMissingClientProtocol"));
                }
                object obj2 = base.ClientProtocol.Documents[this.Url];
                if (obj2 == null)
                {
                    base.Resolve();
                    obj2 = base.ClientProtocol.Documents[this.Url];
                }
                DiscoveryDocument document = obj2 as DiscoveryDocument;
                if (document == null)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebInvalidDocType", new object[] { typeof(DiscoveryDocument).FullName, (obj2 == null) ? string.Empty : obj2.GetType().FullName, this.Url }));
                }
                return document;
            }
        }

        [XmlAttribute("ref")]
        public string Ref
        {
            get
            {
                if (this.reference != null)
                {
                    return this.reference;
                }
                return "";
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.reference = value;
            }
        }

        [XmlIgnore]
        public override string Url
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.Ref;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.Ref = value;
            }
        }
    }
}

