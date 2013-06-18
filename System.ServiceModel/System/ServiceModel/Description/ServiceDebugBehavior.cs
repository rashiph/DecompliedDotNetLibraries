namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;

    public class ServiceDebugBehavior : IServiceBehavior
    {
        private Binding httpHelpPageBinding;
        private bool httpHelpPageEnabled = true;
        private Uri httpHelpPageUrl;
        private Binding httpsHelpPageBinding;
        private bool httpsHelpPageEnabled = true;
        private Uri httpsHelpPageUrl;
        private bool includeExceptionDetailInFaults;

        private void CreateHelpPageEndpoints(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase host, ServiceMetadataExtension mex)
        {
            if (this.httpHelpPageEnabled && !this.EnsureHelpPageDispatcher(host, mex, this.httpHelpPageUrl, Uri.UriSchemeHttp))
            {
                TraceWarning(this.httpHelpPageUrl, "ServiceDebugBehaviorHttpHelpPageUrl", "ServiceDebugBehaviorHttpHelpPageEnabled");
            }
            if (this.httpsHelpPageEnabled && !this.EnsureHelpPageDispatcher(host, mex, this.httpsHelpPageUrl, Uri.UriSchemeHttps))
            {
                TraceWarning(this.httpHelpPageUrl, "ServiceDebugBehaviorHttpsHelpPageUrl", "ServiceDebugBehaviorHttpsHelpPageEnabled");
            }
        }

        private bool EnsureHelpPageDispatcher(ServiceHostBase host, ServiceMetadataExtension mex, Uri url, string scheme)
        {
            Uri via = host.GetVia(scheme, (url == null) ? new Uri(string.Empty, UriKind.Relative) : url);
            if (via == null)
            {
                return false;
            }
            ((ServiceMetadataExtension.HttpGetImpl) mex.EnsureGetDispatcher(via, 1).Endpoints[0].DispatchRuntime.SingletonInstanceContext.UserObject).HelpPageEnabled = true;
            return true;
        }

        private void SetExtensionProperties(ServiceMetadataExtension mex, ServiceHostBase host)
        {
            mex.HttpHelpPageEnabled = this.httpHelpPageEnabled;
            mex.HttpHelpPageUrl = host.GetVia(Uri.UriSchemeHttp, (this.httpHelpPageUrl == null) ? new Uri(string.Empty, UriKind.Relative) : this.httpHelpPageUrl);
            mex.HttpHelpPageBinding = this.HttpHelpPageBinding;
            mex.HttpsHelpPageEnabled = this.httpsHelpPageEnabled;
            mex.HttpsHelpPageUrl = host.GetVia(Uri.UriSchemeHttps, (this.httpsHelpPageUrl == null) ? new Uri(string.Empty, UriKind.Relative) : this.httpsHelpPageUrl);
            mex.HttpsHelpPageBinding = this.HttpsHelpPageBinding;
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (this.includeExceptionDetailInFaults)
            {
                for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
                {
                    ChannelDispatcher dispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                    if (dispatcher != null)
                    {
                        dispatcher.IncludeExceptionDetailInFaults = true;
                    }
                }
            }
            if (this.httpHelpPageEnabled || this.httpsHelpPageEnabled)
            {
                ServiceMetadataExtension mex = ServiceMetadataExtension.EnsureServiceMetadataExtension(description, serviceHostBase);
                this.SetExtensionProperties(mex, serviceHostBase);
                this.CreateHelpPageEndpoints(description, serviceHostBase, mex);
            }
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        private static void TraceWarning(Uri address, string urlProperty, string enabledProperty)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Hashtable hashtable2 = new Hashtable(2);
                hashtable2.Add(enabledProperty, "true");
                hashtable2.Add(urlProperty, (address == null) ? string.Empty : address.ToString());
                Hashtable dictionary = hashtable2;
                TraceUtility.TraceEvent(TraceEventType.Information, 0x8005d, System.ServiceModel.SR.GetString("TraceCodeWarnHelpPageEnabledNoBaseAddress"), new DictionaryTraceRecord(dictionary), null, null);
            }
        }

        public Binding HttpHelpPageBinding
        {
            get
            {
                return this.httpHelpPageBinding;
            }
            set
            {
                if (value != null)
                {
                    if (!value.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxBindingSchemeDoesNotMatch", new object[] { value.Scheme, value.GetType().ToString(), Uri.UriSchemeHttp }));
                    }
                    CustomBinding binding = new CustomBinding(value);
                    TextMessageEncodingBindingElement element = binding.Elements.Find<TextMessageEncodingBindingElement>();
                    if ((element != null) && !element.MessageVersion.IsMatch(MessageVersion.None))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxIncorrectMessageVersion", new object[] { element.MessageVersion.ToString(), MessageVersion.None.ToString() }));
                    }
                    HttpTransportBindingElement element2 = binding.Elements.Find<HttpTransportBindingElement>();
                    if (element2 != null)
                    {
                        element2.Method = "GET";
                    }
                    this.httpHelpPageBinding = binding;
                }
            }
        }

        [DefaultValue(true)]
        public bool HttpHelpPageEnabled
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

        [DefaultValue((string) null), TypeConverter(typeof(UriTypeConverter))]
        public Uri HttpHelpPageUrl
        {
            get
            {
                return this.httpHelpPageUrl;
            }
            set
            {
                if (((value != null) && value.IsAbsoluteUri) && (value.Scheme != Uri.UriSchemeHttp))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxServiceMetadataBehaviorUrlMustBeHttpOrRelative", new object[] { "HttpHelpPageUrl", Uri.UriSchemeHttp, value.ToString(), value.Scheme }));
                }
                this.httpHelpPageUrl = value;
            }
        }

        public Binding HttpsHelpPageBinding
        {
            get
            {
                return this.httpsHelpPageBinding;
            }
            set
            {
                if (value != null)
                {
                    if (!value.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxBindingSchemeDoesNotMatch", new object[] { value.Scheme, value.GetType().ToString(), Uri.UriSchemeHttps }));
                    }
                    CustomBinding binding = new CustomBinding(value);
                    TextMessageEncodingBindingElement element = binding.Elements.Find<TextMessageEncodingBindingElement>();
                    if ((element != null) && !element.MessageVersion.IsMatch(MessageVersion.None))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxIncorrectMessageVersion", new object[] { element.MessageVersion.ToString(), MessageVersion.None.ToString() }));
                    }
                    HttpsTransportBindingElement element2 = binding.Elements.Find<HttpsTransportBindingElement>();
                    if (element2 != null)
                    {
                        element2.Method = "GET";
                    }
                    this.httpsHelpPageBinding = binding;
                }
            }
        }

        [DefaultValue(true)]
        public bool HttpsHelpPageEnabled
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

        [TypeConverter(typeof(UriTypeConverter)), DefaultValue((string) null)]
        public Uri HttpsHelpPageUrl
        {
            get
            {
                return this.httpsHelpPageUrl;
            }
            set
            {
                if (((value != null) && value.IsAbsoluteUri) && (value.Scheme != Uri.UriSchemeHttps))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxServiceMetadataBehaviorUrlMustBeHttpOrRelative", new object[] { "HttpsHelpPageUrl", Uri.UriSchemeHttps, value.ToString(), value.Scheme }));
                }
                this.httpsHelpPageUrl = value;
            }
        }

        [DefaultValue(false)]
        public bool IncludeExceptionDetailInFaults
        {
            get
            {
                return this.includeExceptionDetailInFaults;
            }
            set
            {
                this.includeExceptionDetailInFaults = value;
            }
        }
    }
}

