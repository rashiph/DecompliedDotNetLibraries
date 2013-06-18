namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.ServiceModel;

    public sealed class HttpRequestMessageProperty
    {
        private WebHeaderCollection headers;
        private IHttpHeaderProvider httpHeaderProvider;
        private string method;
        private string queryString;
        private bool suppressEntityBody;

        public HttpRequestMessageProperty()
        {
            this.method = "POST";
            this.queryString = string.Empty;
            this.suppressEntityBody = false;
        }

        internal HttpRequestMessageProperty(IHttpHeaderProvider httpHeaderProvider) : this()
        {
            this.httpHeaderProvider = httpHeaderProvider;
        }

        public WebHeaderCollection Headers
        {
            get
            {
                if (this.headers == null)
                {
                    this.headers = new WebHeaderCollection();
                    if (this.httpHeaderProvider != null)
                    {
                        this.httpHeaderProvider.CopyHeaders(this.headers);
                        this.httpHeaderProvider = null;
                    }
                }
                return this.headers;
            }
        }

        public string Method
        {
            get
            {
                return this.method;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.method = value;
            }
        }

        public static string Name
        {
            get
            {
                return "httpRequest";
            }
        }

        public string QueryString
        {
            get
            {
                return this.queryString;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.queryString = value;
            }
        }

        public bool SuppressEntityBody
        {
            get
            {
                return this.suppressEntityBody;
            }
            set
            {
                this.suppressEntityBody = value;
            }
        }

        internal interface IHttpHeaderProvider
        {
            void CopyHeaders(WebHeaderCollection headers);
        }
    }
}

