namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Net;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Web.Services;

    [ComVisible(true)]
    public abstract class HttpWebClientProtocol : WebClientProtocol
    {
        private bool allowAutoRedirect;
        private X509CertificateCollection clientCertificates;
        private System.Net.CookieContainer cookieJar;
        private bool enableDecompression;
        private IWebProxy proxy;
        private bool unsafeAuthenticatedConnectionSharing;
        private string userAgent;
        private static string UserAgentDefault = ("Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol " + Environment.Version.ToString() + ")");

        protected HttpWebClientProtocol()
        {
            this.allowAutoRedirect = false;
            this.userAgent = UserAgentDefault;
        }

        internal HttpWebClientProtocol(HttpWebClientProtocol protocol) : base(protocol)
        {
            this.allowAutoRedirect = protocol.allowAutoRedirect;
            this.enableDecompression = protocol.enableDecompression;
            this.cookieJar = protocol.cookieJar;
            this.clientCertificates = protocol.clientCertificates;
            this.proxy = protocol.proxy;
            this.userAgent = protocol.userAgent;
        }

        protected void CancelAsync(object userState)
        {
            if (userState == null)
            {
                userState = base.NullToken;
            }
            object[] parameters = new object[1];
            WebClientAsyncResult result = this.OperationCompleted(userState, parameters, null, true);
            if (result != null)
            {
                result.Abort();
            }
        }

        public static bool GenerateXmlMappings(Type type, ArrayList mappings)
        {
            if (!typeof(SoapHttpClientProtocol).IsAssignableFrom(type))
            {
                return false;
            }
            WebServiceBindingAttribute attribute = WebServiceBindingReflector.GetAttribute(type);
            if (attribute == null)
            {
                throw new InvalidOperationException(Res.GetString("WebClientBindingAttributeRequired"));
            }
            string serviceNamespace = attribute.Namespace;
            bool serviceDefaultIsEncoded = SoapReflector.ServiceDefaultIsEncoded(type);
            ArrayList soapMethodList = new ArrayList();
            SoapClientType.GenerateXmlMappings(type, soapMethodList, serviceNamespace, serviceDefaultIsEncoded, mappings);
            return true;
        }

        public static Hashtable GenerateXmlMappings(Type[] types, ArrayList mappings)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            Hashtable hashtable = new Hashtable();
            foreach (Type type in types)
            {
                ArrayList list = new ArrayList();
                if (GenerateXmlMappings(type, mappings))
                {
                    hashtable.Add(type, list);
                    mappings.Add(list);
                }
            }
            return hashtable;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest webRequest = base.GetWebRequest(uri);
            HttpWebRequest request2 = webRequest as HttpWebRequest;
            if (request2 != null)
            {
                request2.UserAgent = this.UserAgent;
                request2.AllowAutoRedirect = this.allowAutoRedirect;
                request2.AutomaticDecompression = this.enableDecompression ? DecompressionMethods.GZip : DecompressionMethods.None;
                request2.AllowWriteStreamBuffering = true;
                request2.SendChunked = false;
                if (this.unsafeAuthenticatedConnectionSharing != request2.UnsafeAuthenticatedConnectionSharing)
                {
                    request2.UnsafeAuthenticatedConnectionSharing = this.unsafeAuthenticatedConnectionSharing;
                }
                if (this.proxy != null)
                {
                    request2.Proxy = this.proxy;
                }
                if ((this.clientCertificates != null) && (this.clientCertificates.Count > 0))
                {
                    request2.ClientCertificates.AddRange(this.clientCertificates);
                }
                request2.CookieContainer = this.cookieJar;
            }
            return webRequest;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            return base.GetWebResponse(request);
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            return base.GetWebResponse(request, result);
        }

        internal WebClientAsyncResult OperationCompleted(object userState, object[] parameters, Exception e, bool canceled)
        {
            WebClientAsyncResult result = (WebClientAsyncResult) base.AsyncInvokes[userState];
            if (result != null)
            {
                AsyncOperation asyncState = (AsyncOperation) result.AsyncState;
                UserToken userSuppliedState = (UserToken) asyncState.UserSuppliedState;
                InvokeCompletedEventArgs arg = new InvokeCompletedEventArgs(parameters, e, canceled, userState);
                base.AsyncInvokes.Remove(userState);
                asyncState.PostOperationCompleted(userSuppliedState.Callback, arg);
            }
            return result;
        }

        [DefaultValue(false), WebServicesDescription("ClientProtocolAllowAutoRedirect")]
        public bool AllowAutoRedirect
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.allowAutoRedirect;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.allowAutoRedirect = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebServicesDescription("ClientProtocolClientCertificates"), Browsable(false)]
        public X509CertificateCollection ClientCertificates
        {
            get
            {
                if (this.clientCertificates == null)
                {
                    this.clientCertificates = new X509CertificateCollection();
                }
                return this.clientCertificates;
            }
        }

        [DefaultValue((string) null), WebServicesDescription("ClientProtocolCookieContainer")]
        public System.Net.CookieContainer CookieContainer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cookieJar;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.cookieJar = value;
            }
        }

        [DefaultValue(false), WebServicesDescription("ClientProtocolEnableDecompression")]
        public bool EnableDecompression
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enableDecompression;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.enableDecompression = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IWebProxy Proxy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.proxy;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.proxy = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UnsafeAuthenticatedConnectionSharing
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.unsafeAuthenticatedConnectionSharing;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.unsafeAuthenticatedConnectionSharing = value;
            }
        }

        [Browsable(false), WebServicesDescription("ClientProtocolUserAgent"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string UserAgent
        {
            get
            {
                if (this.userAgent != null)
                {
                    return this.userAgent;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.userAgent = value;
            }
        }
    }
}

