namespace System.Web.Services.Protocols
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Description;
    using System.Web.Services.Diagnostics;
    using System.Web.Services.Discovery;
    using System.Xml;
    using System.Xml.Serialization;

    [ComVisible(true)]
    public class SoapHttpClientProtocol : HttpWebClientProtocol
    {
        private SoapClientType clientType;
        private SoapProtocolVersion version;

        public SoapHttpClientProtocol()
        {
            Type type = base.GetType();
            this.clientType = (SoapClientType) WebClientProtocol.GetFromCache(type);
            if (this.clientType == null)
            {
                lock (WebClientProtocol.InternalSyncObject)
                {
                    this.clientType = (SoapClientType) WebClientProtocol.GetFromCache(type);
                    if (this.clientType == null)
                    {
                        this.clientType = new SoapClientType(type);
                        WebClientProtocol.AddToCache(type, this.clientType);
                    }
                }
            }
        }

        internal override void AsyncBufferedSerialize(WebRequest request, Stream requestStream, object internalAsyncState)
        {
            InvokeAsyncState state = (InvokeAsyncState) internalAsyncState;
            state.Message.SetStream(requestStream);
            this.Serialize(state.Message);
        }

        private SoapClientMessage BeforeSerialize(WebRequest request, string methodName, object[] parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            SoapClientMethod method = this.clientType.GetMethod(methodName);
            if (method == null)
            {
                throw new ArgumentException(System.Web.Services.Res.GetString("WebInvalidMethodName", new object[] { methodName }));
            }
            SoapReflectedExtension[] reflectedExtensions = (SoapReflectedExtension[]) CombineExtensionsHelper(this.clientType.HighPriExtensions, method.extensions, this.clientType.LowPriExtensions, typeof(SoapReflectedExtension));
            object[] extensionInitializers = (object[]) CombineExtensionsHelper(this.clientType.HighPriExtensionInitializers, method.extensionInitializers, this.clientType.LowPriExtensionInitializers, typeof(object));
            SoapExtension[] extensionArray2 = SoapMessage.InitializeExtensions(reflectedExtensions, extensionInitializers);
            SoapClientMessage message = new SoapClientMessage(this, method, base.Url) {
                initializedExtensions = extensionArray2
            };
            if (extensionArray2 != null)
            {
                message.SetExtensionStream(new SoapExtensionStream());
            }
            message.InitExtensionStreamChain(message.initializedExtensions);
            string action = UrlEncoder.EscapeString(method.action, Encoding.UTF8);
            message.SetStage(SoapMessageStage.BeforeSerialize);
            if (this.version == SoapProtocolVersion.Soap12)
            {
                message.ContentType = ContentType.Compose("application/soap+xml", (base.RequestEncoding != null) ? base.RequestEncoding : Encoding.UTF8, action);
            }
            else
            {
                message.ContentType = ContentType.Compose("text/xml", (base.RequestEncoding != null) ? base.RequestEncoding : Encoding.UTF8);
            }
            message.SetParameterValues(parameters);
            SoapHeaderHandling.GetHeaderMembers(message.Headers, this, method.inHeaderMappings, SoapHeaderDirection.In, true);
            message.RunExtensions(message.initializedExtensions, true);
            request.ContentType = message.ContentType;
            if ((message.ContentEncoding != null) && (message.ContentEncoding.Length > 0))
            {
                request.Headers["Content-Encoding"] = message.ContentEncoding;
            }
            request.Method = "POST";
            if ((this.version != SoapProtocolVersion.Soap12) && (request.Headers["SOAPAction"] == null))
            {
                StringBuilder builder = new StringBuilder(action.Length + 2);
                builder.Append('"');
                builder.Append(action);
                builder.Append('"');
                request.Headers.Add("SOAPAction", builder.ToString());
            }
            return message;
        }

        protected IAsyncResult BeginInvoke(string methodName, object[] parameters, AsyncCallback callback, object asyncState)
        {
            InvokeAsyncState internalAsyncState = new InvokeAsyncState(methodName, parameters);
            WebClientAsyncResult asyncResult = new WebClientAsyncResult(this, internalAsyncState, null, callback, asyncState);
            return base.BeginSend(base.Uri, asyncResult, true);
        }

        private static Array CombineExtensionsHelper(Array array1, Array array2, Array array3, Type elementType)
        {
            int num = (array1.Length + array2.Length) + array3.Length;
            if (num == 0)
            {
                return null;
            }
            Array destinationArray = null;
            if (elementType == typeof(SoapReflectedExtension))
            {
                destinationArray = new SoapReflectedExtension[num];
            }
            else
            {
                if (elementType != typeof(object))
                {
                    throw new ArgumentException(System.Web.Services.Res.GetString("ElementTypeMustBeObjectOrSoapReflectedException"), "elementType");
                }
                destinationArray = new object[num];
            }
            int destinationIndex = 0;
            Array.Copy(array1, 0, destinationArray, destinationIndex, array1.Length);
            destinationIndex += array1.Length;
            Array.Copy(array2, 0, destinationArray, destinationIndex, array2.Length);
            destinationIndex += array2.Length;
            Array.Copy(array3, 0, destinationArray, destinationIndex, array3.Length);
            return destinationArray;
        }

        public void Discover()
        {
            if (this.clientType.Binding == null)
            {
                throw new InvalidOperationException(System.Web.Services.Res.GetString("DiscoveryIsNotPossibleBecauseTypeIsMissing1", new object[] { base.GetType().FullName }));
            }
            DiscoveryClientProtocol protocol = new DiscoveryClientProtocol(this);
            foreach (object obj2 in protocol.Discover(base.Url).References)
            {
                System.Web.Services.Discovery.SoapBinding binding = obj2 as System.Web.Services.Discovery.SoapBinding;
                if (((binding != null) && (this.clientType.Binding.Name == binding.Binding.Name)) && (this.clientType.Binding.Namespace == binding.Binding.Namespace))
                {
                    base.Url = binding.Address;
                    return;
                }
            }
            throw new InvalidOperationException(System.Web.Services.Res.GetString("TheBindingNamedFromNamespaceWasNotFoundIn3", new object[] { this.clientType.Binding.Name, this.clientType.Binding.Namespace, base.Url }));
        }

        protected object[] EndInvoke(IAsyncResult asyncResult)
        {
            object internalAsyncState = null;
            Stream responseStream = null;
            object[] objArray;
            try
            {
                WebResponse response = base.EndSend(asyncResult, ref internalAsyncState, ref responseStream);
                InvokeAsyncState state = (InvokeAsyncState) internalAsyncState;
                objArray = this.ReadResponse(state.Message, response, responseStream, true);
            }
            catch (XmlException exception)
            {
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebResponseBadXml"), exception);
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                }
            }
            return objArray;
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        protected virtual XmlReader GetReaderForMessage(SoapClientMessage message, int bufferSize)
        {
            XmlTextReader reader;
            Encoding encoding = (message.SoapVersion == SoapProtocolVersion.Soap12) ? RequestResponseUtils.GetEncoding2(message.ContentType) : RequestResponseUtils.GetEncoding(message.ContentType);
            if (bufferSize < 0x200)
            {
                bufferSize = 0x200;
            }
            if (encoding != null)
            {
                reader = new XmlTextReader(new StreamReader(message.Stream, encoding, true, bufferSize));
            }
            else
            {
                reader = new XmlTextReader(message.Stream);
            }
            reader.DtdProcessing = DtdProcessing.Prohibit;
            reader.Normalization = true;
            reader.XmlResolver = null;
            return reader;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            return base.GetWebRequest(uri);
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        protected virtual XmlWriter GetWriterForMessage(SoapClientMessage message, int bufferSize)
        {
            if (bufferSize < 0x200)
            {
                bufferSize = 0x200;
            }
            return new XmlTextWriter(new StreamWriter(message.Stream, (base.RequestEncoding != null) ? base.RequestEncoding : new UTF8Encoding(false), bufferSize));
        }

        internal override void InitializeAsyncRequest(WebRequest request, object internalAsyncState)
        {
            InvokeAsyncState state = (InvokeAsyncState) internalAsyncState;
            state.Message = this.BeforeSerialize(request, state.MethodName, state.Parameters);
        }

        protected object[] Invoke(string methodName, object[] parameters)
        {
            WebResponse webResponse = null;
            WebRequest webRequest = null;
            object[] objArray;
            try
            {
                webRequest = this.GetWebRequest(base.Uri);
                base.NotifyClientCallOut(webRequest);
                base.PendingSyncRequest = webRequest;
                SoapClientMessage message = this.BeforeSerialize(webRequest, methodName, parameters);
                Stream requestStream = webRequest.GetRequestStream();
                try
                {
                    message.SetStream(requestStream);
                    this.Serialize(message);
                }
                finally
                {
                    requestStream.Close();
                }
                webResponse = this.GetWebResponse(webRequest);
                Stream responseStream = null;
                try
                {
                    responseStream = webResponse.GetResponseStream();
                    objArray = this.ReadResponse(message, webResponse, responseStream, false);
                }
                catch (XmlException exception)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebResponseBadXml"), exception);
                }
                finally
                {
                    if (responseStream != null)
                    {
                        responseStream.Close();
                    }
                }
            }
            finally
            {
                if (webRequest == base.PendingSyncRequest)
                {
                    base.PendingSyncRequest = null;
                }
            }
            return objArray;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected void InvokeAsync(string methodName, object[] parameters, SendOrPostCallback callback)
        {
            this.InvokeAsync(methodName, parameters, callback, null);
        }

        protected void InvokeAsync(string methodName, object[] parameters, SendOrPostCallback callback, object userState)
        {
            if (userState == null)
            {
                userState = base.NullToken;
            }
            InvokeAsyncState internalAsyncState = new InvokeAsyncState(methodName, parameters);
            AsyncOperation userAsyncState = AsyncOperationManager.CreateOperation(new UserToken(callback, userState));
            WebClientAsyncResult result = new WebClientAsyncResult(this, internalAsyncState, null, new AsyncCallback(this.InvokeAsyncCallback), userAsyncState);
            try
            {
                base.AsyncInvokes.Add(userState, result);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, this, "InvokeAsync", exception);
                }
                Exception exception2 = new ArgumentException(System.Web.Services.Res.GetString("AsyncDuplicateUserState"), exception);
                object[] results = new object[1];
                InvokeCompletedEventArgs arg = new InvokeCompletedEventArgs(results, exception2, false, userState);
                userAsyncState.PostOperationCompleted(callback, arg);
                return;
            }
            try
            {
                base.BeginSend(base.Uri, result, true);
            }
            catch (Exception exception3)
            {
                if (((exception3 is ThreadAbortException) || (exception3 is StackOverflowException)) || (exception3 is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, this, "InvokeAsync", exception3);
                }
                object[] objArray2 = new object[1];
                base.OperationCompleted(userState, objArray2, exception3, false);
            }
        }

        private void InvokeAsyncCallback(IAsyncResult result)
        {
            object[] parameters = null;
            Exception e = null;
            WebClientAsyncResult asyncResult = (WebClientAsyncResult) result;
            if (asyncResult.Request != null)
            {
                object internalAsyncState = null;
                Stream responseStream = null;
                try
                {
                    WebResponse response = base.EndSend(asyncResult, ref internalAsyncState, ref responseStream);
                    InvokeAsyncState state = (InvokeAsyncState) internalAsyncState;
                    parameters = this.ReadResponse(state.Message, response, responseStream, true);
                }
                catch (XmlException exception2)
                {
                    if (Tracing.On)
                    {
                        Tracing.ExceptionCatch(TraceEventType.Warning, this, "InvokeAsyncCallback", exception2);
                    }
                    e = new InvalidOperationException(System.Web.Services.Res.GetString("WebResponseBadXml"), exception2);
                }
                catch (Exception exception3)
                {
                    if (((exception3 is ThreadAbortException) || (exception3 is StackOverflowException)) || (exception3 is OutOfMemoryException))
                    {
                        throw;
                    }
                    if (Tracing.On)
                    {
                        Tracing.ExceptionCatch(TraceEventType.Warning, this, "InvokeAsyncCallback", exception3);
                    }
                    e = exception3;
                }
                finally
                {
                    if (responseStream != null)
                    {
                        responseStream.Close();
                    }
                }
            }
            AsyncOperation asyncState = (AsyncOperation) result.AsyncState;
            UserToken userSuppliedState = (UserToken) asyncState.UserSuppliedState;
            base.OperationCompleted(userSuppliedState.UserState, parameters, e, false);
        }

        private XmlQualifiedName ReadFaultCode(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Skip();
                return null;
            }
            reader.ReadStartElement();
            string str = reader.ReadString();
            int index = str.IndexOf(":", StringComparison.Ordinal);
            string namespaceURI = reader.NamespaceURI;
            if (index >= 0)
            {
                string prefix = str.Substring(0, index);
                namespaceURI = reader.LookupNamespace(prefix);
                if (namespaceURI == null)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebQNamePrefixUndefined", new object[] { prefix }));
                }
            }
            reader.ReadEndElement();
            return new XmlQualifiedName(str.Substring(index + 1), namespaceURI);
        }

        private object[] ReadResponse(SoapClientMessage message, WebResponse response, Stream responseStream, bool asyncCall)
        {
            int bufferSize;
            SoapClientMethod method = message.Method;
            HttpWebResponse response2 = response as HttpWebResponse;
            int num = (response2 != null) ? ((int) response2.StatusCode) : -1;
            if (((num >= 300) && (num != 500)) && (num != 400))
            {
                throw new WebException(RequestResponseUtils.CreateResponseExceptionString(response2, responseStream), null, WebExceptionStatus.ProtocolError, response2);
            }
            message.Headers.Clear();
            message.SetStream(responseStream);
            message.InitExtensionStreamChain(message.initializedExtensions);
            message.SetStage(SoapMessageStage.BeforeDeserialize);
            message.ContentType = response.ContentType;
            message.ContentEncoding = response.Headers["Content-Encoding"];
            message.RunExtensions(message.initializedExtensions, false);
            if (method.oneWay && ((response2 == null) || (response2.StatusCode != HttpStatusCode.InternalServerError)))
            {
                return new object[0];
            }
            bool flag = ContentType.IsSoap(message.ContentType);
            if (!flag || ((flag && (response2 != null)) && (response2.ContentLength == 0L)))
            {
                if (num == 400)
                {
                    throw new WebException(RequestResponseUtils.CreateResponseExceptionString(response2, responseStream), null, WebExceptionStatus.ProtocolError, response2);
                }
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebResponseContent", new object[] { message.ContentType, this.HttpContentType }) + Environment.NewLine + RequestResponseUtils.CreateResponseExceptionString(response, responseStream));
            }
            if (message.Exception != null)
            {
                throw message.Exception;
            }
            if (asyncCall || (response2 == null))
            {
                bufferSize = 0x200;
            }
            else
            {
                bufferSize = RequestResponseUtils.GetBufferSize((int) response2.ContentLength);
            }
            XmlReader readerForMessage = this.GetReaderForMessage(message, bufferSize);
            if (readerForMessage == null)
            {
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebNullReaderForMessage"));
            }
            readerForMessage.MoveToContent();
            int depth = readerForMessage.Depth;
            string encodingNs = this.EncodingNs;
            string namespaceURI = readerForMessage.NamespaceURI;
            if ((namespaceURI == null) || (namespaceURI.Length == 0))
            {
                readerForMessage.ReadStartElement("Envelope");
            }
            else if (readerForMessage.NamespaceURI == "http://schemas.xmlsoap.org/soap/envelope/")
            {
                readerForMessage.ReadStartElement("Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
            }
            else
            {
                if (readerForMessage.NamespaceURI != "http://www.w3.org/2003/05/soap-envelope")
                {
                    throw new SoapException(System.Web.Services.Res.GetString("WebInvalidEnvelopeNamespace", new object[] { namespaceURI, this.EnvelopeNs }), SoapException.VersionMismatchFaultCode);
                }
                readerForMessage.ReadStartElement("Envelope", "http://www.w3.org/2003/05/soap-envelope");
            }
            readerForMessage.MoveToContent();
            new SoapHeaderHandling().ReadHeaders(readerForMessage, method.outHeaderSerializer, message.Headers, method.outHeaderMappings, SoapHeaderDirection.Fault | SoapHeaderDirection.Out, namespaceURI, (method.use == SoapBindingUse.Encoded) ? encodingNs : null, false);
            readerForMessage.MoveToContent();
            readerForMessage.ReadStartElement("Body", namespaceURI);
            readerForMessage.MoveToContent();
            if (readerForMessage.IsStartElement("Fault", namespaceURI))
            {
                message.Exception = this.ReadSoapException(readerForMessage);
            }
            else if (method.oneWay)
            {
                readerForMessage.Skip();
                message.SetParameterValues(new object[0]);
            }
            else
            {
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "ReadResponse", new object[0]) : null;
                bool flag2 = method.use == SoapBindingUse.Encoded;
                if (Tracing.On)
                {
                    Tracing.Enter(Tracing.TraceId("TraceReadResponse"), caller, new TraceMethod(method.returnSerializer, "Deserialize", new object[] { readerForMessage, flag2 ? encodingNs : null }));
                }
                if (!flag2 && (WebServicesSection.Current.SoapEnvelopeProcessing.IsStrict || Tracing.On))
                {
                    XmlDeserializationEvents events = Tracing.On ? Tracing.GetDeserializationEvents() : RuntimeUtils.GetDeserializationEvents();
                    message.SetParameterValues((object[]) method.returnSerializer.Deserialize(readerForMessage, null, events));
                }
                else
                {
                    message.SetParameterValues((object[]) method.returnSerializer.Deserialize(readerForMessage, flag2 ? encodingNs : null));
                }
                if (Tracing.On)
                {
                    Tracing.Exit(Tracing.TraceId("TraceReadResponse"), caller);
                }
            }
            while ((depth < readerForMessage.Depth) && readerForMessage.Read())
            {
            }
            if (readerForMessage.NodeType == XmlNodeType.EndElement)
            {
                readerForMessage.Read();
            }
            message.SetStage(SoapMessageStage.AfterDeserialize);
            message.RunExtensions(message.initializedExtensions, false);
            SoapHeaderHandling.SetHeaderMembers(message.Headers, this, method.outHeaderMappings, SoapHeaderDirection.Fault | SoapHeaderDirection.Out, true);
            if (message.Exception != null)
            {
                throw message.Exception;
            }
            return message.GetParameterValues();
        }

        private XmlQualifiedName ReadSoap12FaultCode(XmlReader reader, out SoapFaultSubCode subcode)
        {
            SoapFaultSubCode code = this.ReadSoap12FaultCodesRecursive(reader, 0);
            if (code == null)
            {
                subcode = null;
                return null;
            }
            subcode = code.SubCode;
            return code.Code;
        }

        private SoapFaultSubCode ReadSoap12FaultCodesRecursive(XmlReader reader, int depth)
        {
            if (depth > 100)
            {
                return null;
            }
            if (reader.IsEmptyElement)
            {
                reader.Skip();
                return null;
            }
            XmlQualifiedName name = null;
            SoapFaultSubCode subCode = null;
            int num = reader.Depth;
            reader.ReadStartElement();
            reader.MoveToContent();
            while ((reader.NodeType != XmlNodeType.EndElement) && (reader.NodeType != XmlNodeType.None))
            {
                if (((reader.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope") || (reader.NamespaceURI == null)) || (reader.NamespaceURI.Length == 0))
                {
                    if (reader.LocalName == "Value")
                    {
                        name = this.ReadFaultCode(reader);
                    }
                    else if (reader.LocalName == "Subcode")
                    {
                        subCode = this.ReadSoap12FaultCodesRecursive(reader, depth + 1);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
                else
                {
                    reader.Skip();
                }
                reader.MoveToContent();
            }
            while ((num < reader.Depth) && reader.Read())
            {
            }
            if (reader.NodeType == XmlNodeType.EndElement)
            {
                reader.Read();
            }
            return new SoapFaultSubCode(name, subCode);
        }

        private SoapException ReadSoapException(XmlReader reader)
        {
            XmlQualifiedName empty = XmlQualifiedName.Empty;
            string message = null;
            string actor = null;
            string role = null;
            XmlNode detail = null;
            SoapFaultSubCode subcode = null;
            string lang = null;
            bool flag = reader.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope";
            if (reader.IsEmptyElement)
            {
                reader.Skip();
            }
            else
            {
                reader.ReadStartElement();
                reader.MoveToContent();
                int depth = reader.Depth;
                while ((reader.NodeType != XmlNodeType.EndElement) && (reader.NodeType != XmlNodeType.None))
                {
                    if (((reader.NamespaceURI == "http://schemas.xmlsoap.org/soap/envelope/") || (reader.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope")) || ((reader.NamespaceURI == null) || (reader.NamespaceURI.Length == 0)))
                    {
                        if ((reader.LocalName == "faultcode") || (reader.LocalName == "Code"))
                        {
                            if (flag)
                            {
                                empty = this.ReadSoap12FaultCode(reader, out subcode);
                            }
                            else
                            {
                                empty = this.ReadFaultCode(reader);
                            }
                        }
                        else if (reader.LocalName == "faultstring")
                        {
                            lang = reader.GetAttribute("lang", "http://www.w3.org/XML/1998/namespace");
                            reader.MoveToElement();
                            message = reader.ReadElementString();
                        }
                        else if (reader.LocalName == "Reason")
                        {
                            if (reader.IsEmptyElement)
                            {
                                reader.Skip();
                            }
                            else
                            {
                                reader.ReadStartElement();
                                reader.MoveToContent();
                                while ((reader.NodeType != XmlNodeType.EndElement) && (reader.NodeType != XmlNodeType.None))
                                {
                                    if ((reader.LocalName == "Text") && (reader.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope"))
                                    {
                                        message = reader.ReadElementString();
                                    }
                                    else
                                    {
                                        reader.Skip();
                                    }
                                    reader.MoveToContent();
                                }
                                while (reader.NodeType == XmlNodeType.Whitespace)
                                {
                                    reader.Skip();
                                }
                                if (reader.NodeType == XmlNodeType.None)
                                {
                                    reader.Skip();
                                }
                                else
                                {
                                    reader.ReadEndElement();
                                }
                            }
                        }
                        else if ((reader.LocalName == "faultactor") || (reader.LocalName == "Node"))
                        {
                            actor = reader.ReadElementString();
                        }
                        else if ((reader.LocalName == "detail") || (reader.LocalName == "Detail"))
                        {
                            detail = new XmlDocument().ReadNode(reader);
                        }
                        else if (reader.LocalName == "Role")
                        {
                            role = reader.ReadElementString();
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                    reader.MoveToContent();
                }
                while (reader.Read() && (depth < reader.Depth))
                {
                }
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    reader.Read();
                }
            }
            if ((detail == null) && !flag)
            {
                return new SoapHeaderException(message, empty, actor, role, lang, subcode, null);
            }
            return new SoapException(message, empty, actor, role, lang, detail, subcode, null);
        }

        private void Serialize(SoapClientMessage message)
        {
            Stream stream = message.Stream;
            SoapClientMethod method = message.Method;
            bool isEncoded = method.use == SoapBindingUse.Encoded;
            string envelopeNs = this.EnvelopeNs;
            string encodingNs = this.EncodingNs;
            XmlWriter writerForMessage = this.GetWriterForMessage(message, 0x400);
            if (writerForMessage == null)
            {
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebNullWriterForMessage"));
            }
            writerForMessage.WriteStartDocument();
            writerForMessage.WriteStartElement("soap", "Envelope", envelopeNs);
            writerForMessage.WriteAttributeString("xmlns", "soap", null, envelopeNs);
            if (isEncoded)
            {
                writerForMessage.WriteAttributeString("xmlns", "soapenc", null, encodingNs);
                writerForMessage.WriteAttributeString("xmlns", "tns", null, this.clientType.serviceNamespace);
                writerForMessage.WriteAttributeString("xmlns", "types", null, SoapReflector.GetEncodedNamespace(this.clientType.serviceNamespace, this.clientType.serviceDefaultIsEncoded));
            }
            writerForMessage.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            writerForMessage.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
            SoapHeaderHandling.WriteHeaders(writerForMessage, method.inHeaderSerializer, message.Headers, method.inHeaderMappings, SoapHeaderDirection.In, isEncoded, this.clientType.serviceNamespace, this.clientType.serviceDefaultIsEncoded, envelopeNs);
            writerForMessage.WriteStartElement("Body", envelopeNs);
            if (isEncoded && (this.version != SoapProtocolVersion.Soap12))
            {
                writerForMessage.WriteAttributeString("soap", "encodingStyle", null, encodingNs);
            }
            object[] parameterValues = message.GetParameterValues();
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "Serialize", new object[0]) : null;
            if (Tracing.On)
            {
                object[] args = new object[4];
                args[0] = writerForMessage;
                args[1] = parameterValues;
                args[3] = isEncoded ? encodingNs : null;
                Tracing.Enter(Tracing.TraceId("TraceWriteRequest"), caller, new TraceMethod(method.parameterSerializer, "Serialize", args));
            }
            method.parameterSerializer.Serialize(writerForMessage, parameterValues, null, isEncoded ? encodingNs : null);
            if (Tracing.On)
            {
                Tracing.Exit(Tracing.TraceId("TraceWriteRequest"), caller);
            }
            writerForMessage.WriteEndElement();
            writerForMessage.WriteEndElement();
            writerForMessage.Flush();
            message.SetStage(SoapMessageStage.AfterSerialize);
            message.RunExtensions(message.initializedExtensions, true);
        }

        private string EncodingNs
        {
            get
            {
                if (this.version != SoapProtocolVersion.Soap12)
                {
                    return "http://schemas.xmlsoap.org/soap/encoding/";
                }
                return "http://www.w3.org/2003/05/soap-encoding";
            }
        }

        private string EnvelopeNs
        {
            get
            {
                if (this.version != SoapProtocolVersion.Soap12)
                {
                    return "http://schemas.xmlsoap.org/soap/envelope/";
                }
                return "http://www.w3.org/2003/05/soap-envelope";
            }
        }

        private string HttpContentType
        {
            get
            {
                if (this.version != SoapProtocolVersion.Soap12)
                {
                    return "text/xml";
                }
                return "application/soap+xml";
            }
        }

        [DefaultValue(0), ComVisible(false), WebServicesDescription("ClientProtocolSoapVersion")]
        public SoapProtocolVersion SoapVersion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.version;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.version = value;
            }
        }

        private class InvokeAsyncState
        {
            public SoapClientMessage Message;
            public string MethodName;
            public object[] Parameters;

            public InvokeAsyncState(string methodName, object[] parameters)
            {
                this.MethodName = methodName;
                this.Parameters = parameters;
            }
        }
    }
}

