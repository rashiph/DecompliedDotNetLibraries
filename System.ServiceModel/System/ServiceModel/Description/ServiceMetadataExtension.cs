namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;

    public class ServiceMetadataExtension : IExtension<ServiceHostBase>
    {
        private const string BaseAddressPattern = "{%BaseAddress%}";
        private static readonly Uri EmptyUri = new Uri(string.Empty, UriKind.Relative);
        private Uri externalMetadataLocation;
        private System.ServiceModel.Channels.Binding httpGetBinding;
        private bool httpGetEnabled;
        private static readonly System.Type[] httpGetSupportedChannels = new System.Type[] { typeof(IReplyChannel) };
        private Uri httpGetUrl;
        private System.ServiceModel.Channels.Binding httpHelpPageBinding;
        private bool httpHelpPageEnabled;
        private Uri httpHelpPageUrl;
        private System.ServiceModel.Channels.Binding httpsGetBinding;
        private bool httpsGetEnabled;
        private Uri httpsGetUrl;
        private System.ServiceModel.Channels.Binding httpsHelpPageBinding;
        private bool httpsHelpPageEnabled;
        private Uri httpsHelpPageUrl;
        private ServiceMetadataBehavior.MetadataExtensionInitializer initializer;
        private bool isInitialized;
        private MetadataSet metadata;
        private bool mexEnabled;
        private Uri mexUrl;
        private ServiceHostBase owner;
        private object syncRoot;

        public ServiceMetadataExtension() : this(null)
        {
        }

        internal ServiceMetadataExtension(ServiceMetadataBehavior.MetadataExtensionInitializer initializer)
        {
            this.syncRoot = new object();
            this.initializer = initializer;
        }

        private ChannelDispatcher CreateGetDispatcher(Uri listenUri)
        {
            if (listenUri.Scheme == Uri.UriSchemeHttp)
            {
                return this.CreateGetDispatcher(listenUri, MetadataExchangeBindings.HttpGet);
            }
            if (listenUri.Scheme != Uri.UriSchemeHttps)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxGetChannelDispatcherDoesNotSupportScheme", new object[] { typeof(ChannelDispatcher).Name, Uri.UriSchemeHttp, Uri.UriSchemeHttps })));
            }
            return this.CreateGetDispatcher(listenUri, MetadataExchangeBindings.HttpsGet);
        }

        private ChannelDispatcher CreateGetDispatcher(Uri listenUri, System.ServiceModel.Channels.Binding binding)
        {
            EndpointAddress address = new EndpointAddress(listenUri, new AddressHeader[0]);
            Uri listenUriBaseAddress = listenUri;
            string listenUriRelativeAddress = string.Empty;
            BindingParameterCollection bindingParameters = GetBindingParameters(this.owner.Description);
            IChannelListener listener = null;
            if (!binding.CanBuildChannelListener<IReplyChannel>(bindingParameters))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxBindingNotSupportedForMetadataHttpGet")));
            }
            listener = binding.BuildChannelListener<IReplyChannel>(listenUriBaseAddress, listenUriRelativeAddress, bindingParameters);
            ChannelDispatcher dispatcher = new ChannelDispatcher(listener, "ServiceMetadataBehaviorHttpGetBinding", binding) {
                MessageVersion = binding.MessageVersion
            };
            EndpointDispatcher item = new EndpointDispatcher(address, "IHttpGetHelpPageAndMetadataContract", "http://schemas.microsoft.com/2006/04/http/metadata", true);
            DispatchOperation operation = new DispatchOperation(item.DispatchRuntime, "Get", "*", "*") {
                Formatter = MessageOperationFormatter.Instance
            };
            MethodInfo method = typeof(IHttpGetMetadata).GetMethod("Get");
            operation.Invoker = new SyncMethodInvoker(method);
            item.DispatchRuntime.Operations.Add(operation);
            HttpGetImpl implementation = new HttpGetImpl(this, listener.Uri);
            item.DispatchRuntime.SingletonInstanceContext = new InstanceContext(this.owner, implementation, false);
            item.DispatchRuntime.MessageInspectors.Add(implementation);
            dispatcher.Endpoints.Add(item);
            item.ContractFilter = new MatchAllMessageFilter();
            item.FilterPriority = 0;
            item.DispatchRuntime.InstanceContextProvider = InstanceContextProviderBase.GetProviderForMode(InstanceContextMode.Single, item.DispatchRuntime);
            dispatcher.ServiceThrottle = this.owner.ServiceThrottle;
            ServiceDebugBehavior behavior = this.owner.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (behavior != null)
            {
                dispatcher.IncludeExceptionDetailInFaults |= behavior.IncludeExceptionDetailInFaults;
            }
            ServiceBehaviorAttribute attribute = this.owner.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            if (attribute != null)
            {
                dispatcher.IncludeExceptionDetailInFaults |= attribute.IncludeExceptionDetailInFaults;
            }
            return dispatcher;
        }

        internal ChannelDispatcher EnsureGetDispatcher(Uri listenUri)
        {
            ChannelDispatcher item = this.FindGetDispatcher(listenUri);
            if (item == null)
            {
                item = this.CreateGetDispatcher(listenUri);
                this.owner.ChannelDispatchers.Add(item);
            }
            return item;
        }

        internal ChannelDispatcher EnsureGetDispatcher(Uri listenUri, bool isServiceDebugBehavior)
        {
            ChannelDispatcher item = this.FindGetDispatcher(listenUri);
            if (item == null)
            {
                System.ServiceModel.Channels.Binding binding;
                if (listenUri.Scheme == Uri.UriSchemeHttp)
                {
                    if (isServiceDebugBehavior)
                    {
                        binding = this.httpHelpPageBinding ?? MetadataExchangeBindings.HttpGet;
                    }
                    else
                    {
                        binding = this.httpGetBinding ?? MetadataExchangeBindings.HttpGet;
                    }
                }
                else
                {
                    if (listenUri.Scheme != Uri.UriSchemeHttps)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxGetChannelDispatcherDoesNotSupportScheme", new object[] { typeof(ChannelDispatcher).Name, Uri.UriSchemeHttp, Uri.UriSchemeHttps })));
                    }
                    if (isServiceDebugBehavior)
                    {
                        binding = this.httpsHelpPageBinding ?? MetadataExchangeBindings.HttpsGet;
                    }
                    else
                    {
                        binding = this.httpsGetBinding ?? MetadataExchangeBindings.HttpsGet;
                    }
                }
                item = this.CreateGetDispatcher(listenUri, binding);
                this.owner.ChannelDispatchers.Add(item);
            }
            return item;
        }

        private void EnsureInitialized()
        {
            if (!this.isInitialized)
            {
                lock (this.syncRoot)
                {
                    if (!this.isInitialized)
                    {
                        if (this.initializer != null)
                        {
                            this.metadata = this.initializer.GenerateMetadata();
                        }
                        if (this.metadata == null)
                        {
                            this.metadata = new MetadataSet();
                        }
                        Thread.MemoryBarrier();
                        this.isInitialized = true;
                        this.initializer = null;
                    }
                }
            }
        }

        internal static ServiceMetadataExtension EnsureServiceMetadataExtension(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase host)
        {
            ServiceMetadataExtension item = host.Extensions.Find<ServiceMetadataExtension>();
            if (item == null)
            {
                item = new ServiceMetadataExtension();
                host.Extensions.Add(item);
            }
            return item;
        }

        private ChannelDispatcher FindGetDispatcher(Uri listenUri)
        {
            foreach (ChannelDispatcherBase base2 in this.owner.ChannelDispatchers)
            {
                ChannelDispatcher dispatcher = base2 as ChannelDispatcher;
                if ((((dispatcher != null) && (dispatcher.Listener.Uri == listenUri)) && ((dispatcher.Endpoints.Count == 1) && (dispatcher.Endpoints[0].DispatchRuntime.SingletonInstanceContext != null))) && (dispatcher.Endpoints[0].DispatchRuntime.SingletonInstanceContext.UserObject is HttpGetImpl))
                {
                    return dispatcher;
                }
            }
            return null;
        }

        private static BindingParameterCollection GetBindingParameters(System.ServiceModel.Description.ServiceDescription description)
        {
            BindingParameterCollection bindingParameters = new BindingParameterCollection();
            foreach (IServiceBehavior behavior in description.Behaviors)
            {
                if ((behavior is ServiceCredentials) || (behavior is ServiceSecurityAuditBehavior))
                {
                    bindingParameters.Add(behavior);
                }
                else
                {
                    AspNetEnvironment.Current.ProcessBehaviorForMetadataExtension(behavior, bindingParameters);
                }
            }
            return bindingParameters;
        }

        private DynamicAddressUpdateWriter GetDynamicAddressWriter(System.ServiceModel.Channels.Message request, Uri listenUri, bool removeBaseAddress)
        {
            string host;
            int port;
            if (!TryGetHttpHostAndPort(listenUri, request, out host, out port))
            {
                if (request.Headers.To == null)
                {
                    return null;
                }
                host = request.Headers.To.Host;
                port = request.Headers.To.Port;
            }
            if ((!(host == listenUri.Host) || (port != listenUri.Port)) || ((this.UpdatePortsByScheme != null) && (this.UpdatePortsByScheme.Count != 0)))
            {
                return new DynamicAddressUpdateWriter(listenUri, host, port, this.UpdatePortsByScheme, removeBaseAddress);
            }
            return null;
        }

        private WriteFilter GetWriteFilter(System.ServiceModel.Channels.Message request, Uri listenUri, bool removeBaseAddress)
        {
            WriteFilter filter = null;
            if (this.UpdateAddressDynamically)
            {
                filter = this.GetDynamicAddressWriter(request, listenUri, removeBaseAddress);
            }
            if (filter != null)
            {
                return filter;
            }
            if (removeBaseAddress)
            {
                return new LocationUpdatingWriter("{%BaseAddress%}", null);
            }
            return new LocationUpdatingWriter("{%BaseAddress%}", listenUri.ToString());
        }

        void IExtension<ServiceHostBase>.Attach(ServiceHostBase owner)
        {
            if (owner == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("owner"));
            }
            if (this.owner != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TheServiceMetadataExtensionInstanceCouldNot2_0")));
            }
            owner.ThrowIfClosedOrOpened();
            this.owner = owner;
        }

        void IExtension<ServiceHostBase>.Detach(ServiceHostBase owner)
        {
            if (owner == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("owner");
            }
            if (this.owner == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TheServiceMetadataExtensionInstanceCouldNot3_0")));
            }
            if (this.owner != owner)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("owner", System.ServiceModel.SR.GetString("TheServiceMetadataExtensionInstanceCouldNot4_0"));
            }
            this.owner.ThrowIfClosedOrOpened();
            this.owner = null;
        }

        internal static bool TryGetHttpHostAndPort(Uri listenUri, System.ServiceModel.Channels.Message request, out string host, out int port)
        {
            object obj2;
            Uri uri;
            host = null;
            port = 0;
            if (!request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out obj2))
            {
                return false;
            }
            HttpRequestMessageProperty property = obj2 as HttpRequestMessageProperty;
            if (property == null)
            {
                return false;
            }
            string str = property.Headers[HttpRequestHeader.Host];
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            if (!Uri.TryCreate(listenUri.Scheme + "://" + str, UriKind.Absolute, out uri))
            {
                return false;
            }
            host = uri.Host;
            port = uri.Port;
            return true;
        }

        internal Uri ExternalMetadataLocation
        {
            get
            {
                return this.externalMetadataLocation;
            }
            set
            {
                this.externalMetadataLocation = value;
            }
        }

        internal bool HelpPageEnabled
        {
            get
            {
                if (!this.httpHelpPageEnabled)
                {
                    return this.httpsHelpPageEnabled;
                }
                return true;
            }
        }

        internal System.ServiceModel.Channels.Binding HttpGetBinding
        {
            get
            {
                return this.httpGetBinding;
            }
            set
            {
                this.httpGetBinding = value;
            }
        }

        internal bool HttpGetEnabled
        {
            get
            {
                return this.httpGetEnabled;
            }
            set
            {
                this.httpGetEnabled = value;
            }
        }

        internal Uri HttpGetUrl
        {
            get
            {
                return this.httpGetUrl;
            }
            set
            {
                this.httpGetUrl = value;
            }
        }

        internal System.ServiceModel.Channels.Binding HttpHelpPageBinding
        {
            get
            {
                return this.httpHelpPageBinding;
            }
            set
            {
                this.httpHelpPageBinding = value;
            }
        }

        internal bool HttpHelpPageEnabled
        {
            get
            {
                return this.httpHelpPageEnabled;
            }
            set
            {
                this.httpHelpPageEnabled = value;
            }
        }

        internal Uri HttpHelpPageUrl
        {
            get
            {
                return this.httpHelpPageUrl;
            }
            set
            {
                this.httpHelpPageUrl = value;
            }
        }

        internal System.ServiceModel.Channels.Binding HttpsGetBinding
        {
            get
            {
                return this.httpsGetBinding;
            }
            set
            {
                this.httpsGetBinding = value;
            }
        }

        internal bool HttpsGetEnabled
        {
            get
            {
                return this.httpsGetEnabled;
            }
            set
            {
                this.httpsGetEnabled = value;
            }
        }

        internal Uri HttpsGetUrl
        {
            get
            {
                return this.httpsGetUrl;
            }
            set
            {
                this.httpsGetUrl = value;
            }
        }

        internal System.ServiceModel.Channels.Binding HttpsHelpPageBinding
        {
            get
            {
                return this.httpsHelpPageBinding;
            }
            set
            {
                this.httpsHelpPageBinding = value;
            }
        }

        internal bool HttpsHelpPageEnabled
        {
            get
            {
                return this.httpsHelpPageEnabled;
            }
            set
            {
                this.httpsHelpPageEnabled = value;
            }
        }

        internal Uri HttpsHelpPageUrl
        {
            get
            {
                return this.httpsHelpPageUrl;
            }
            set
            {
                this.httpsHelpPageUrl = value;
            }
        }

        internal ServiceMetadataBehavior.MetadataExtensionInitializer Initializer
        {
            get
            {
                return this.initializer;
            }
            set
            {
                this.initializer = value;
            }
        }

        public MetadataSet Metadata
        {
            get
            {
                this.EnsureInitialized();
                return this.metadata;
            }
        }

        internal bool MetadataEnabled
        {
            get
            {
                if (!this.mexEnabled && !this.httpGetEnabled)
                {
                    return this.httpsGetEnabled;
                }
                return true;
            }
        }

        internal bool MexEnabled
        {
            get
            {
                return this.mexEnabled;
            }
            set
            {
                this.mexEnabled = value;
            }
        }

        internal Uri MexUrl
        {
            get
            {
                return this.mexUrl;
            }
            set
            {
                this.mexUrl = value;
            }
        }

        internal bool UpdateAddressDynamically { get; set; }

        internal IDictionary<string, int> UpdatePortsByScheme { get; set; }

        private class DynamicAddressUpdateWriter : ServiceMetadataExtension.WriteFilter
        {
            private readonly string newBaseAddress;
            private readonly string newHostName;
            private readonly string oldHostName;
            private readonly bool removeBaseAddress;
            private readonly int requestPort;
            private readonly string requestScheme;
            private readonly IDictionary<string, int> updatePortsByScheme;

            internal DynamicAddressUpdateWriter(Uri listenUri, string requestHost, int requestPort, IDictionary<string, int> updatePortsByScheme, bool removeBaseAddress) : this(listenUri.Host, requestHost, removeBaseAddress, listenUri.Scheme, requestPort, updatePortsByScheme)
            {
                this.newBaseAddress = this.UpdateUri(listenUri).ToString();
            }

            private DynamicAddressUpdateWriter(string oldHostName, string newHostName, bool removeBaseAddress, string requestScheme, int requestPort, IDictionary<string, int> updatePortsByScheme)
            {
                this.oldHostName = oldHostName;
                this.newHostName = newHostName;
                this.removeBaseAddress = removeBaseAddress;
                this.requestScheme = requestScheme;
                this.requestPort = requestPort;
                this.updatePortsByScheme = updatePortsByScheme;
            }

            private DynamicAddressUpdateWriter(string oldHostName, string newHostName, string newBaseAddress, bool removeBaseAddress, string requestScheme, int requestPort, IDictionary<string, int> updatePortsByScheme) : this(oldHostName, newHostName, removeBaseAddress, requestScheme, requestPort, updatePortsByScheme)
            {
                this.newBaseAddress = newBaseAddress;
            }

            public override ServiceMetadataExtension.WriteFilter CloneWriteFilter()
            {
                return new ServiceMetadataExtension.DynamicAddressUpdateWriter(this.oldHostName, this.newHostName, this.newBaseAddress, this.removeBaseAddress, this.requestScheme, this.requestPort, this.updatePortsByScheme);
            }

            private Uri UpdateUri(Uri uri)
            {
                int requestPort;
                if (uri.Host != this.oldHostName)
                {
                    return null;
                }
                if (uri.Scheme == this.requestScheme)
                {
                    requestPort = this.requestPort;
                }
                else if (!this.updatePortsByScheme.TryGetValue(uri.Scheme, out requestPort))
                {
                    return null;
                }
                UriBuilder builder = new UriBuilder(uri) {
                    Host = this.newHostName,
                    Port = requestPort
                };
                return builder.Uri;
            }

            public void UpdateUri(ref Uri uri)
            {
                Uri uri2 = this.UpdateUri((Uri) uri);
                if (uri2 != null)
                {
                    uri = uri2;
                }
            }

            public override void WriteString(string text)
            {
                if (this.removeBaseAddress && text.StartsWith("{%BaseAddress%}", StringComparison.Ordinal))
                {
                    text = string.Empty;
                }
                else if (!this.removeBaseAddress && text.Contains("{%BaseAddress%}"))
                {
                    text = text.Replace("{%BaseAddress%}", this.newBaseAddress);
                }
                else
                {
                    Uri uri;
                    if (Uri.TryCreate(text, UriKind.Absolute, out uri))
                    {
                        Uri uri2 = this.UpdateUri(uri);
                        if (uri2 != null)
                        {
                            text = uri2.ToString();
                        }
                    }
                }
                base.WriteString(text);
            }
        }

        internal class HttpGetImpl : ServiceMetadataExtension.IHttpGetMetadata, IDispatchMessageInspector
        {
            private const int closeTimeoutInSeconds = 90;
            internal const string ContractName = "IHttpGetHelpPageAndMetadataContract";
            internal const string ContractNamespace = "http://schemas.microsoft.com/2006/04/http/metadata";
            private const string DiscoQueryString = "disco";
            private const string DiscoToken = "disco token";
            internal const string GetMethodName = "Get";
            private bool getWsdlEnabled;
            private bool helpPageEnabled;
            internal const string HtmlBreak = "<BR/>";
            private const string HtmlContentType = "text/html; charset=UTF-8";
            private InitializationData initData;
            private Uri listenUri;
            private const int maxQueryStringChars = 0x800;
            internal const string MetadataHttpGetBinding = "ServiceMetadataBehaviorHttpGetBinding";
            private static string[] NoQueries = new string[0];
            private ServiceMetadataExtension parent;
            internal const string ReplyAction = "*";
            internal const string RequestAction = "*";
            private object sync = new object();
            private const string WsdlQueryString = "wsdl";
            private const string XmlContentType = "text/xml; charset=UTF-8";
            private const string XsdQueryString = "xsd";

            internal HttpGetImpl(ServiceMetadataExtension parent, Uri listenUri)
            {
                this.parent = parent;
                this.listenUri = listenUri;
            }

            private static void AddHttpProperty(System.ServiceModel.Channels.Message message, HttpStatusCode status, string contentType)
            {
                HttpResponseMessageProperty property = new HttpResponseMessageProperty {
                    StatusCode = status
                };
                property.Headers.Add(HttpResponseHeader.ContentType, contentType);
                message.Properties.Add(HttpResponseMessageProperty.Name, property);
            }

            public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel, InstanceContext instanceContext)
            {
                return request.Version;
            }

            public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
            {
                if ((reply != null) && reply.IsFault)
                {
                    string errorMessage = System.ServiceModel.SR.GetString("SFxInternalServerError");
                    ExceptionDetail exceptionDetail = null;
                    MessageFault fault = MessageFault.CreateFault(reply, 0x10000);
                    if (fault.HasDetail)
                    {
                        exceptionDetail = fault.GetDetail<ExceptionDetail>();
                        if (exceptionDetail != null)
                        {
                            errorMessage = System.ServiceModel.SR.GetString("SFxDocExt_Error");
                        }
                    }
                    reply = new MetadataOnHelpPageMessage(errorMessage, exceptionDetail);
                    AddHttpProperty(reply, HttpStatusCode.InternalServerError, "text/html; charset=UTF-8");
                }
            }

            private System.ServiceModel.Channels.Message CreateDiscoMessage(ServiceMetadataExtension.DynamicAddressUpdateWriter addressUpdater)
            {
                Uri listenUri = this.listenUri;
                if (addressUpdater != null)
                {
                    addressUpdater.UpdateUri(ref listenUri);
                }
                string wsdlAddress = listenUri.ToString() + "?wsdl";
                Uri uri = null;
                if (this.listenUri.Scheme == Uri.UriSchemeHttp)
                {
                    if (this.parent.HttpHelpPageEnabled)
                    {
                        uri = this.parent.HttpHelpPageUrl;
                    }
                    else if (this.parent.HttpsHelpPageEnabled)
                    {
                        uri = this.parent.HttpsGetUrl;
                    }
                }
                else if (this.parent.HttpsHelpPageEnabled)
                {
                    uri = this.parent.HttpsHelpPageUrl;
                }
                else if (this.parent.HttpHelpPageEnabled)
                {
                    uri = this.parent.HttpGetUrl;
                }
                if (addressUpdater != null)
                {
                    addressUpdater.UpdateUri(ref uri);
                }
                return new DiscoMessage(wsdlAddress, uri.ToString());
            }

            private static System.ServiceModel.Channels.Message CreateHttpResponseMessage(HttpStatusCode code)
            {
                System.ServiceModel.Channels.Message message = new NullMessage();
                HttpResponseMessageProperty property = new HttpResponseMessageProperty {
                    StatusCode = code
                };
                message.Properties.Add(HttpResponseMessageProperty.Name, property);
                return message;
            }

            private static System.ServiceModel.Channels.Message CreateRedirectMessage(string redirectedDestination)
            {
                System.ServiceModel.Channels.Message message = CreateHttpResponseMessage(HttpStatusCode.TemporaryRedirect);
                HttpResponseMessageProperty property = (HttpResponseMessageProperty) message.Properties[HttpResponseMessageProperty.Name];
                property.Headers["Location"] = redirectedDestination;
                return message;
            }

            private string FindQuery(string[] queries)
            {
                string str = null;
                foreach (string str2 in queries)
                {
                    int indexA = ((str2.Length > 0) && (str2[0] == '?')) ? 1 : 0;
                    if (string.Compare(str2, indexA, "wsdl", 0, "wsdl".Length, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        str = str2;
                    }
                    else if (string.Compare(str2, indexA, "xsd", 0, "xsd".Length, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        str = str2;
                    }
                    else if (this.parent.HelpPageEnabled && (string.Compare(str2, indexA, "disco", 0, "disco".Length, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        str = str2;
                    }
                }
                return str;
            }

            private string FindWsdlReference(ServiceMetadataExtension.DynamicAddressUpdateWriter addressUpdater)
            {
                if ((this.parent.ExternalMetadataLocation == null) || (this.parent.ExternalMetadataLocation == ServiceMetadataExtension.EmptyUri))
                {
                    return null;
                }
                Uri externalMetadataLocation = this.parent.ExternalMetadataLocation;
                Uri uri = ServiceHostBase.GetUri(this.listenUri, externalMetadataLocation);
                if (addressUpdater != null)
                {
                    addressUpdater.UpdateUri(ref uri);
                }
                return uri.ToString();
            }

            public System.ServiceModel.Channels.Message Get(System.ServiceModel.Channels.Message message)
            {
                return this.ProcessHttpRequest(message);
            }

            private string GetHttpGetUrl(ServiceMetadataExtension.DynamicAddressUpdateWriter addressUpdater)
            {
                Uri httpGetUrl = null;
                if (this.listenUri.Scheme == Uri.UriSchemeHttp)
                {
                    if (this.parent.HttpGetEnabled)
                    {
                        httpGetUrl = this.parent.HttpGetUrl;
                    }
                    else if (this.parent.HttpsGetEnabled)
                    {
                        httpGetUrl = this.parent.HttpsGetUrl;
                    }
                }
                else if (this.parent.HttpsGetEnabled)
                {
                    httpGetUrl = this.parent.HttpsGetUrl;
                }
                else if (this.parent.HttpGetEnabled)
                {
                    httpGetUrl = this.parent.HttpGetUrl;
                }
                if (httpGetUrl == null)
                {
                    return null;
                }
                if (addressUpdater != null)
                {
                    addressUpdater.UpdateUri(ref httpGetUrl);
                }
                return httpGetUrl.ToString();
            }

            private InitializationData GetInitData()
            {
                if (this.initData == null)
                {
                    lock (this.sync)
                    {
                        if (this.initData == null)
                        {
                            this.initData = InitializationData.InitializeFrom(this.parent);
                        }
                    }
                }
                return this.initData;
            }

            private string GetMexUrl(ServiceMetadataExtension.DynamicAddressUpdateWriter addressUpdater)
            {
                if (!this.parent.MexEnabled)
                {
                    return null;
                }
                Uri mexUrl = this.parent.MexUrl;
                if (addressUpdater != null)
                {
                    addressUpdater.UpdateUri(ref mexUrl);
                }
                return mexUrl.ToString();
            }

            private System.ServiceModel.Channels.Message ProcessHttpRequest(System.ServiceModel.Channels.Message httpGetRequest)
            {
                string query = httpGetRequest.Properties.Via.Query;
                if (query.Length > 0x800)
                {
                    return CreateHttpResponseMessage(HttpStatusCode.RequestUriTooLong);
                }
                if (query.StartsWith("?", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Substring(1);
                }
                string[] queries = (query.Length > 0) ? query.Split(new char[] { '&' }) : NoQueries;
                System.ServiceModel.Channels.Message replyMessage = null;
                if (this.TryHandleMetadataRequest(httpGetRequest, queries, out replyMessage))
                {
                    return replyMessage;
                }
                if (this.TryHandleDocumentationRequest(httpGetRequest, queries, out replyMessage))
                {
                    return replyMessage;
                }
                return CreateHttpResponseMessage(HttpStatusCode.MethodNotAllowed);
            }

            private bool TryHandleDocumentationRequest(System.ServiceModel.Channels.Message httpGetRequest, string[] queries, out System.ServiceModel.Channels.Message replyMessage)
            {
                replyMessage = null;
                if (!this.HelpPageEnabled)
                {
                    return false;
                }
                if (this.parent.MetadataEnabled)
                {
                    string discoUrl = null;
                    string metadataUrl = null;
                    string httpGetUrl = null;
                    bool linkMetadata = true;
                    ServiceMetadataExtension.DynamicAddressUpdateWriter addressUpdater = null;
                    if (this.parent.UpdateAddressDynamically)
                    {
                        addressUpdater = this.parent.GetDynamicAddressWriter(httpGetRequest, this.listenUri, false);
                    }
                    metadataUrl = this.FindWsdlReference(addressUpdater);
                    httpGetUrl = this.GetHttpGetUrl(addressUpdater);
                    if ((metadataUrl == null) && (httpGetUrl != null))
                    {
                        metadataUrl = httpGetUrl + "?wsdl";
                    }
                    if (httpGetUrl != null)
                    {
                        discoUrl = httpGetUrl + "?disco";
                    }
                    if (metadataUrl == null)
                    {
                        metadataUrl = this.GetMexUrl(addressUpdater);
                        linkMetadata = false;
                    }
                    replyMessage = new MetadataOnHelpPageMessage(discoUrl, metadataUrl, this.GetInitData().ServiceName, this.GetInitData().ClientName, linkMetadata);
                }
                else
                {
                    replyMessage = new MetadataOffHelpPageMessage(this.GetInitData().ServiceName);
                }
                AddHttpProperty(replyMessage, HttpStatusCode.OK, "text/html; charset=UTF-8");
                return true;
            }

            private bool TryHandleMetadataRequest(System.ServiceModel.Channels.Message httpGetRequest, string[] queries, out System.ServiceModel.Channels.Message replyMessage)
            {
                replyMessage = null;
                if (this.GetWsdlEnabled)
                {
                    object obj2;
                    ServiceMetadataExtension.WriteFilter responseWriter = this.parent.GetWriteFilter(httpGetRequest, this.listenUri, false);
                    string str = this.FindQuery(queries);
                    if (string.IsNullOrEmpty(str))
                    {
                        if (!this.helpPageEnabled && (this.GetInitData().DefaultWsdl != null))
                        {
                            using (httpGetRequest)
                            {
                                replyMessage = new ServiceDescriptionMessage(this.GetInitData().DefaultWsdl, responseWriter);
                                AddHttpProperty(replyMessage, HttpStatusCode.OK, "text/xml; charset=UTF-8");
                                this.GetInitData().FixImportAddresses();
                                return true;
                            }
                        }
                        return false;
                    }
                    if (this.GetInitData().TryQueryLookup(str, out obj2))
                    {
                        using (httpGetRequest)
                        {
                            if (obj2 is System.Web.Services.Description.ServiceDescription)
                            {
                                replyMessage = new ServiceDescriptionMessage((System.Web.Services.Description.ServiceDescription) obj2, responseWriter);
                            }
                            else if (obj2 is System.Xml.Schema.XmlSchema)
                            {
                                replyMessage = new XmlSchemaMessage((System.Xml.Schema.XmlSchema) obj2, responseWriter);
                            }
                            else
                            {
                                if (!(obj2 is string))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Bad object in HttpGetImpl docFromQuery table", new object[0])));
                                }
                                if (((string) obj2) != "disco token")
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Bad object in HttpGetImpl docFromQuery table", new object[0])));
                                }
                                replyMessage = this.CreateDiscoMessage(responseWriter as ServiceMetadataExtension.DynamicAddressUpdateWriter);
                            }
                            AddHttpProperty(replyMessage, HttpStatusCode.OK, "text/xml; charset=UTF-8");
                            this.GetInitData().FixImportAddresses();
                            return true;
                        }
                    }
                    if (string.Compare(str, "wsdl", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (this.GetInitData().DefaultWsdl != null)
                        {
                            using (httpGetRequest)
                            {
                                replyMessage = new ServiceDescriptionMessage(this.GetInitData().DefaultWsdl, responseWriter);
                                AddHttpProperty(replyMessage, HttpStatusCode.OK, "text/xml; charset=UTF-8");
                                this.GetInitData().FixImportAddresses();
                                return true;
                            }
                        }
                        string redirectedDestination = this.FindWsdlReference(responseWriter as ServiceMetadataExtension.DynamicAddressUpdateWriter);
                        if (redirectedDestination != null)
                        {
                            replyMessage = CreateRedirectMessage(redirectedDestination);
                            return true;
                        }
                    }
                }
                return false;
            }

            public bool GetWsdlEnabled
            {
                get
                {
                    return this.getWsdlEnabled;
                }
                set
                {
                    this.getWsdlEnabled = value;
                }
            }

            public bool HelpPageEnabled
            {
                get
                {
                    return this.helpPageEnabled;
                }
                set
                {
                    this.helpPageEnabled = value;
                }
            }

            private class DiscoMessage : ContentOnlyMessage
            {
                private string docAddress;
                private string wsdlAddress;

                public DiscoMessage(string wsdlAddress, string docAddress)
                {
                    this.wsdlAddress = wsdlAddress;
                    this.docAddress = docAddress;
                }

                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("discovery", "http://schemas.xmlsoap.org/disco/");
                    writer.WriteStartElement("contractRef", "http://schemas.xmlsoap.org/disco/scl/");
                    writer.WriteAttributeString("ref", this.wsdlAddress);
                    writer.WriteAttributeString("docRef", this.docAddress);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }

            private class InitializationData
            {
                public string ClientName;
                public System.Web.Services.Description.ServiceDescription DefaultWsdl;
                private readonly Dictionary<string, object> docFromQuery;
                private readonly Dictionary<object, string> queryFromDoc;
                public string ServiceName;
                private ServiceDescriptionCollection wsdls;
                private XmlSchemaSet xsds;

                private InitializationData(Dictionary<string, object> docFromQuery, Dictionary<object, string> queryFromDoc, ServiceDescriptionCollection wsdls, XmlSchemaSet xsds)
                {
                    this.docFromQuery = docFromQuery;
                    this.queryFromDoc = queryFromDoc;
                    this.wsdls = wsdls;
                    this.xsds = xsds;
                }

                private static ServiceDescriptionCollection CollectWsdls(MetadataSet metadata)
                {
                    ServiceDescriptionCollection descriptions = new ServiceDescriptionCollection();
                    foreach (MetadataSection section in metadata.MetadataSections)
                    {
                        if (section.Metadata is System.Web.Services.Description.ServiceDescription)
                        {
                            descriptions.Add((System.Web.Services.Description.ServiceDescription) section.Metadata);
                        }
                    }
                    return descriptions;
                }

                private static XmlSchemaSet CollectXsds(MetadataSet metadata)
                {
                    XmlSchemaSet set = new XmlSchemaSet {
                        XmlResolver = null
                    };
                    foreach (MetadataSection section in metadata.MetadataSections)
                    {
                        if (section.Metadata is System.Xml.Schema.XmlSchema)
                        {
                            set.Add((System.Xml.Schema.XmlSchema) section.Metadata);
                        }
                    }
                    return set;
                }

                internal void FixImportAddresses()
                {
                    foreach (System.Web.Services.Description.ServiceDescription description in this.wsdls)
                    {
                        this.FixImportAddresses(description);
                    }
                    foreach (System.Xml.Schema.XmlSchema schema in this.xsds.Schemas())
                    {
                        this.FixImportAddresses(schema);
                    }
                }

                private void FixImportAddresses(System.Web.Services.Description.ServiceDescription wsdlDoc)
                {
                    foreach (Import import in wsdlDoc.Imports)
                    {
                        if (string.IsNullOrEmpty(import.Location))
                        {
                            System.Web.Services.Description.ServiceDescription description = this.wsdls[import.Namespace ?? string.Empty];
                            if (description != null)
                            {
                                string str = this.queryFromDoc[description];
                                import.Location = "{%BaseAddress%}?" + str;
                            }
                        }
                    }
                    if (wsdlDoc.Types != null)
                    {
                        foreach (System.Xml.Schema.XmlSchema schema in wsdlDoc.Types.Schemas)
                        {
                            this.FixImportAddresses(schema);
                        }
                    }
                }

                private void FixImportAddresses(System.Xml.Schema.XmlSchema xsdDoc)
                {
                    foreach (XmlSchemaExternal external in xsdDoc.Includes)
                    {
                        if ((external != null) && string.IsNullOrEmpty(external.SchemaLocation))
                        {
                            string str = (external is XmlSchemaImport) ? ((XmlSchemaImport) external).Namespace : xsdDoc.TargetNamespace;
                            foreach (System.Xml.Schema.XmlSchema schema in this.xsds.Schemas(str ?? string.Empty))
                            {
                                if (schema != xsdDoc)
                                {
                                    string str2 = this.queryFromDoc[schema];
                                    external.SchemaLocation = "{%BaseAddress%}?" + str2;
                                    break;
                                }
                            }
                        }
                    }
                }

                private static string GetAnyContractName(ServiceDescriptionCollection wsdls)
                {
                    foreach (System.Web.Services.Description.ServiceDescription description in wsdls)
                    {
                        foreach (Service service in description.Services)
                        {
                            foreach (Port port in service.Ports)
                            {
                                if (!port.Binding.IsEmpty)
                                {
                                    System.Web.Services.Description.Binding binding = wsdls.GetBinding(port.Binding);
                                    if (!binding.Type.IsEmpty)
                                    {
                                        return binding.Type.Name;
                                    }
                                }
                            }
                        }
                    }
                    return null;
                }

                private static Service GetAnyService(ServiceDescriptionCollection wsdls)
                {
                    foreach (System.Web.Services.Description.ServiceDescription description in wsdls)
                    {
                        if (description.Services.Count > 0)
                        {
                            return description.Services[0];
                        }
                    }
                    return null;
                }

                private static string GetAnyWsdlName(ServiceDescriptionCollection wsdls)
                {
                    foreach (System.Web.Services.Description.ServiceDescription description in wsdls)
                    {
                        if (!string.IsNullOrEmpty(description.Name))
                        {
                            return description.Name;
                        }
                    }
                    return null;
                }

                public static ServiceMetadataExtension.HttpGetImpl.InitializationData InitializeFrom(ServiceMetadataExtension extension)
                {
                    Dictionary<string, object> docFromQuery = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    Dictionary<object, string> queryFromDoc = new Dictionary<object, string>();
                    ServiceDescriptionCollection wsdls = CollectWsdls(extension.Metadata);
                    XmlSchemaSet xsds = CollectXsds(extension.Metadata);
                    System.Web.Services.Description.ServiceDescription serviceDescription = null;
                    Service anyService = GetAnyService(wsdls);
                    if (anyService != null)
                    {
                        serviceDescription = anyService.ServiceDescription;
                    }
                    int num = 0;
                    foreach (System.Web.Services.Description.ServiceDescription description2 in wsdls)
                    {
                        string key = "wsdl";
                        if (description2 != serviceDescription)
                        {
                            key = key + "=wsdl" + num++.ToString(CultureInfo.InvariantCulture);
                        }
                        docFromQuery.Add(key, description2);
                        queryFromDoc.Add(description2, key);
                    }
                    int num2 = 0;
                    foreach (System.Xml.Schema.XmlSchema schema in xsds.Schemas())
                    {
                        string str2 = "xsd=xsd" + num2++.ToString(CultureInfo.InvariantCulture);
                        docFromQuery.Add(str2, schema);
                        queryFromDoc.Add(schema, str2);
                    }
                    if (extension.HelpPageEnabled)
                    {
                        string str3 = "disco";
                        docFromQuery.Add(str3, "disco token");
                        queryFromDoc.Add("disco token", str3);
                    }
                    return new ServiceMetadataExtension.HttpGetImpl.InitializationData(docFromQuery, queryFromDoc, wsdls, xsds) { DefaultWsdl = serviceDescription, ServiceName = GetAnyWsdlName(wsdls), ClientName = ClientClassGenerator.GetClientClassName(GetAnyContractName(wsdls) ?? "IHello") };
                }

                public bool TryQueryLookup(string query, out object doc)
                {
                    return this.docFromQuery.TryGetValue(query, out doc);
                }
            }

            private class MetadataOffHelpPageMessage : ContentOnlyMessage
            {
                public MetadataOffHelpPageMessage(string serviceName)
                {
                }

                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    writer.WriteStartElement("HTML");
                    writer.WriteStartElement("HEAD");
                    writer.WriteRaw(string.Format(CultureInfo.InvariantCulture, "<STYLE type=\"text/css\">#content{{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}}BODY{{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}}P{{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}}PRE{{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}}.heading1{{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; MARGIN-BOTTOM: 0px; PADDING-BOTTOM: 3px; MARGIN-LEFT: -30px; WIDTH: 100%; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #003366}}.intro{{MARGIN-LEFT: -15px}}</STYLE>\r\n<TITLE>Service</TITLE>", new object[0]));
                    writer.WriteEndElement();
                    writer.WriteRaw(string.Format(CultureInfo.InvariantCulture, "<BODY>\r\n<DIV id=\"content\">\r\n<P class=\"heading1\">Service</P>\r\n<BR/>\r\n<P class=\"intro\">{0}</P>\r\n<PRE>\r\n<font color=\"blue\">&lt;<font color=\"darkred\">behaviors</font>&gt;</font>\r\n<font color=\"blue\">    &lt;<font color=\"darkred\">serviceBehaviors</font>&gt;</font>\r\n<font color=\"blue\">        &lt;<font color=\"darkred\">behavior </font><font color=\"red\">name</font>=<font color=\"black\">\"</font>MyServiceTypeBehaviors<font color=\"black\">\" </font>&gt;</font>\r\n<font color=\"blue\">            &lt;<font color=\"darkred\">serviceMetadata </font><font color=\"red\">httpGetEnabled</font>=<font color=\"black\">\"</font>true<font color=\"black\">\" </font>/&gt;</font>\r\n<font color=\"blue\">        &lt;<font color=\"darkred\">/behavior</font>&gt;</font>\r\n<font color=\"blue\">    &lt;<font color=\"darkred\">/serviceBehaviors</font>&gt;</font>\r\n<font color=\"blue\">&lt;<font color=\"darkred\">/behaviors</font>&gt;</font>\r\n</PRE>\r\n<P class=\"intro\">{1}</P>\r\n<PRE>\r\n<font color=\"blue\">&lt;<font color=\"darkred\">service </font><font color=\"red\">name</font>=<font color=\"black\">\"</font><i>MyNamespace.MyServiceType</i><font color=\"black\">\" </font><font color=\"red\">behaviorConfiguration</font>=<font color=\"black\">\"</font><i>MyServiceTypeBehaviors</i><font color=\"black\">\" </font>&gt;</font>\r\n</PRE>\r\n<P class=\"intro\">{2}</P>\r\n<PRE>\r\n<font color=\"blue\">&lt;<font color=\"darkred\">endpoint </font><font color=\"red\">contract</font>=<font color=\"black\">\"</font>IMetadataExchange<font color=\"black\">\" </font><font color=\"red\">binding</font>=<font color=\"black\">\"</font>mexHttpBinding<font color=\"black\">\" </font><font color=\"red\">address</font>=<font color=\"black\">\"</font>mex<font color=\"black\">\" </font>/&gt;</font>\r\n</PRE>\r\n\r\n<P class=\"intro\">{3}</P>\r\n<PRE>\r\n<font color=\"blue\">&lt;<font color=\"darkred\">configuration</font>&gt;</font>\r\n<font color=\"blue\">    &lt;<font color=\"darkred\">system.serviceModel</font>&gt;</font>\r\n \r\n<font color=\"blue\">        &lt;<font color=\"darkred\">services</font>&gt;</font>\r\n<font color=\"blue\">            &lt;!-- <font color=\"green\">{4}</font> --&gt;</font>\r\n<font color=\"blue\">            &lt;<font color=\"darkred\">service </font><font color=\"red\">name</font>=<font color=\"black\">\"</font><i>MyNamespace.MyServiceType</i><font color=\"black\">\" </font><font color=\"red\">behaviorConfiguration</font>=<font color=\"black\">\"</font><i>MyServiceTypeBehaviors</i><font color=\"black\">\" </font>&gt;</font>\r\n<font color=\"blue\">                &lt;!-- <font color=\"green\">{5}</font> --&gt;</font>\r\n<font color=\"blue\">                &lt;!-- <font color=\"green\">{6}</font> --&gt;</font>\r\n<font color=\"blue\">                &lt;<font color=\"darkred\">endpoint </font><font color=\"red\">contract</font>=<font color=\"black\">\"</font>IMetadataExchange<font color=\"black\">\" </font><font color=\"red\">binding</font>=<font color=\"black\">\"</font>mexHttpBinding<font color=\"black\">\" </font><font color=\"red\">address</font>=<font color=\"black\">\"</font>mex<font color=\"black\">\" </font>/&gt;</font>\r\n<font color=\"blue\">            &lt;<font color=\"darkred\">/service</font>&gt;</font>\r\n<font color=\"blue\">        &lt;<font color=\"darkred\">/services</font>&gt;</font>\r\n \r\n<font color=\"blue\">        &lt;<font color=\"darkred\">behaviors</font>&gt;</font>\r\n<font color=\"blue\">            &lt;<font color=\"darkred\">serviceBehaviors</font>&gt;</font>\r\n<font color=\"blue\">                &lt;<font color=\"darkred\">behavior </font><font color=\"red\">name</font>=<font color=\"black\">\"</font><i>MyServiceTypeBehaviors</i><font color=\"black\">\" </font>&gt;</font>\r\n<font color=\"blue\">                    &lt;!-- <font color=\"green\">{7}</font> --&gt;</font>\r\n<font color=\"blue\">                    &lt;<font color=\"darkred\">serviceMetadata </font><font color=\"red\">httpGetEnabled</font>=<font color=\"black\">\"</font>true<font color=\"black\">\" </font>/&gt;</font>\r\n<font color=\"blue\">                &lt;<font color=\"darkred\">/behavior</font>&gt;</font>\r\n<font color=\"blue\">            &lt;<font color=\"darkred\">/serviceBehaviors</font>&gt;</font>\r\n<font color=\"blue\">        &lt;<font color=\"darkred\">/behaviors</font>&gt;</font>\r\n \r\n<font color=\"blue\">    &lt;<font color=\"darkred\">/system.serviceModel</font>&gt;</font>\r\n<font color=\"blue\">&lt;<font color=\"darkred\">/configuration</font>&gt;</font>\r\n</PRE>\r\n<P class=\"intro\">{8}</P>\r\n</DIV>\r\n</BODY>", new object[] { System.ServiceModel.SR.GetString("SFxDocExt_NoMetadataSection1"), System.ServiceModel.SR.GetString("SFxDocExt_NoMetadataSection2"), System.ServiceModel.SR.GetString("SFxDocExt_NoMetadataSection3"), System.ServiceModel.SR.GetString("SFxDocExt_NoMetadataSection4"), System.ServiceModel.SR.GetString("SFxDocExt_NoMetadataConfigComment1"), System.ServiceModel.SR.GetString("SFxDocExt_NoMetadataConfigComment2"), System.ServiceModel.SR.GetString("SFxDocExt_NoMetadataConfigComment3"), System.ServiceModel.SR.GetString("SFxDocExt_NoMetadataConfigComment4"), System.ServiceModel.SR.GetString("SFxDocExt_NoMetadataSection5") }));
                    writer.WriteEndElement();
                }
            }

            private class MetadataOnHelpPageMessage : ContentOnlyMessage
            {
                private string clientName;
                private string discoUrl;
                private string errorMessage;
                private ExceptionDetail exceptionDetail;
                private bool linkMetadata;
                private string metadataUrl;
                private string serviceName;

                public MetadataOnHelpPageMessage(string errorMessage, ExceptionDetail exceptionDetail)
                {
                    this.errorMessage = errorMessage;
                    this.exceptionDetail = exceptionDetail;
                }

                public MetadataOnHelpPageMessage(string discoUrl, string metadataUrl, string serviceName, string clientName, bool linkMetadata)
                {
                    this.discoUrl = discoUrl;
                    this.metadataUrl = metadataUrl;
                    this.serviceName = serviceName;
                    this.clientName = clientName;
                    this.linkMetadata = linkMetadata;
                }

                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    HelpPageWriter writer2 = new HelpPageWriter(writer);
                    writer.WriteStartElement("HTML");
                    writer.WriteStartElement("HEAD");
                    if (!string.IsNullOrEmpty(this.discoUrl))
                    {
                        writer2.WriteDiscoLink(this.discoUrl);
                    }
                    writer2.WriteStyleSheet();
                    writer2.WriteTitle(!string.IsNullOrEmpty(this.serviceName) ? System.ServiceModel.SR.GetString("SFxDocExt_MainPageTitle", new object[] { this.serviceName }) : System.ServiceModel.SR.GetString("SFxDocExt_MainPageTitleNoServiceName"));
                    if (!string.IsNullOrEmpty(this.errorMessage))
                    {
                        writer2.WriteError(this.errorMessage);
                        if (this.exceptionDetail != null)
                        {
                            writer2.WriteExceptionDetail(this.exceptionDetail);
                        }
                    }
                    else
                    {
                        writer2.WriteToolUsage(this.metadataUrl, this.linkMetadata);
                        writer2.WriteSampleCode(this.clientName);
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                [StructLayout(LayoutKind.Sequential)]
                private struct HelpPageWriter
                {
                    private XmlWriter writer;
                    public HelpPageWriter(XmlWriter writer)
                    {
                        this.writer = writer;
                    }

                    internal void WriteClass(string className)
                    {
                        this.writer.WriteStartElement("font");
                        this.writer.WriteAttributeString("color", "teal");
                        this.writer.WriteString(className);
                        this.writer.WriteEndElement();
                    }

                    internal void WriteComment(string comment)
                    {
                        this.writer.WriteStartElement("font");
                        this.writer.WriteAttributeString("color", "green");
                        this.writer.WriteString(comment);
                        this.writer.WriteEndElement();
                    }

                    internal void WriteDiscoLink(string discoUrl)
                    {
                        this.writer.WriteStartElement("link");
                        this.writer.WriteAttributeString("rel", "alternate");
                        this.writer.WriteAttributeString("type", "text/xml");
                        this.writer.WriteAttributeString("href", discoUrl);
                        this.writer.WriteEndElement();
                    }

                    internal void WriteError(string message)
                    {
                        this.writer.WriteStartElement("P");
                        this.writer.WriteAttributeString("class", "intro");
                        this.writer.WriteString(message);
                        this.writer.WriteEndElement();
                    }

                    internal void WriteKeyword(string keyword)
                    {
                        this.writer.WriteStartElement("font");
                        this.writer.WriteAttributeString("color", "blue");
                        this.writer.WriteString(keyword);
                        this.writer.WriteEndElement();
                    }

                    internal void WriteSampleCode(string clientName)
                    {
                        this.writer.WriteStartElement("P");
                        this.writer.WriteAttributeString("class", "intro");
                        this.writer.WriteEndElement();
                        this.writer.WriteRaw(System.ServiceModel.SR.GetString("SFxDocExt_MainPageIntro2"));
                        this.writer.WriteRaw(System.ServiceModel.SR.GetString("SFxDocExt_CS"));
                        this.writer.WriteStartElement("PRE");
                        this.WriteKeyword("class ");
                        this.WriteClass("Test\n");
                        this.writer.WriteString("{\n");
                        this.WriteKeyword("    static void ");
                        this.writer.WriteString("Main()\n");
                        this.writer.WriteString("    {\n");
                        this.writer.WriteString("        ");
                        this.WriteClass(clientName);
                        this.writer.WriteString(" client = ");
                        this.WriteKeyword("new ");
                        this.WriteClass(clientName);
                        this.writer.WriteString("();\n\n");
                        this.WriteComment("        // " + System.ServiceModel.SR.GetString("SFxDocExt_MainPageComment") + "\n\n");
                        this.WriteComment("        // " + System.ServiceModel.SR.GetString("SFxDocExt_MainPageComment2") + "\n");
                        this.writer.WriteString("        client.Close();\n");
                        this.writer.WriteString("    }\n");
                        this.writer.WriteString("}\n");
                        this.writer.WriteEndElement();
                        this.writer.WriteRaw("<BR/>");
                        this.writer.WriteRaw(System.ServiceModel.SR.GetString("SFxDocExt_VB"));
                        this.writer.WriteStartElement("PRE");
                        this.WriteKeyword("Class ");
                        this.WriteClass("Test\n");
                        this.WriteKeyword("    Shared Sub ");
                        this.writer.WriteString("Main()\n");
                        this.WriteKeyword("        Dim ");
                        this.writer.WriteString("client As ");
                        this.WriteClass(clientName);
                        this.writer.WriteString(" = ");
                        this.WriteKeyword("New ");
                        this.WriteClass(clientName);
                        this.writer.WriteString("()\n");
                        this.WriteComment("        ' " + System.ServiceModel.SR.GetString("SFxDocExt_MainPageComment") + "\n\n");
                        this.WriteComment("        ' " + System.ServiceModel.SR.GetString("SFxDocExt_MainPageComment2") + "\n");
                        this.writer.WriteString("        client.Close()\n");
                        this.WriteKeyword("    End Sub\n");
                        this.WriteKeyword("End Class");
                        this.writer.WriteEndElement();
                    }

                    internal void WriteExceptionDetail(ExceptionDetail exceptionDetail)
                    {
                        this.writer.WriteStartElement("PRE");
                        this.writer.WriteString(exceptionDetail.ToString().Replace("\r", ""));
                        this.writer.WriteEndElement();
                    }

                    internal void WriteStyleSheet()
                    {
                        this.writer.WriteStartElement("STYLE");
                        this.writer.WriteAttributeString("type", "text/css");
                        this.writer.WriteString("#content{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}");
                        this.writer.WriteString("BODY{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}");
                        this.writer.WriteString("P{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}");
                        this.writer.WriteString("PRE{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}");
                        this.writer.WriteString(".heading1{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; MARGIN-BOTTOM: 0px; PADDING-BOTTOM: 3px; MARGIN-LEFT: -30px; WIDTH: 100%; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #003366}");
                        this.writer.WriteString(".intro{MARGIN-LEFT: -15px}");
                        this.writer.WriteEndElement();
                    }

                    internal void WriteTitle(string title)
                    {
                        this.writer.WriteElementString("TITLE", title);
                        this.writer.WriteEndElement();
                        this.writer.WriteStartElement("BODY");
                        this.writer.WriteStartElement("DIV");
                        this.writer.WriteAttributeString("id", "content");
                        this.writer.WriteStartElement("P");
                        this.writer.WriteAttributeString("class", "heading1");
                        this.writer.WriteString(title);
                        this.writer.WriteEndElement();
                        this.writer.WriteRaw("<BR/>");
                    }

                    internal void WriteToolUsage(string wsdlUrl, bool linkMetadata)
                    {
                        this.writer.WriteStartElement("P");
                        this.writer.WriteAttributeString("class", "intro");
                        if (wsdlUrl != null)
                        {
                            this.writer.WriteRaw(System.ServiceModel.SR.GetString("SFxDocExt_MainPageIntro1a"));
                            this.writer.WriteRaw("<BR/>");
                            this.writer.WriteStartElement("PRE");
                            this.writer.WriteString("svcutil.exe ");
                            if (linkMetadata)
                            {
                                this.writer.WriteStartElement("A");
                                this.writer.WriteAttributeString("HREF", wsdlUrl);
                            }
                            this.writer.WriteString(wsdlUrl);
                            if (linkMetadata)
                            {
                                this.writer.WriteEndElement();
                            }
                            this.writer.WriteEndElement();
                        }
                        else
                        {
                            this.writer.WriteRaw(System.ServiceModel.SR.GetString("SFxDocExt_MainPageIntro1b"));
                        }
                        this.writer.WriteEndElement();
                    }
                }
            }

            private class ServiceDescriptionMessage : ContentOnlyMessage
            {
                private System.Web.Services.Description.ServiceDescription description;
                private ServiceMetadataExtension.WriteFilter responseWriter;

                public ServiceDescriptionMessage(System.Web.Services.Description.ServiceDescription description, ServiceMetadataExtension.WriteFilter responseWriter)
                {
                    this.description = description;
                    this.responseWriter = responseWriter;
                }

                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    this.responseWriter.Writer = writer;
                    this.description.Write(this.responseWriter);
                }
            }

            private class XmlSchemaMessage : ContentOnlyMessage
            {
                private ServiceMetadataExtension.WriteFilter responseWriter;
                private System.Xml.Schema.XmlSchema schema;

                public XmlSchemaMessage(System.Xml.Schema.XmlSchema schema, ServiceMetadataExtension.WriteFilter responseWriter)
                {
                    this.schema = schema;
                    this.responseWriter = responseWriter;
                }

                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    this.responseWriter.Writer = writer;
                    this.schema.Write(this.responseWriter);
                }
            }
        }

        [ServiceContract]
        internal interface IHttpGetMetadata
        {
            [OperationContract(Action="*", ReplyAction="*")]
            System.ServiceModel.Channels.Message Get(System.ServiceModel.Channels.Message msg);
        }

        private class LocationUpdatingWriter : ServiceMetadataExtension.WriteFilter
        {
            private readonly string newValue;
            private readonly string oldValue;

            internal LocationUpdatingWriter(string oldValue, string newValue)
            {
                this.oldValue = oldValue;
                this.newValue = newValue;
            }

            public override ServiceMetadataExtension.WriteFilter CloneWriteFilter()
            {
                return new ServiceMetadataExtension.LocationUpdatingWriter(this.oldValue, this.newValue);
            }

            public override void WriteString(string text)
            {
                if (this.newValue != null)
                {
                    text = text.Replace(this.oldValue, this.newValue);
                }
                else if (text.StartsWith(this.oldValue, StringComparison.Ordinal))
                {
                    text = string.Empty;
                }
                base.WriteString(text);
            }
        }

        internal abstract class WriteFilter : XmlDictionaryWriter
        {
            internal XmlWriter Writer;

            protected WriteFilter()
            {
            }

            public abstract ServiceMetadataExtension.WriteFilter CloneWriteFilter();
            public override void Close()
            {
                this.Writer.Close();
            }

            public override void Flush()
            {
                this.Writer.Flush();
            }

            public override string LookupPrefix(string ns)
            {
                return this.Writer.LookupPrefix(ns);
            }

            public override void WriteBase64(byte[] buffer, int index, int count)
            {
                this.Writer.WriteBase64(buffer, index, count);
            }

            public override void WriteCData(string text)
            {
                this.Writer.WriteCData(text);
            }

            public override void WriteCharEntity(char ch)
            {
                this.Writer.WriteCharEntity(ch);
            }

            public override void WriteChars(char[] buffer, int index, int count)
            {
                this.Writer.WriteChars(buffer, index, count);
            }

            public override void WriteComment(string text)
            {
                this.Writer.WriteComment(text);
            }

            public override void WriteDocType(string name, string pubid, string sysid, string subset)
            {
                this.Writer.WriteDocType(name, pubid, sysid, subset);
            }

            public override void WriteEndAttribute()
            {
                this.Writer.WriteEndAttribute();
            }

            public override void WriteEndDocument()
            {
                this.Writer.WriteEndDocument();
            }

            public override void WriteEndElement()
            {
                this.Writer.WriteEndElement();
            }

            public override void WriteEntityRef(string name)
            {
                this.Writer.WriteEntityRef(name);
            }

            public override void WriteFullEndElement()
            {
                this.Writer.WriteFullEndElement();
            }

            public override void WriteProcessingInstruction(string name, string text)
            {
                this.Writer.WriteProcessingInstruction(name, text);
            }

            public override void WriteRaw(string data)
            {
                this.Writer.WriteRaw(data);
            }

            public override void WriteRaw(char[] buffer, int index, int count)
            {
                this.Writer.WriteRaw(buffer, index, count);
            }

            public override void WriteStartAttribute(string prefix, string localName, string ns)
            {
                this.Writer.WriteStartAttribute(prefix, localName, ns);
            }

            public override void WriteStartDocument()
            {
                this.Writer.WriteStartDocument();
            }

            public override void WriteStartDocument(bool standalone)
            {
                this.Writer.WriteStartDocument(standalone);
            }

            public override void WriteStartElement(string prefix, string localName, string ns)
            {
                this.Writer.WriteStartElement(prefix, localName, ns);
            }

            public override void WriteString(string text)
            {
                this.Writer.WriteString(text);
            }

            public override void WriteSurrogateCharEntity(char lowChar, char highChar)
            {
                this.Writer.WriteSurrogateCharEntity(lowChar, highChar);
            }

            public override void WriteWhitespace(string ws)
            {
                this.Writer.WriteWhitespace(ws);
            }

            public override System.Xml.WriteState WriteState
            {
                get
                {
                    return this.Writer.WriteState;
                }
            }
        }

        internal class WSMexImpl : IMetadataExchange
        {
            internal const string ContractName = "WS-Transfer";
            internal const string ContractNamespace = "http://schemas.xmlsoap.org/ws/2004/09/transfer";
            private TypedMessageConverter converter;
            internal const string GetMethodName = "Get";
            private bool isListeningOnHttps;
            private Uri listenUri;
            private MetadataSet metadataLocationSet;
            internal const string MetadataMexBinding = "ServiceMetadataBehaviorMexBinding";
            private ServiceMetadataExtension parent;
            internal const string ReplyAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse";
            internal const string RequestAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get";

            internal WSMexImpl(ServiceMetadataExtension parent, bool isListeningOnHttps, Uri listenUri)
            {
                this.parent = parent;
                this.isListeningOnHttps = isListeningOnHttps;
                this.listenUri = listenUri;
                if ((this.parent.ExternalMetadataLocation != null) && (this.parent.ExternalMetadataLocation != ServiceMetadataExtension.EmptyUri))
                {
                    this.metadataLocationSet = new MetadataSet();
                    string locationToReturn = this.GetLocationToReturn();
                    MetadataSection item = new MetadataSection(MetadataSection.ServiceDescriptionDialect, null, new MetadataLocation(locationToReturn));
                    this.metadataLocationSet.MetadataSections.Add(item);
                }
            }

            public IAsyncResult BeginGet(System.ServiceModel.Channels.Message request, AsyncCallback callback, object state)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            public System.ServiceModel.Channels.Message EndGet(IAsyncResult result)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            private MetadataSet GatherMetadata(string dialect, string identifier)
            {
                if (this.metadataLocationSet != null)
                {
                    return this.metadataLocationSet;
                }
                MetadataSet set = new MetadataSet();
                foreach (MetadataSection section in this.parent.Metadata.MetadataSections)
                {
                    if (((dialect == null) || (dialect == section.Dialect)) && ((identifier == null) || (identifier == section.Identifier)))
                    {
                        set.MetadataSections.Add(section);
                    }
                }
                return set;
            }

            public System.ServiceModel.Channels.Message Get(System.ServiceModel.Channels.Message request)
            {
                GetResponse typedMessage = new GetResponse {
                    Metadata = this.GatherMetadata(null, null)
                };
                typedMessage.Metadata.WriteFilter = this.parent.GetWriteFilter(request, this.listenUri, true);
                if (this.converter == null)
                {
                    this.converter = TypedMessageConverter.Create(typeof(GetResponse), "http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse");
                }
                return this.converter.ToMessage(typedMessage, request.Version);
            }

            private string GetLocationToReturn()
            {
                Uri externalMetadataLocation = this.parent.ExternalMetadataLocation;
                if (!externalMetadataLocation.IsAbsoluteUri)
                {
                    Uri via = this.parent.owner.GetVia(Uri.UriSchemeHttp, externalMetadataLocation);
                    Uri uri3 = this.parent.owner.GetVia(Uri.UriSchemeHttps, externalMetadataLocation);
                    if (!this.IsListeningOnHttps || (uri3 == null))
                    {
                        if (via == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ExternalMetadataLocation", System.ServiceModel.SR.GetString("SFxBadMetadataLocationNoAppropriateBaseAddress", new object[] { this.parent.ExternalMetadataLocation.OriginalString }));
                        }
                        externalMetadataLocation = via;
                    }
                    else
                    {
                        externalMetadataLocation = uri3;
                    }
                }
                return externalMetadataLocation.ToString();
            }

            internal bool IsListeningOnHttps
            {
                get
                {
                    return this.isListeningOnHttps;
                }
                set
                {
                    this.isListeningOnHttps = value;
                }
            }
        }
    }
}

