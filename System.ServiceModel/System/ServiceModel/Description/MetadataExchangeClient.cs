namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Mime;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;

    public class MetadataExchangeClient
    {
        private EndpointAddress ctorEndpointAddress;
        private Uri ctorUri;
        private ChannelFactory<IMetadataExchange> factory;
        private int maximumResolvedReferences;
        private long maxMessageSize;
        internal const string MetadataExchangeClientKey = "MetadataExchangeClientKey";
        private XmlDictionaryReaderQuotas readerQuotas;
        private bool resolveMetadataReferences;
        private TimeSpan resolveTimeout;
        private object thisLock;
        private ICredentials webRequestCredentials;

        public MetadataExchangeClient()
        {
            this.resolveTimeout = TimeSpan.FromMinutes(1.0);
            this.maximumResolvedReferences = 10;
            this.resolveMetadataReferences = true;
            this.thisLock = new object();
            this.factory = new ChannelFactory<IMetadataExchange>("*");
            this.maxMessageSize = GetMaxMessageSize(this.factory.Endpoint.Binding);
        }

        public MetadataExchangeClient(System.ServiceModel.Channels.Binding mexBinding)
        {
            this.resolveTimeout = TimeSpan.FromMinutes(1.0);
            this.maximumResolvedReferences = 10;
            this.resolveMetadataReferences = true;
            this.thisLock = new object();
            if (mexBinding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("mexBinding");
            }
            this.factory = new ChannelFactory<IMetadataExchange>(mexBinding);
            this.maxMessageSize = GetMaxMessageSize(this.factory.Endpoint.Binding);
        }

        public MetadataExchangeClient(EndpointAddress address)
        {
            this.resolveTimeout = TimeSpan.FromMinutes(1.0);
            this.maximumResolvedReferences = 10;
            this.resolveMetadataReferences = true;
            this.thisLock = new object();
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            this.ctorEndpointAddress = address;
            this.CreateChannelFactory(address.Uri.Scheme);
        }

        public MetadataExchangeClient(string endpointConfigurationName)
        {
            this.resolveTimeout = TimeSpan.FromMinutes(1.0);
            this.maximumResolvedReferences = 10;
            this.resolveMetadataReferences = true;
            this.thisLock = new object();
            if (endpointConfigurationName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
            }
            this.factory = new ChannelFactory<IMetadataExchange>(endpointConfigurationName);
            this.maxMessageSize = GetMaxMessageSize(this.factory.Endpoint.Binding);
        }

        public MetadataExchangeClient(Uri address, MetadataExchangeClientMode mode)
        {
            this.resolveTimeout = TimeSpan.FromMinutes(1.0);
            this.maximumResolvedReferences = 10;
            this.resolveMetadataReferences = true;
            this.thisLock = new object();
            this.Validate(address, mode);
            if (mode == MetadataExchangeClientMode.HttpGet)
            {
                this.ctorUri = address;
            }
            else
            {
                this.ctorEndpointAddress = new EndpointAddress(address, new AddressHeader[0]);
            }
            this.CreateChannelFactory(address.Scheme);
        }

        public IAsyncResult BeginGetMetadata(AsyncCallback callback, object asyncState)
        {
            if (this.ctorUri != null)
            {
                return this.BeginGetMetadata(this.ctorUri, MetadataExchangeClientMode.HttpGet, callback, asyncState);
            }
            if (this.ctorEndpointAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMetadataExchangeClientNoMetadataAddress")));
            }
            return this.BeginGetMetadata(this.ctorEndpointAddress, callback, asyncState);
        }

        private IAsyncResult BeginGetMetadata(MetadataRetriever retriever, AsyncCallback callback, object asyncState)
        {
            ResolveCallState resolveCallState = new ResolveCallState(this.maximumResolvedReferences, this.resolveMetadataReferences, new TimeoutHelper(this.OperationTimeout), this);
            resolveCallState.StackedRetrievers.Push(retriever);
            return new AsyncMetadataResolver(resolveCallState, callback, asyncState);
        }

        public IAsyncResult BeginGetMetadata(EndpointAddress address, AsyncCallback callback, object asyncState)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            return this.BeginGetMetadata(new MetadataReferenceRetriever(address, this), callback, asyncState);
        }

        public IAsyncResult BeginGetMetadata(Uri address, MetadataExchangeClientMode mode, AsyncCallback callback, object asyncState)
        {
            this.Validate(address, mode);
            if (mode == MetadataExchangeClientMode.HttpGet)
            {
                return this.BeginGetMetadata(new MetadataLocationRetriever(address, this), callback, asyncState);
            }
            return this.BeginGetMetadata(new MetadataReferenceRetriever(new EndpointAddress(address, new AddressHeader[0]), this), callback, asyncState);
        }

        [SecuritySafeCritical]
        private bool ClientEndpointExists(string name)
        {
            ClientSection section = ClientSection.UnsafeGetSection();
            if (section != null)
            {
                foreach (ChannelEndpointElement element in section.Endpoints)
                {
                    if ((element.Name == name) && (element.Contract == "IMetadataExchange"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void CreateChannelFactory(string scheme)
        {
            if (this.ClientEndpointExists(scheme))
            {
                this.factory = new ChannelFactory<IMetadataExchange>(scheme);
            }
            else
            {
                System.ServiceModel.Channels.Binding binding = null;
                if (!MetadataExchangeBindings.TryGetBindingForScheme(scheme, out binding))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("scheme", System.ServiceModel.SR.GetString("SFxMetadataExchangeClientCouldNotCreateChannelFactoryBadScheme", new object[] { scheme }));
                }
                this.factory = new ChannelFactory<IMetadataExchange>(binding);
            }
            this.maxMessageSize = GetMaxMessageSize(this.factory.Endpoint.Binding);
        }

        public MetadataSet EndGetMetadata(IAsyncResult result)
        {
            return AsyncMetadataResolver.End(result);
        }

        protected internal virtual ChannelFactory<IMetadataExchange> GetChannelFactory(EndpointAddress metadataAddress, string dialect, string identifier)
        {
            return this.factory;
        }

        private static long GetMaxMessageSize(System.ServiceModel.Channels.Binding mexBinding)
        {
            TransportBindingElement element = mexBinding.CreateBindingElements().Find<TransportBindingElement>();
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBindingDoesNotHaveATransportBindingElement")));
            }
            return element.MaxReceivedMessageSize;
        }

        public MetadataSet GetMetadata()
        {
            if (this.ctorUri != null)
            {
                return this.GetMetadata(this.ctorUri, MetadataExchangeClientMode.HttpGet);
            }
            if (this.ctorEndpointAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMetadataExchangeClientNoMetadataAddress")));
            }
            return this.GetMetadata(this.ctorEndpointAddress);
        }

        private MetadataSet GetMetadata(MetadataRetriever retriever)
        {
            ResolveCallState resolveCallState = new ResolveCallState(this.maximumResolvedReferences, this.resolveMetadataReferences, new TimeoutHelper(this.OperationTimeout), this);
            resolveCallState.StackedRetrievers.Push(retriever);
            this.ResolveNext(resolveCallState);
            return resolveCallState.MetadataSet;
        }

        public MetadataSet GetMetadata(EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            MetadataReferenceRetriever retriever = new MetadataReferenceRetriever(address, this);
            return this.GetMetadata(retriever);
        }

        public MetadataSet GetMetadata(EndpointAddress address, Uri via)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            if (via == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("via");
            }
            MetadataReferenceRetriever retriever = new MetadataReferenceRetriever(address, via, this);
            return this.GetMetadata(retriever);
        }

        public MetadataSet GetMetadata(Uri address, MetadataExchangeClientMode mode)
        {
            MetadataRetriever retriever;
            this.Validate(address, mode);
            if (mode == MetadataExchangeClientMode.HttpGet)
            {
                retriever = new MetadataLocationRetriever(address, this);
            }
            else
            {
                retriever = new MetadataReferenceRetriever(new EndpointAddress(address, new AddressHeader[0]), this);
            }
            return this.GetMetadata(retriever);
        }

        protected internal virtual HttpWebRequest GetWebRequest(Uri location, string dialect, string identifier)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(location);
            request.Method = "GET";
            request.Credentials = this.HttpCredentials;
            return request;
        }

        private bool IsHttpOrHttps(Uri address)
        {
            if (!(address.Scheme == Uri.UriSchemeHttp))
            {
                return (address.Scheme == Uri.UriSchemeHttps);
            }
            return true;
        }

        private void ResolveNext(ResolveCallState resolveCallState)
        {
            if (resolveCallState.StackedRetrievers.Count > 0)
            {
                MetadataRetriever retriever = resolveCallState.StackedRetrievers.Pop();
                if (resolveCallState.HasBeenUsed(retriever))
                {
                    this.ResolveNext(resolveCallState);
                }
                else
                {
                    if (resolveCallState.ResolvedMaxResolvedReferences)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxResolvedMaxResolvedReferences")));
                    }
                    resolveCallState.LogUse(retriever);
                    resolveCallState.HandleSection(retriever.Retrieve(resolveCallState.TimeoutHelper));
                    this.ResolveNext(resolveCallState);
                }
            }
        }

        internal static void TraceReceiveReply(string sourceUrl, System.Type metadataType)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Hashtable dictionary = new Hashtable(2);
                dictionary.Add("SourceUrl", sourceUrl);
                dictionary.Add("MetadataType", metadataType.ToString());
                TraceUtility.TraceEvent(TraceEventType.Information, 0x8005c, System.ServiceModel.SR.GetString("TraceCodeMetadataExchangeClientReceiveReply"), new DictionaryTraceRecord(dictionary), null, null);
            }
        }

        internal static void TraceSendRequest(EndpointAddress address)
        {
            TraceSendRequest(0x8005b, System.ServiceModel.SR.GetString("TraceCodeMetadataExchangeClientSendRequest"), address.ToString(), MetadataExchangeClientMode.MetadataExchange.ToString());
        }

        internal static void TraceSendRequest(Uri address)
        {
            TraceSendRequest(0x8005b, System.ServiceModel.SR.GetString("TraceCodeMetadataExchangeClientSendRequest"), address.ToString(), MetadataExchangeClientMode.HttpGet.ToString());
        }

        private static void TraceSendRequest(int traceCode, string traceDescription, string address, string mode)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Hashtable hashtable2 = new Hashtable(2);
                hashtable2.Add("Address", address);
                hashtable2.Add("Mode", mode);
                Hashtable dictionary = hashtable2;
                TraceUtility.TraceEvent(TraceEventType.Information, traceCode, traceDescription, new DictionaryTraceRecord(dictionary), null, null);
            }
        }

        private void Validate(Uri address, MetadataExchangeClientMode mode)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            if (!address.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("address", System.ServiceModel.SR.GetString("SFxCannotGetMetadataFromRelativeAddress", new object[] { address }));
            }
            if ((mode == MetadataExchangeClientMode.HttpGet) && !this.IsHttpOrHttps(address))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("address", System.ServiceModel.SR.GetString("SFxCannotHttpGetMetadataFromAddress", new object[] { address }));
            }
            MetadataExchangeClientModeHelper.Validate(mode);
        }

        public ICredentials HttpCredentials
        {
            get
            {
                return this.webRequestCredentials;
            }
            set
            {
                this.webRequestCredentials = value;
            }
        }

        public int MaximumResolvedReferences
        {
            get
            {
                return this.maximumResolvedReferences;
            }
            set
            {
                if (value < 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("SFxMaximumResolvedReferencesOutOfRange", new object[] { value })));
                }
                this.maximumResolvedReferences = value;
            }
        }

        internal long MaxMessageSize
        {
            get
            {
                return this.maxMessageSize;
            }
            set
            {
                this.maxMessageSize = value;
            }
        }

        public TimeSpan OperationTimeout
        {
            get
            {
                return this.resolveTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.resolveTimeout = value;
            }
        }

        internal XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                if (this.readerQuotas == null)
                {
                    if (this.factory != null)
                    {
                        BindingElementCollection elements = this.factory.Endpoint.Binding.CreateBindingElements();
                        if (elements != null)
                        {
                            MessageEncodingBindingElement element = elements.Find<MessageEncodingBindingElement>();
                            if (element != null)
                            {
                                this.readerQuotas = element.GetIndividualProperty<XmlDictionaryReaderQuotas>();
                            }
                        }
                    }
                    this.readerQuotas = this.readerQuotas ?? EncoderDefaults.ReaderQuotas;
                }
                return this.readerQuotas;
            }
        }

        public bool ResolveMetadataReferences
        {
            get
            {
                return this.resolveMetadataReferences;
            }
            set
            {
                this.resolveMetadataReferences = value;
            }
        }

        public ClientCredentials SoapCredentials
        {
            get
            {
                return this.factory.Credentials;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.factory.Endpoint.Behaviors.RemoveAll<ClientCredentials>();
                this.factory.Endpoint.Behaviors.Add(value);
            }
        }

        internal object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        private class AsyncMetadataResolver : AsyncResult
        {
            private MetadataExchangeClient.ResolveCallState resolveCallState;

            internal AsyncMetadataResolver(MetadataExchangeClient.ResolveCallState resolveCallState, AsyncCallback callerCallback, object callerAsyncState) : base(callerCallback, callerAsyncState)
            {
                if (resolveCallState == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("resolveCallState");
                }
                this.resolveCallState = resolveCallState;
                Exception exception = null;
                bool flag = false;
                try
                {
                    flag = this.ResolveNext();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                    flag = true;
                }
                if (flag)
                {
                    base.Complete(true, exception);
                }
            }

            internal static MetadataSet End(IAsyncResult result)
            {
                return AsyncResult.End<MetadataExchangeClient.AsyncMetadataResolver>(result).resolveCallState.MetadataSet;
            }

            private bool HandleResult(IAsyncResult result)
            {
                MetadataSection section = ((MetadataExchangeClient.MetadataRetriever) result.AsyncState).EndRetrieve(result);
                this.resolveCallState.HandleSection(section);
                return this.ResolveNext();
            }

            private bool ResolveNext()
            {
                bool flag = false;
                if (this.resolveCallState.StackedRetrievers.Count > 0)
                {
                    MetadataExchangeClient.MetadataRetriever retriever = this.resolveCallState.StackedRetrievers.Pop();
                    if (this.resolveCallState.HasBeenUsed(retriever))
                    {
                        return this.ResolveNext();
                    }
                    if (this.resolveCallState.ResolvedMaxResolvedReferences)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxResolvedMaxResolvedReferences")));
                    }
                    this.resolveCallState.LogUse(retriever);
                    IAsyncResult result = retriever.BeginRetrieve(this.resolveCallState.TimeoutHelper, Fx.ThunkCallback(new AsyncCallback(this.RetrieveCallback)), retriever);
                    if (result.CompletedSynchronously)
                    {
                        flag = this.HandleResult(result);
                    }
                    return flag;
                }
                return true;
            }

            internal void RetrieveCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        flag = this.HandleResult(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                        flag = true;
                    }
                    if (flag)
                    {
                        base.Complete(false, exception);
                    }
                }
            }
        }

        internal class EncodingHelper
        {
            internal const string ApplicationBase = "application";

            internal static Encoding GetDictionaryReaderEncoding(string contentTypeStr)
            {
                if (!string.IsNullOrEmpty(contentTypeStr))
                {
                    Encoding rfcEncoding = GetRfcEncoding(contentTypeStr);
                    if (rfcEncoding == null)
                    {
                        return TextEncoderDefaults.Encoding;
                    }
                    string webName = rfcEncoding.WebName;
                    Encoding[] supportedEncodings = TextEncoderDefaults.SupportedEncodings;
                    for (int i = 0; i < supportedEncodings.Length; i++)
                    {
                        if (webName == supportedEncodings[i].WebName)
                        {
                            return rfcEncoding;
                        }
                    }
                }
                return TextEncoderDefaults.Encoding;
            }

            internal static Encoding GetRfcEncoding(string contentTypeStr)
            {
                Encoding encoding = null;
                ContentType contentType = null;
                try
                {
                    contentType = new ContentType(contentTypeStr);
                    string name = (contentType == null) ? string.Empty : contentType.CharSet;
                    if ((name != null) && (name.Length > 0))
                    {
                        encoding = Encoding.GetEncoding(name);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                }
                if (!IsApplication(contentType))
                {
                    return encoding;
                }
                if (encoding != null)
                {
                    return encoding;
                }
                return new ASCIIEncoding();
            }

            internal static bool IsApplication(ContentType contentType)
            {
                return (string.Compare((contentType == null) ? string.Empty : contentType.MediaType, "application", StringComparison.OrdinalIgnoreCase) == 0);
            }
        }

        private class MetadataLocationRetriever : MetadataExchangeClient.MetadataRetriever
        {
            private Uri location;
            private Uri responseLocation;

            internal MetadataLocationRetriever(Uri location, MetadataExchangeClient resolver) : this(location, resolver, null, null)
            {
            }

            internal MetadataLocationRetriever(Uri location, MetadataExchangeClient resolver, string dialect, string identifier) : base(resolver, dialect, identifier)
            {
                ValidateLocation(location);
                this.location = location;
                this.responseLocation = location;
            }

            internal override IAsyncResult BeginRetrieve(TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                AsyncMetadataLocationRetriever retriever;
                try
                {
                    HttpWebRequest request;
                    try
                    {
                        request = base.resolver.GetWebRequest(this.location, base.dialect, base.identifier);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMetadataExchangeClientCouldNotCreateWebRequest", new object[] { this.location, base.dialect, base.identifier }), exception));
                    }
                    MetadataExchangeClient.TraceSendRequest(this.location);
                    retriever = new AsyncMetadataLocationRetriever(request, base.resolver.MaxMessageSize, base.resolver.ReaderQuotas, timeoutHelper, callback, state);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBadMetadataReference", new object[] { this.SourceUrl }), exception2));
                }
                return retriever;
            }

            protected override XmlReader DownloadMetadata(TimeoutHelper timeoutHelper)
            {
                HttpWebRequest request;
                try
                {
                    request = base.resolver.GetWebRequest(this.location, base.dialect, base.identifier);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMetadataExchangeClientCouldNotCreateWebRequest", new object[] { this.location, base.dialect, base.identifier }), exception));
                }
                MetadataExchangeClient.TraceSendRequest(this.location);
                request.Timeout = TimeoutHelper.ToMilliseconds(timeoutHelper.RemainingTime());
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                this.responseLocation = request.Address;
                return GetXmlReader(response, base.resolver.MaxMessageSize, base.resolver.ReaderQuotas);
            }

            internal override MetadataSection EndRetrieve(IAsyncResult result)
            {
                MetadataSection section;
                try
                {
                    section = AsyncMetadataLocationRetriever.End(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBadMetadataReference", new object[] { this.SourceUrl }), exception));
                }
                return section;
            }

            public override bool Equals(object obj)
            {
                return ((obj is MetadataExchangeClient.MetadataLocationRetriever) && (((MetadataExchangeClient.MetadataLocationRetriever) obj).location == this.location));
            }

            public override int GetHashCode()
            {
                return this.location.GetHashCode();
            }

            internal static XmlReader GetXmlReader(HttpWebResponse response, long maxMessageSize, XmlDictionaryReaderQuotas readerQuotas)
            {
                readerQuotas = readerQuotas ?? EncoderDefaults.ReaderQuotas;
                XmlReader reader = XmlDictionaryReader.CreateTextReader(new MaxMessageSizeStream(response.GetResponseStream(), maxMessageSize), MetadataExchangeClient.EncodingHelper.GetDictionaryReaderEncoding(response.ContentType), readerQuotas, null);
                reader.Read();
                reader.MoveToContent();
                return reader;
            }

            internal static void ValidateLocation(Uri location)
            {
                if (location == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("location");
                }
                if ((location.Scheme != Uri.UriSchemeHttp) && (location.Scheme != Uri.UriSchemeHttps))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("location", System.ServiceModel.SR.GetString("SFxCannotGetMetadataFromLocation", new object[] { location.ToString() }));
                }
            }

            protected override string SourceUrl
            {
                get
                {
                    return this.responseLocation.ToString();
                }
            }

            private class AsyncMetadataLocationRetriever : AsyncResult
            {
                private long maxMessageSize;
                private XmlDictionaryReaderQuotas readerQuotas;
                private MetadataSection section;

                internal AsyncMetadataLocationRetriever(WebRequest request, long maxMessageSize, XmlDictionaryReaderQuotas readerQuotas, TimeoutHelper timeoutHelper, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.maxMessageSize = maxMessageSize;
                    this.readerQuotas = readerQuotas;
                    IAsyncResult result = request.BeginGetResponse(Fx.ThunkCallback(new AsyncCallback(this.GetResponseCallback)), request);
                    ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, Fx.ThunkCallback(new WaitOrTimerCallback(MetadataExchangeClient.MetadataLocationRetriever.AsyncMetadataLocationRetriever.RetrieveTimeout)), request, TimeoutHelper.ToMilliseconds(timeoutHelper.RemainingTime()), true);
                    if (result.CompletedSynchronously)
                    {
                        this.HandleResult(result);
                        base.Complete(true);
                    }
                }

                internal static MetadataSection End(IAsyncResult result)
                {
                    return AsyncResult.End<MetadataExchangeClient.MetadataLocationRetriever.AsyncMetadataLocationRetriever>(result).section;
                }

                internal void GetResponseCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        Exception exception = null;
                        try
                        {
                            this.HandleResult(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        base.Complete(false, exception);
                    }
                }

                private void HandleResult(IAsyncResult result)
                {
                    HttpWebRequest asyncState = (HttpWebRequest) result.AsyncState;
                    using (XmlReader reader = MetadataExchangeClient.MetadataLocationRetriever.GetXmlReader((HttpWebResponse) asyncState.EndGetResponse(result), this.maxMessageSize, this.readerQuotas))
                    {
                        this.section = MetadataExchangeClient.MetadataRetriever.CreateMetadataSection(reader, asyncState.Address.ToString());
                    }
                }

                private static void RetrieveTimeout(object state, bool timedOut)
                {
                    if (timedOut)
                    {
                        HttpWebRequest request = state as HttpWebRequest;
                        if (request != null)
                        {
                            request.Abort();
                        }
                    }
                }
            }
        }

        private class MetadataReferenceRetriever : MetadataExchangeClient.MetadataRetriever
        {
            private EndpointAddress address;
            private Uri via;

            public MetadataReferenceRetriever(EndpointAddress address, MetadataExchangeClient resolver) : this(address, null, resolver, null, null)
            {
            }

            public MetadataReferenceRetriever(EndpointAddress address, Uri via, MetadataExchangeClient resolver) : this(address, via, resolver, null, null)
            {
            }

            public MetadataReferenceRetriever(EndpointAddress address, MetadataExchangeClient resolver, string dialect, string identifier) : this(address, null, resolver, dialect, identifier)
            {
            }

            private MetadataReferenceRetriever(EndpointAddress address, Uri via, MetadataExchangeClient resolver, string dialect, string identifier) : base(resolver, dialect, identifier)
            {
                if (address == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
                }
                this.address = address;
                this.via = via;
            }

            internal override IAsyncResult BeginRetrieve(TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                IAsyncResult result;
                try
                {
                    IMetadataExchange exchange;
                    MessageVersion messageVersion;
                    lock (base.resolver.ThisLock)
                    {
                        ChannelFactory<IMetadataExchange> factory;
                        try
                        {
                            factory = base.resolver.GetChannelFactory(this.address, base.dialect, base.identifier);
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMetadataExchangeClientCouldNotCreateChannelFactory", new object[] { this.address, base.dialect, base.identifier }), exception));
                        }
                        exchange = this.CreateChannel(factory);
                        messageVersion = factory.Endpoint.Binding.MessageVersion;
                    }
                    MetadataExchangeClient.TraceSendRequest(this.address);
                    result = new AsyncMetadataReferenceRetriever(exchange, messageVersion, timeoutHelper, callback, state);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBadMetadataReference", new object[] { this.SourceUrl }), exception2));
                }
                return result;
            }

            private IMetadataExchange CreateChannel(ChannelFactory<IMetadataExchange> channelFactory)
            {
                if (this.via != null)
                {
                    return channelFactory.CreateChannel(this.address, this.via);
                }
                return channelFactory.CreateChannel(this.address);
            }

            private static System.ServiceModel.Channels.Message CreateGetMessage(MessageVersion messageVersion)
            {
                return System.ServiceModel.Channels.Message.CreateMessage(messageVersion, "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get");
            }

            protected override XmlReader DownloadMetadata(TimeoutHelper timeoutHelper)
            {
                IMetadataExchange exchange;
                MessageVersion messageVersion;
                System.ServiceModel.Channels.Message message;
                lock (base.resolver.ThisLock)
                {
                    ChannelFactory<IMetadataExchange> factory;
                    try
                    {
                        factory = base.resolver.GetChannelFactory(this.address, base.dialect, base.identifier);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMetadataExchangeClientCouldNotCreateChannelFactory", new object[] { this.address, base.dialect, base.identifier }), exception));
                    }
                    exchange = this.CreateChannel(factory);
                    messageVersion = factory.Endpoint.Binding.MessageVersion;
                }
                MetadataExchangeClient.TraceSendRequest(this.address);
                try
                {
                    using (System.ServiceModel.Channels.Message message2 = CreateGetMessage(messageVersion))
                    {
                        ((IClientChannel) exchange).OperationTimeout = timeoutHelper.RemainingTime();
                        message = exchange.Get(message2);
                    }
                    ((IClientChannel) exchange).Close();
                }
                finally
                {
                    ((IClientChannel) exchange).Abort();
                }
                if (message.IsFault)
                {
                    MessageFault fault = MessageFault.CreateFault(message, 0x10000);
                    StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
                    XmlWriter writer = XmlWriter.Create(output);
                    fault.WriteTo(writer, message.Version.Envelope);
                    writer.Flush();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(output.ToString()));
                }
                return message.GetReaderAtBodyContents();
            }

            internal override MetadataSection EndRetrieve(IAsyncResult result)
            {
                MetadataSection section;
                try
                {
                    section = AsyncMetadataReferenceRetriever.End(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBadMetadataReference", new object[] { this.SourceUrl }), exception));
                }
                return section;
            }

            public override bool Equals(object obj)
            {
                return ((obj is MetadataExchangeClient.MetadataReferenceRetriever) && (((MetadataExchangeClient.MetadataReferenceRetriever) obj).address == this.address));
            }

            public override int GetHashCode()
            {
                return this.address.GetHashCode();
            }

            protected override string SourceUrl
            {
                get
                {
                    return this.address.Uri.ToString();
                }
            }

            private class AsyncMetadataReferenceRetriever : AsyncResult
            {
                private System.ServiceModel.Channels.Message message;
                private MetadataSection section;

                internal AsyncMetadataReferenceRetriever(IMetadataExchange metadataClient, MessageVersion messageVersion, TimeoutHelper timeoutHelper, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.message = MetadataExchangeClient.MetadataReferenceRetriever.CreateGetMessage(messageVersion);
                    ((IClientChannel) metadataClient).OperationTimeout = timeoutHelper.RemainingTime();
                    IAsyncResult result = metadataClient.BeginGet(this.message, Fx.ThunkCallback(new AsyncCallback(this.RequestCallback)), metadataClient);
                    if (result.CompletedSynchronously)
                    {
                        this.HandleResult(result);
                        base.Complete(true);
                    }
                }

                internal static MetadataSection End(IAsyncResult result)
                {
                    return AsyncResult.End<MetadataExchangeClient.MetadataReferenceRetriever.AsyncMetadataReferenceRetriever>(result).section;
                }

                private void HandleResult(IAsyncResult result)
                {
                    IMetadataExchange asyncState = (IMetadataExchange) result.AsyncState;
                    System.ServiceModel.Channels.Message message = asyncState.EndGet(result);
                    using (this.message)
                    {
                        if (message.IsFault)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBadMetadataReference", new object[] { ((IClientChannel) asyncState).RemoteAddress.Uri.ToString() })));
                        }
                        using (XmlReader reader = message.GetReaderAtBodyContents())
                        {
                            this.section = MetadataExchangeClient.MetadataRetriever.CreateMetadataSection(reader, ((IClientChannel) asyncState).RemoteAddress.Uri.ToString());
                        }
                    }
                }

                internal void RequestCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        Exception exception = null;
                        try
                        {
                            this.HandleResult(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        base.Complete(false, exception);
                    }
                }
            }
        }

        private abstract class MetadataRetriever
        {
            protected string dialect;
            protected string identifier;
            protected MetadataExchangeClient resolver;

            public MetadataRetriever(MetadataExchangeClient resolver, string dialect, string identifier)
            {
                this.resolver = resolver;
                this.dialect = dialect;
                this.identifier = identifier;
            }

            internal abstract IAsyncResult BeginRetrieve(TimeoutHelper timeoutHelper, AsyncCallback callback, object state);
            private static bool CanReadMetadataSet(XmlReader reader)
            {
                return ((reader.LocalName == "Metadata") && (reader.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/mex"));
            }

            private static bool CanReadSchema(XmlReader reader)
            {
                return ((reader.LocalName == "schema") && (reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema"));
            }

            internal static MetadataSection CreateMetadataSection(XmlReader reader, string sourceUrl)
            {
                MetadataSection section = null;
                System.Type metadataType = null;
                if (CanReadMetadataSet(reader))
                {
                    MetadataSet metadata = MetadataSet.ReadFrom(reader);
                    section = new MetadataSection(MetadataSection.MetadataExchangeDialect, null, metadata);
                    metadataType = typeof(MetadataSet);
                }
                else if (System.Web.Services.Description.ServiceDescription.CanRead(reader))
                {
                    section = MetadataSection.CreateFromServiceDescription(System.Web.Services.Description.ServiceDescription.Read(reader));
                    metadataType = typeof(System.Web.Services.Description.ServiceDescription);
                }
                else if (CanReadSchema(reader))
                {
                    section = MetadataSection.CreateFromSchema(System.Xml.Schema.XmlSchema.Read(reader, null));
                    metadataType = typeof(System.Xml.Schema.XmlSchema);
                }
                else
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(reader);
                    section = new MetadataSection(null, null, document.DocumentElement);
                    metadataType = typeof(XmlElement);
                }
                section.SourceUrl = sourceUrl;
                MetadataExchangeClient.TraceReceiveReply(sourceUrl, metadataType);
                return section;
            }

            protected abstract XmlReader DownloadMetadata(TimeoutHelper timeoutHelper);
            internal abstract MetadataSection EndRetrieve(IAsyncResult result);
            internal MetadataSection Retrieve(TimeoutHelper timeoutHelper)
            {
                MetadataSection section;
                try
                {
                    using (XmlReader reader = this.DownloadMetadata(timeoutHelper))
                    {
                        section = CreateMetadataSection(reader, this.SourceUrl);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBadMetadataReference", new object[] { this.SourceUrl }), exception));
                }
                return section;
            }

            protected abstract string SourceUrl { get; }
        }

        private class ResolveCallState
        {
            private int maxResolvedReferences;
            private System.ServiceModel.Description.MetadataSet metadataSet;
            private bool resolveMetadataReferences;
            private MetadataExchangeClient resolver;
            private Stack<MetadataExchangeClient.MetadataRetriever> stackedRetrievers;
            private System.Runtime.TimeoutHelper timeoutHelper;
            private Dictionary<MetadataExchangeClient.MetadataRetriever, MetadataExchangeClient.MetadataRetriever> usedRetrievers;

            internal ResolveCallState(int maxResolvedReferences, bool resolveMetadataReferences, System.Runtime.TimeoutHelper timeoutHelper, MetadataExchangeClient resolver)
            {
                this.maxResolvedReferences = maxResolvedReferences;
                this.resolveMetadataReferences = resolveMetadataReferences;
                this.resolver = resolver;
                this.timeoutHelper = timeoutHelper;
                this.metadataSet = new System.ServiceModel.Description.MetadataSet();
                this.usedRetrievers = new Dictionary<MetadataExchangeClient.MetadataRetriever, MetadataExchangeClient.MetadataRetriever>();
                this.stackedRetrievers = new Stack<MetadataExchangeClient.MetadataRetriever>();
            }

            private Uri CreateUri(string baseUri, string relativeUri)
            {
                return new Uri(new Uri(baseUri), relativeUri);
            }

            private void EnqueueRetrieverIfShouldResolve(MetadataExchangeClient.MetadataRetriever retriever)
            {
                if (this.resolveMetadataReferences)
                {
                    this.stackedRetrievers.Push(retriever);
                }
            }

            private void HandleSchemaImports(MetadataSection section)
            {
                System.Xml.Schema.XmlSchema metadata = (System.Xml.Schema.XmlSchema) section.Metadata;
                foreach (XmlSchemaExternal external in metadata.Includes)
                {
                    if (!string.IsNullOrEmpty(external.SchemaLocation))
                    {
                        this.EnqueueRetrieverIfShouldResolve(new MetadataExchangeClient.MetadataLocationRetriever(this.CreateUri(section.SourceUrl, external.SchemaLocation), this.resolver));
                    }
                }
            }

            internal void HandleSection(MetadataSection section)
            {
                if (section.Metadata is System.ServiceModel.Description.MetadataSet)
                {
                    foreach (MetadataSection section2 in ((System.ServiceModel.Description.MetadataSet) section.Metadata).MetadataSections)
                    {
                        section2.SourceUrl = section.SourceUrl;
                        this.HandleSection(section2);
                    }
                }
                else if (section.Metadata is MetadataReference)
                {
                    if (this.resolveMetadataReferences)
                    {
                        MetadataExchangeClient.MetadataRetriever item = new MetadataExchangeClient.MetadataReferenceRetriever(((MetadataReference) section.Metadata).Address, this.resolver, section.Dialect, section.Identifier);
                        this.stackedRetrievers.Push(item);
                    }
                    else
                    {
                        this.metadataSet.MetadataSections.Add(section);
                    }
                }
                else if (section.Metadata is MetadataLocation)
                {
                    if (this.resolveMetadataReferences)
                    {
                        string location = ((MetadataLocation) section.Metadata).Location;
                        MetadataExchangeClient.MetadataRetriever retriever2 = new MetadataExchangeClient.MetadataLocationRetriever(this.CreateUri(section.SourceUrl, location), this.resolver, section.Dialect, section.Identifier);
                        this.stackedRetrievers.Push(retriever2);
                    }
                    else
                    {
                        this.metadataSet.MetadataSections.Add(section);
                    }
                }
                else if (section.Metadata is System.Web.Services.Description.ServiceDescription)
                {
                    if (this.resolveMetadataReferences)
                    {
                        this.HandleWsdlImports(section);
                    }
                    this.metadataSet.MetadataSections.Add(section);
                }
                else if (section.Metadata is System.Xml.Schema.XmlSchema)
                {
                    if (this.resolveMetadataReferences)
                    {
                        this.HandleSchemaImports(section);
                    }
                    this.metadataSet.MetadataSections.Add(section);
                }
                else
                {
                    this.metadataSet.MetadataSections.Add(section);
                }
            }

            private void HandleWsdlImports(MetadataSection section)
            {
                System.Web.Services.Description.ServiceDescription metadata = (System.Web.Services.Description.ServiceDescription) section.Metadata;
                foreach (Import import in metadata.Imports)
                {
                    if (!string.IsNullOrEmpty(import.Location))
                    {
                        this.EnqueueRetrieverIfShouldResolve(new MetadataExchangeClient.MetadataLocationRetriever(this.CreateUri(section.SourceUrl, import.Location), this.resolver));
                    }
                }
                foreach (System.Xml.Schema.XmlSchema schema in metadata.Types.Schemas)
                {
                    MetadataSection section2 = new MetadataSection(null, null, schema) {
                        SourceUrl = section.SourceUrl
                    };
                    this.HandleSchemaImports(section2);
                }
            }

            internal bool HasBeenUsed(MetadataExchangeClient.MetadataRetriever retriever)
            {
                return this.usedRetrievers.ContainsKey(retriever);
            }

            internal void LogUse(MetadataExchangeClient.MetadataRetriever retriever)
            {
                this.usedRetrievers.Add(retriever, retriever);
            }

            internal System.ServiceModel.Description.MetadataSet MetadataSet
            {
                get
                {
                    return this.metadataSet;
                }
            }

            internal bool ResolvedMaxResolvedReferences
            {
                get
                {
                    return (this.usedRetrievers.Count == this.maxResolvedReferences);
                }
            }

            internal Stack<MetadataExchangeClient.MetadataRetriever> StackedRetrievers
            {
                get
                {
                    return this.stackedRetrievers;
                }
            }

            internal System.Runtime.TimeoutHelper TimeoutHelper
            {
                get
                {
                    return this.timeoutHelper;
                }
            }
        }
    }
}

