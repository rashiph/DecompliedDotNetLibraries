namespace System.Web.Services.Discovery
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Text;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Description;
    using System.Web.Services.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlRoot("contractRef", Namespace="http://schemas.xmlsoap.org/disco/scl/")]
    public class ContractReference : DiscoveryReference
    {
        private string docRef;
        public const string Namespace = "http://schemas.xmlsoap.org/disco/scl/";
        private string reference;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ContractReference()
        {
        }

        public ContractReference(string href)
        {
            this.Ref = href;
        }

        public ContractReference(string href, string docRef)
        {
            this.Ref = href;
            this.DocRef = docRef;
        }

        internal override void LoadExternals(Hashtable loadedExternals)
        {
            ServiceDescription contract = null;
            try
            {
                contract = this.Contract;
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                base.ClientProtocol.Errors[this.Url] = exception;
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, this, "LoadExternals", exception);
                }
            }
            if (contract != null)
            {
                foreach (XmlSchema schema in this.Contract.Types.Schemas)
                {
                    SchemaReference.LoadExternals(schema, this.Url, base.ClientProtocol, loadedExternals);
                }
            }
        }

        public override object ReadDocument(Stream stream)
        {
            return ServiceDescription.Read(stream, true);
        }

        protected internal override void Resolve(string contentType, Stream stream)
        {
            if (ContentType.IsHtml(contentType))
            {
                throw new InvalidContentTypeException(System.Web.Services.Res.GetString("WebInvalidContentType", new object[] { contentType }), contentType);
            }
            ServiceDescription description = base.ClientProtocol.Documents[this.Url] as ServiceDescription;
            if (description == null)
            {
                description = ServiceDescription.Read(stream, true);
                description.RetrievalUrl = this.Url;
                base.ClientProtocol.Documents[this.Url] = description;
            }
            base.ClientProtocol.References[this.Url] = this;
            ArrayList list = new ArrayList();
            foreach (Import import in description.Imports)
            {
                if (import.Location != null)
                {
                    list.Add(import.Location);
                }
            }
            foreach (XmlSchema schema in description.Types.Schemas)
            {
                foreach (XmlSchemaExternal external in schema.Includes)
                {
                    if ((external.SchemaLocation != null) && (external.SchemaLocation.Length > 0))
                    {
                        list.Add(external.SchemaLocation);
                    }
                }
            }
            foreach (string str in list)
            {
                string url = DiscoveryReference.UriToString(this.Url, str);
                if (base.ClientProtocol.Documents[url] == null)
                {
                    string str3 = url;
                    try
                    {
                        stream = base.ClientProtocol.Download(ref url, ref contentType);
                        try
                        {
                            if (base.ClientProtocol.Documents[url] == null)
                            {
                                XmlTextReader reader = new XmlTextReader(new StreamReader(stream, RequestResponseUtils.GetEncoding(contentType))) {
                                    WhitespaceHandling = WhitespaceHandling.Significant,
                                    XmlResolver = null,
                                    DtdProcessing = DtdProcessing.Prohibit
                                };
                                if (ServiceDescription.CanRead(reader))
                                {
                                    ServiceDescription description2 = ServiceDescription.Read(reader, true);
                                    description2.RetrievalUrl = url;
                                    base.ClientProtocol.Documents[url] = description2;
                                    ContractReference reference = new ContractReference(url, null) {
                                        ClientProtocol = base.ClientProtocol
                                    };
                                    try
                                    {
                                        reference.Resolve(contentType, stream);
                                    }
                                    catch (Exception exception)
                                    {
                                        if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                                        {
                                            throw;
                                        }
                                        reference.Url = str3;
                                        if (Tracing.On)
                                        {
                                            Tracing.ExceptionCatch(TraceEventType.Warning, this, "Resolve", exception);
                                        }
                                    }
                                }
                                else if (reader.IsStartElement("schema", "http://www.w3.org/2001/XMLSchema"))
                                {
                                    base.ClientProtocol.Documents[url] = XmlSchema.Read(reader, null);
                                    SchemaReference reference2 = new SchemaReference(url) {
                                        ClientProtocol = base.ClientProtocol
                                    };
                                    try
                                    {
                                        reference2.Resolve(contentType, stream);
                                    }
                                    catch (Exception exception2)
                                    {
                                        if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                                        {
                                            throw;
                                        }
                                        reference2.Url = str3;
                                        if (Tracing.On)
                                        {
                                            Tracing.ExceptionCatch(TraceEventType.Warning, this, "Resolve", exception2);
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            stream.Close();
                        }
                    }
                    catch (Exception exception3)
                    {
                        if (((exception3 is ThreadAbortException) || (exception3 is StackOverflowException)) || (exception3 is OutOfMemoryException))
                        {
                            throw;
                        }
                        throw new InvalidDocumentContentsException(System.Web.Services.Res.GetString("TheWSDLDocumentContainsLinksThatCouldNotBeResolved", new object[] { url }), exception3);
                    }
                }
            }
        }

        public override void WriteDocument(object document, Stream stream)
        {
            ((ServiceDescription) document).Write(new StreamWriter(stream, new UTF8Encoding(false)));
        }

        [XmlIgnore]
        public ServiceDescription Contract
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
                ServiceDescription description = obj2 as ServiceDescription;
                if (description == null)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebInvalidDocType", new object[] { typeof(ServiceDescription).FullName, (obj2 == null) ? string.Empty : obj2.GetType().FullName, this.Url }));
                }
                return description;
            }
        }

        [XmlIgnore]
        public override string DefaultFilename
        {
            get
            {
                string path = DiscoveryReference.MakeValidFilename(this.Contract.Name);
                if ((path == null) || (path.Length == 0))
                {
                    path = DiscoveryReference.FilenameFromUrl(this.Url);
                }
                return Path.ChangeExtension(path, ".wsdl");
            }
        }

        [XmlAttribute("docRef")]
        public string DocRef
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.docRef;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.docRef = value;
            }
        }

        [XmlAttribute("ref")]
        public string Ref
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.reference;
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

