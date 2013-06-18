namespace System.Web.Services.Protocols
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Description;
    using System.Web.Services.Diagnostics;
    using System.Xml;
    using System.Xml.Serialization;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class SoapServerProtocol : ServerProtocol
    {
        private SoapServerProtocolHelper helper;
        private bool isOneWay;
        private SoapServerMessage message;
        private Exception onewayInitException;
        private WebServiceProtocols protocolsSupported = WebServicesSection.Current.EnabledProtocols;
        private SoapServerMethod serverMethod;
        private SoapServerType serverType;
        private SoapProtocolVersion version;

        protected internal SoapServerProtocol()
        {
        }

        private void CheckHelperVersion()
        {
            if (this.helper.RequestNamespace != null)
            {
                if (this.helper.RequestNamespace != this.helper.EnvelopeNs)
                {
                    string requestNamespace = this.helper.RequestNamespace;
                    if (this.IsSupported(WebServiceProtocols.HttpSoap))
                    {
                        this.SetHelper(new Soap11ServerProtocolHelper(this));
                    }
                    else
                    {
                        this.SetHelper(new Soap12ServerProtocolHelper(this));
                    }
                    throw new SoapException(System.Web.Services.Res.GetString("WebInvalidEnvelopeNamespace", new object[] { requestNamespace, this.helper.EnvelopeNs }), SoapException.VersionMismatchFaultCode);
                }
                if (!this.IsSupported(this.helper.Protocol))
                {
                    string str2 = this.helper.RequestNamespace;
                    string str3 = this.IsSupported(WebServiceProtocols.HttpSoap) ? "http://schemas.xmlsoap.org/soap/envelope/" : "http://www.w3.org/2003/05/soap-envelope";
                    this.SetHelper(new Soap11ServerProtocolHelper(this));
                    throw new SoapException(System.Web.Services.Res.GetString("WebInvalidEnvelopeNamespace", new object[] { str2, str3 }), SoapException.VersionMismatchFaultCode);
                }
            }
        }

        private static Array CombineExtensionsHelper(Array array1, Array array2, Type elementType)
        {
            if (array1 == null)
            {
                return array2;
            }
            if (array2 == null)
            {
                return array1;
            }
            int num = array1.Length + array2.Length;
            if (num == 0)
            {
                return null;
            }
            Array destinationArray = null;
            if (elementType == typeof(SoapReflectedExtension))
            {
                destinationArray = new SoapReflectedExtension[num];
            }
            else if (elementType == typeof(SoapExtension))
            {
                destinationArray = new SoapExtension[num];
            }
            else
            {
                if (elementType != typeof(object))
                {
                    throw new ArgumentException(System.Web.Services.Res.GetString("ElementTypeMustBeObjectOrSoapExtensionOrSoapReflectedException"), "elementType");
                }
                destinationArray = new object[num];
            }
            Array.Copy(array1, 0, destinationArray, 0, array1.Length);
            Array.Copy(array2, 0, destinationArray, array1.Length, array2.Length);
            return destinationArray;
        }

        internal override void CreateServerInstance()
        {
            base.CreateServerInstance();
            this.message.SetStage(SoapMessageStage.AfterDeserialize);
            this.message.RunExtensions(this.message.allExtensions, true);
            SoapHeaderHandling.SetHeaderMembers(this.message.Headers, this.Target, this.serverMethod.inHeaderMappings, SoapHeaderDirection.In, false);
        }

        private static XmlElement CreateUpgradeEnvelope(XmlDocument doc, string prefix, string envelopeNs)
        {
            XmlElement element = doc.CreateElement("soap12", "SupportedEnvelope", "http://www.w3.org/2003/05/soap-envelope");
            XmlAttribute node = doc.CreateAttribute("xmlns", prefix, "http://www.w3.org/2000/xmlns/");
            node.Value = envelopeNs;
            XmlAttribute attribute2 = doc.CreateAttribute("qname");
            attribute2.Value = prefix + ":Envelope";
            element.Attributes.Append(attribute2);
            element.Attributes.Append(node);
            return element;
        }

        internal SoapUnknownHeader CreateUpgradeHeader()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement("soap12", "Upgrade", "http://www.w3.org/2003/05/soap-envelope");
            if (this.IsSupported(WebServiceProtocols.HttpSoap))
            {
                element.AppendChild(CreateUpgradeEnvelope(doc, "soap", "http://schemas.xmlsoap.org/soap/envelope/"));
            }
            if (this.IsSupported(WebServiceProtocols.HttpSoap12))
            {
                element.AppendChild(CreateUpgradeEnvelope(doc, "soap12", "http://www.w3.org/2003/05/soap-envelope"));
            }
            return new SoapUnknownHeader { Element = element };
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        protected virtual XmlReader GetReaderForMessage(SoapServerMessage message, int bufferSize)
        {
            XmlTextReader reader;
            Encoding encoding = RequestResponseUtils.GetEncoding2(message.ContentType);
            if (bufferSize < 0x200)
            {
                bufferSize = 0x200;
            }
            int readTimeout = WebServicesSection.Current.SoapEnvelopeProcessing.ReadTimeout;
            long num2 = (readTimeout < 0) ? 0L : (readTimeout * 0x989680L);
            long ticks = DateTime.UtcNow.Ticks;
            long timeout = ((0x7fffffffffffffffL - num2) <= ticks) ? 0x7fffffffffffffffL : (ticks + num2);
            if (encoding != null)
            {
                if (timeout == 0x7fffffffffffffffL)
                {
                    reader = new XmlTextReader(new StreamReader(message.Stream, encoding, true, bufferSize));
                }
                else
                {
                    reader = new SoapEnvelopeReader(new StreamReader(message.Stream, encoding, true, bufferSize), timeout);
                }
            }
            else if (timeout == 0x7fffffffffffffffL)
            {
                reader = new XmlTextReader(message.Stream);
            }
            else
            {
                reader = new SoapEnvelopeReader(message.Stream, timeout);
            }
            reader.DtdProcessing = DtdProcessing.Prohibit;
            reader.Normalization = true;
            reader.XmlResolver = null;
            return reader;
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        protected virtual XmlWriter GetWriterForMessage(SoapServerMessage message, int bufferSize)
        {
            if (bufferSize < 0x200)
            {
                bufferSize = 0x200;
            }
            return new XmlTextWriter(new StreamWriter(message.Stream, new UTF8Encoding(false), bufferSize));
        }

        internal XmlReader GetXmlReader()
        {
            Encoding encoding = RequestResponseUtils.GetEncoding2(this.Message.ContentType);
            if (((((this.serverMethod != null) && ((this.serverMethod.wsiClaims & WsiProfiles.BasicProfile1_1) != WsiProfiles.None)) && (this.Version != SoapProtocolVersion.Soap12)) && (encoding != null)) && (!(encoding is UTF8Encoding) && !(encoding is UnicodeEncoding)))
            {
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebWsiContentTypeEncoding"));
            }
            XmlReader readerForMessage = this.GetReaderForMessage(this.Message, RequestResponseUtils.GetBufferSize(base.Request.ContentLength));
            if (readerForMessage == null)
            {
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebNullReaderForMessage"));
            }
            return readerForMessage;
        }

        private void GuessVersion()
        {
            if (this.IsSupported(WebServiceProtocols.AnyHttpSoap))
            {
                if ((base.Request.Headers["SOAPAction"] == null) || ContentType.MatchesBase(base.Request.ContentType, "application/soap+xml"))
                {
                    this.SetHelper(new Soap12ServerProtocolHelper(this));
                }
                else
                {
                    this.SetHelper(new Soap11ServerProtocolHelper(this));
                }
            }
            else if (this.IsSupported(WebServiceProtocols.HttpSoap))
            {
                this.SetHelper(new Soap11ServerProtocolHelper(this));
            }
            else if (this.IsSupported(WebServiceProtocols.HttpSoap12))
            {
                this.SetHelper(new Soap12ServerProtocolHelper(this));
            }
        }

        internal override bool Initialize()
        {
            this.GuessVersion();
            this.message = new SoapServerMessage(this);
            this.onewayInitException = null;
            this.serverType = (SoapServerType) base.GetFromCache(typeof(SoapServerProtocol), base.Type);
            if (this.serverType == null)
            {
                lock (ServerProtocol.InternalSyncObject)
                {
                    this.serverType = (SoapServerType) base.GetFromCache(typeof(SoapServerProtocol), base.Type);
                    if (this.serverType == null)
                    {
                        this.serverType = new SoapServerType(base.Type, this.protocolsSupported);
                        base.AddToCache(typeof(SoapServerProtocol), base.Type, this.serverType);
                    }
                }
            }
            Exception innerException = null;
            try
            {
                this.message.highPriConfigExtensions = SoapMessage.InitializeExtensions(this.serverType.HighPriExtensions, this.serverType.HighPriExtensionInitializers);
                this.message.highPriConfigExtensions = this.ModifyInitializedExtensions(PriorityGroup.High, this.message.highPriConfigExtensions);
                this.message.SetStream(base.Request.InputStream);
                this.message.InitExtensionStreamChain(this.message.highPriConfigExtensions);
                this.message.SetStage(SoapMessageStage.BeforeDeserialize);
                this.message.ContentType = base.Request.ContentType;
                this.message.ContentEncoding = base.Request.Headers["Content-Encoding"];
                this.message.RunExtensions(this.message.highPriConfigExtensions, false);
                innerException = this.message.Exception;
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, this, "Initialize", exception2);
                }
                innerException = exception2;
            }
            this.message.allExtensions = this.message.highPriConfigExtensions;
            this.GuessVersion();
            try
            {
                this.serverMethod = this.RouteRequest(this.message);
                if (this.serverMethod == null)
                {
                    throw new SoapException(System.Web.Services.Res.GetString("UnableToHandleRequest0"), new XmlQualifiedName("Server", "http://schemas.xmlsoap.org/soap/envelope/"));
                }
            }
            catch (Exception exception3)
            {
                if (((exception3 is ThreadAbortException) || (exception3 is StackOverflowException)) || (exception3 is OutOfMemoryException))
                {
                    throw;
                }
                if (this.helper.RequestNamespace != null)
                {
                    this.SetHelper(SoapServerProtocolHelper.GetHelper(this, this.helper.RequestNamespace));
                }
                this.CheckHelperVersion();
                throw;
            }
            this.isOneWay = this.serverMethod.oneWay;
            if (innerException == null)
            {
                try
                {
                    SoapReflectedExtension[] reflectedExtensions = (SoapReflectedExtension[]) CombineExtensionsHelper(this.serverMethod.extensions, this.serverType.LowPriExtensions, typeof(SoapReflectedExtension));
                    object[] extensionInitializers = (object[]) CombineExtensionsHelper(this.serverMethod.extensionInitializers, this.serverType.LowPriExtensionInitializers, typeof(object));
                    this.message.otherExtensions = SoapMessage.InitializeExtensions(reflectedExtensions, extensionInitializers);
                    this.message.otherExtensions = this.ModifyInitializedExtensions(PriorityGroup.Low, this.message.otherExtensions);
                    this.message.allExtensions = (SoapExtension[]) CombineExtensionsHelper(this.message.highPriConfigExtensions, this.message.otherExtensions, typeof(SoapExtension));
                }
                catch (Exception exception4)
                {
                    if (((exception4 is ThreadAbortException) || (exception4 is StackOverflowException)) || (exception4 is OutOfMemoryException))
                    {
                        throw;
                    }
                    if (Tracing.On)
                    {
                        Tracing.ExceptionCatch(TraceEventType.Warning, this, "Initialize", exception4);
                    }
                    innerException = exception4;
                }
            }
            if (innerException != null)
            {
                if (!this.isOneWay)
                {
                    if (innerException is SoapException)
                    {
                        throw innerException;
                    }
                    throw SoapException.Create(this.Version, System.Web.Services.Res.GetString("WebConfigExtensionError"), new XmlQualifiedName("Server", "http://schemas.xmlsoap.org/soap/envelope/"), innerException);
                }
                this.onewayInitException = innerException;
            }
            return true;
        }

        internal bool IsSupported(WebServiceProtocols protocol)
        {
            return ((this.protocolsSupported & protocol) == protocol);
        }

        protected virtual SoapExtension[] ModifyInitializedExtensions(PriorityGroup group, SoapExtension[] extensions)
        {
            return extensions;
        }

        internal override object[] ReadParameters()
        {
            object[] objArray2;
            this.message.InitExtensionStreamChain(this.message.otherExtensions);
            this.message.RunExtensions(this.message.otherExtensions, true);
            if (!ContentType.IsSoap(this.message.ContentType))
            {
                throw new SoapException(System.Web.Services.Res.GetString("WebRequestContent", new object[] { this.message.ContentType, this.helper.HttpContentType }), new XmlQualifiedName("Client", "http://schemas.xmlsoap.org/soap/envelope/"), new SoapFaultSubCode(Soap12FaultCodes.UnsupportedMediaTypeFaultCode));
            }
            XmlReader xmlReader = null;
            try
            {
                xmlReader = this.GetXmlReader();
                xmlReader.MoveToContent();
                this.SetHelper(SoapServerProtocolHelper.GetHelper(this, xmlReader.NamespaceURI));
            }
            catch (XmlException exception)
            {
                throw new SoapException(System.Web.Services.Res.GetString("WebRequestUnableToRead"), new XmlQualifiedName("Client", "http://schemas.xmlsoap.org/soap/envelope/"), exception);
            }
            this.CheckHelperVersion();
            if ((this.version == SoapProtocolVersion.Soap11) && !ContentType.MatchesBase(this.message.ContentType, this.helper.HttpContentType))
            {
                throw new SoapException(System.Web.Services.Res.GetString("WebRequestContent", new object[] { this.message.ContentType, this.helper.HttpContentType }), new XmlQualifiedName("Client", "http://schemas.xmlsoap.org/soap/envelope/"), new SoapFaultSubCode(Soap12FaultCodes.UnsupportedMediaTypeFaultCode));
            }
            if (this.message.Exception != null)
            {
                throw this.message.Exception;
            }
            try
            {
                object[] objArray;
                if (!xmlReader.IsStartElement("Envelope", this.helper.EnvelopeNs))
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebMissingEnvelopeElement"));
                }
                if (xmlReader.IsEmptyElement)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebMissingBodyElement"));
                }
                int depth = xmlReader.Depth;
                xmlReader.ReadStartElement("Envelope", this.helper.EnvelopeNs);
                xmlReader.MoveToContent();
                bool checkRequiredHeaders = ((this.serverMethod.wsiClaims & WsiProfiles.BasicProfile1_1) != WsiProfiles.None) && (this.version != SoapProtocolVersion.Soap12);
                string str = new SoapHeaderHandling().ReadHeaders(xmlReader, this.serverMethod.inHeaderSerializer, this.message.Headers, this.serverMethod.inHeaderMappings, SoapHeaderDirection.In, this.helper.EnvelopeNs, (this.serverMethod.use == SoapBindingUse.Encoded) ? this.helper.EncodingNs : null, checkRequiredHeaders);
                if (str != null)
                {
                    throw new SoapHeaderException(System.Web.Services.Res.GetString("WebMissingHeader", new object[] { str }), new XmlQualifiedName("MustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/"));
                }
                if (!xmlReader.IsStartElement("Body", this.helper.EnvelopeNs))
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebMissingBodyElement"));
                }
                xmlReader.ReadStartElement("Body", this.helper.EnvelopeNs);
                xmlReader.MoveToContent();
                bool flag2 = this.serverMethod.use == SoapBindingUse.Encoded;
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "ReadParameters", new object[0]) : null;
                if (Tracing.On)
                {
                    Tracing.Enter(Tracing.TraceId("TraceReadRequest"), caller, new TraceMethod(this.serverMethod.parameterSerializer, "Deserialize", new object[] { xmlReader, (this.serverMethod.use == SoapBindingUse.Encoded) ? this.helper.EncodingNs : null }));
                }
                if (!flag2 && (WebServicesSection.Current.SoapEnvelopeProcessing.IsStrict || Tracing.On))
                {
                    XmlDeserializationEvents events = Tracing.On ? Tracing.GetDeserializationEvents() : RuntimeUtils.GetDeserializationEvents();
                    objArray = (object[]) this.serverMethod.parameterSerializer.Deserialize(xmlReader, null, events);
                }
                else
                {
                    objArray = (object[]) this.serverMethod.parameterSerializer.Deserialize(xmlReader, flag2 ? this.helper.EncodingNs : null);
                }
                if (Tracing.On)
                {
                    Tracing.Exit(Tracing.TraceId("TraceReadRequest"), caller);
                }
                while ((depth < xmlReader.Depth) && xmlReader.Read())
                {
                }
                if (xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    xmlReader.Read();
                }
                this.message.SetParameterValues(objArray);
                objArray2 = objArray;
            }
            catch (SoapException)
            {
                throw;
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                throw new SoapException(System.Web.Services.Res.GetString("WebRequestUnableToRead"), new XmlQualifiedName("Client", "http://schemas.xmlsoap.org/soap/envelope/"), exception2);
            }
            return objArray2;
        }

        protected virtual SoapServerMethod RouteRequest(SoapServerMessage message)
        {
            return this.helper.RouteRequest();
        }

        private void SetHelper(SoapServerProtocolHelper helper)
        {
            this.helper = helper;
            this.version = helper.Version;
            base.Context.Items[WebService.SoapVersionContextSlot] = helper.Version;
        }

        internal override bool WriteException(Exception e, Stream outputStream)
        {
            SoapException exception;
            if (this.message == null)
            {
                return false;
            }
            this.message.Headers.Clear();
            if ((this.serverMethod != null) && (this.Target != null))
            {
                SoapHeaderHandling.GetHeaderMembers(this.message.Headers, this.Target, this.serverMethod.outHeaderMappings, SoapHeaderDirection.Fault, false);
            }
            if (e is SoapException)
            {
                exception = (SoapException) e;
            }
            else if (((this.serverMethod != null) && this.serverMethod.rpc) && ((this.helper.Version == SoapProtocolVersion.Soap12) && (e is ArgumentException)))
            {
                exception = SoapException.Create(this.Version, System.Web.Services.Res.GetString("WebRequestUnableToProcess"), new XmlQualifiedName("Client", "http://schemas.xmlsoap.org/soap/envelope/"), null, null, null, new SoapFaultSubCode(Soap12FaultCodes.RpcBadArgumentsFaultCode), e);
            }
            else
            {
                exception = SoapException.Create(this.Version, System.Web.Services.Res.GetString("WebRequestUnableToProcess"), new XmlQualifiedName("Server", "http://schemas.xmlsoap.org/soap/envelope/"), e);
            }
            if (SoapException.IsVersionMismatchFaultCode(exception.Code) && this.IsSupported(WebServiceProtocols.HttpSoap12))
            {
                SoapUnknownHeader header = this.CreateUpgradeHeader();
                if (header != null)
                {
                    this.Message.Headers.Add(header);
                }
            }
            base.Response.ClearHeaders();
            base.Response.Clear();
            HttpStatusCode statusCode = this.helper.SetResponseErrorCode(base.Response, exception);
            bool flag = false;
            SoapExtensionStream extensionStream = new SoapExtensionStream();
            if (this.message.allExtensions != null)
            {
                this.message.SetExtensionStream(extensionStream);
            }
            try
            {
                this.message.InitExtensionStreamChain(this.message.allExtensions);
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, this, "WriteException", exception2);
                }
                flag = true;
            }
            this.message.SetStage(SoapMessageStage.BeforeSerialize);
            this.message.ContentType = ContentType.Compose(this.helper.HttpContentType, Encoding.UTF8);
            this.message.Exception = exception;
            if (!flag)
            {
                try
                {
                    this.message.RunExtensions(this.message.allExtensions, false);
                }
                catch (Exception exception3)
                {
                    if (((exception3 is ThreadAbortException) || (exception3 is StackOverflowException)) || (exception3 is OutOfMemoryException))
                    {
                        throw;
                    }
                    if (Tracing.On)
                    {
                        Tracing.ExceptionCatch(TraceEventType.Warning, this, "WriteException", exception3);
                    }
                    flag = true;
                }
            }
            this.message.SetStream(outputStream);
            base.Response.ContentType = this.message.ContentType;
            if ((this.message.ContentEncoding != null) && (this.message.ContentEncoding.Length > 0))
            {
                base.Response.AppendHeader("Content-Encoding", this.message.ContentEncoding);
            }
            XmlWriter writerForMessage = this.GetWriterForMessage(this.message, 0x200);
            if (writerForMessage == null)
            {
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebNullWriterForMessage"));
            }
            this.helper.WriteFault(writerForMessage, this.message.Exception, statusCode);
            if (!flag)
            {
                SoapException exception4 = null;
                try
                {
                    this.message.SetStage(SoapMessageStage.AfterSerialize);
                    this.message.RunExtensions(this.message.allExtensions, false);
                }
                catch (Exception exception5)
                {
                    if (((exception5 is ThreadAbortException) || (exception5 is StackOverflowException)) || (exception5 is OutOfMemoryException))
                    {
                        throw;
                    }
                    if (Tracing.On)
                    {
                        Tracing.ExceptionCatch(TraceEventType.Warning, this, "WriteException", exception5);
                    }
                    if (!extensionStream.HasWritten)
                    {
                        exception4 = SoapException.Create(this.Version, System.Web.Services.Res.GetString("WebExtensionError"), new XmlQualifiedName("Server", "http://schemas.xmlsoap.org/soap/envelope/"), exception5);
                    }
                }
                if (exception4 != null)
                {
                    base.Response.ContentType = ContentType.Compose("text/plain", Encoding.UTF8);
                    StreamWriter writer2 = new StreamWriter(outputStream, new UTF8Encoding(false));
                    writer2.WriteLine(base.GenerateFaultString(this.message.Exception));
                    writer2.Flush();
                }
            }
            return true;
        }

        private bool WriteException_TryWriteFault(SoapServerMessage message, Stream outputStream, HttpStatusCode statusCode, bool disableExtensions)
        {
            return true;
        }

        internal override void WriteReturns(object[] returnValues, Stream outputStream)
        {
            if (!this.serverMethod.oneWay)
            {
                bool isEncoded = this.serverMethod.use == SoapBindingUse.Encoded;
                SoapHeaderHandling.EnsureHeadersUnderstood(this.message.Headers);
                this.message.Headers.Clear();
                SoapHeaderHandling.GetHeaderMembers(this.message.Headers, this.Target, this.serverMethod.outHeaderMappings, SoapHeaderDirection.Out, false);
                if (this.message.allExtensions != null)
                {
                    this.message.SetExtensionStream(new SoapExtensionStream());
                }
                this.message.InitExtensionStreamChain(this.message.allExtensions);
                this.message.SetStage(SoapMessageStage.BeforeSerialize);
                this.message.ContentType = ContentType.Compose(this.helper.HttpContentType, Encoding.UTF8);
                this.message.SetParameterValues(returnValues);
                this.message.RunExtensions(this.message.allExtensions, true);
                this.message.SetStream(outputStream);
                base.Response.ContentType = this.message.ContentType;
                if ((this.message.ContentEncoding != null) && (this.message.ContentEncoding.Length > 0))
                {
                    base.Response.AppendHeader("Content-Encoding", this.message.ContentEncoding);
                }
                XmlWriter writerForMessage = this.GetWriterForMessage(this.message, 0x400);
                if (writerForMessage == null)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebNullWriterForMessage"));
                }
                writerForMessage.WriteStartDocument();
                writerForMessage.WriteStartElement("soap", "Envelope", this.helper.EnvelopeNs);
                writerForMessage.WriteAttributeString("xmlns", "soap", null, this.helper.EnvelopeNs);
                if (isEncoded)
                {
                    writerForMessage.WriteAttributeString("xmlns", "soapenc", null, this.helper.EncodingNs);
                    writerForMessage.WriteAttributeString("xmlns", "tns", null, this.serverType.serviceNamespace);
                    writerForMessage.WriteAttributeString("xmlns", "types", null, SoapReflector.GetEncodedNamespace(this.serverType.serviceNamespace, this.serverType.serviceDefaultIsEncoded));
                }
                if (this.serverMethod.rpc && (this.version == SoapProtocolVersion.Soap12))
                {
                    writerForMessage.WriteAttributeString("xmlns", "rpc", null, "http://www.w3.org/2003/05/soap-rpc");
                }
                writerForMessage.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                writerForMessage.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
                SoapHeaderHandling.WriteHeaders(writerForMessage, this.serverMethod.outHeaderSerializer, this.message.Headers, this.serverMethod.outHeaderMappings, SoapHeaderDirection.Out, isEncoded, this.serverType.serviceNamespace, this.serverType.serviceDefaultIsEncoded, this.helper.EnvelopeNs);
                writerForMessage.WriteStartElement("Body", this.helper.EnvelopeNs);
                if (isEncoded && (this.version != SoapProtocolVersion.Soap12))
                {
                    writerForMessage.WriteAttributeString("soap", "encodingStyle", null, this.helper.EncodingNs);
                }
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "WriteReturns", new object[0]) : null;
                if (Tracing.On)
                {
                    object[] args = new object[4];
                    args[0] = writerForMessage;
                    args[1] = returnValues;
                    args[3] = isEncoded ? this.helper.EncodingNs : null;
                    Tracing.Enter(Tracing.TraceId("TraceWriteResponse"), caller, new TraceMethod(this.serverMethod.returnSerializer, "Serialize", args));
                }
                this.serverMethod.returnSerializer.Serialize(writerForMessage, returnValues, null, isEncoded ? this.helper.EncodingNs : null);
                if (Tracing.On)
                {
                    Tracing.Exit(Tracing.TraceId("TraceWriteResponse"), caller);
                }
                writerForMessage.WriteEndElement();
                writerForMessage.WriteEndElement();
                writerForMessage.Flush();
                this.message.SetStage(SoapMessageStage.AfterSerialize);
                this.message.RunExtensions(this.message.allExtensions, true);
            }
        }

        internal override bool IsOneWay
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isOneWay;
            }
        }

        internal SoapServerMessage Message
        {
            get
            {
                return this.message;
            }
        }

        internal override LogicalMethodInfo MethodInfo
        {
            get
            {
                return this.serverMethod.methodInfo;
            }
        }

        internal override Exception OnewayInitException
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.onewayInitException;
            }
        }

        internal SoapServerMethod ServerMethod
        {
            get
            {
                return this.serverMethod;
            }
        }

        internal override System.Web.Services.Protocols.ServerType ServerType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serverType;
            }
        }

        internal SoapProtocolVersion Version
        {
            get
            {
                return this.version;
            }
        }

        internal class SoapEnvelopeReader : XmlTextReader
        {
            private long readerTimedout;

            internal SoapEnvelopeReader(Stream input, long timeout) : base(input)
            {
                this.readerTimedout = timeout;
            }

            internal SoapEnvelopeReader(TextReader input, long timeout) : base(input)
            {
                this.readerTimedout = timeout;
            }

            private void CheckTimeout()
            {
                if (DateTime.UtcNow.Ticks > this.readerTimedout)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebTimeout"));
                }
            }

            public override XmlNodeType MoveToContent()
            {
                this.CheckTimeout();
                return base.MoveToContent();
            }

            public override bool MoveToNextAttribute()
            {
                this.CheckTimeout();
                return base.MoveToNextAttribute();
            }

            public override bool Read()
            {
                this.CheckTimeout();
                return base.Read();
            }
        }
    }
}

