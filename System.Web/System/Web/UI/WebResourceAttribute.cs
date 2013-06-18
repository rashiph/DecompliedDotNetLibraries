namespace System.Web.UI
{
    using System;
    using System.Web.Util;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class WebResourceAttribute : Attribute
    {
        private string _cdnPath;
        private string _cdnPathSecureConnection;
        private bool _cdnSupportsSecureConnection;
        private string _contentType;
        internal const string _microsoftCdnBasePath = "http://ajax.microsoft.com/ajax/4.0/2/";
        private bool _performSubstitution;
        private string _webResource;

        public WebResourceAttribute(string webResource, string contentType)
        {
            if (string.IsNullOrEmpty(webResource))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("webResource");
            }
            if (string.IsNullOrEmpty(contentType))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("contentType");
            }
            this._contentType = contentType;
            this._webResource = webResource;
            this._performSubstitution = false;
        }

        public string CdnPath
        {
            get
            {
                return (this._cdnPath ?? string.Empty);
            }
            set
            {
                this._cdnPath = value;
            }
        }

        internal string CdnPathSecureConnection
        {
            get
            {
                if (this._cdnPathSecureConnection == null)
                {
                    string cdnPath = this.CdnPath;
                    if ((string.IsNullOrEmpty(cdnPath) || !this.CdnSupportsSecureConnection) || !cdnPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    {
                        cdnPath = string.Empty;
                    }
                    else
                    {
                        cdnPath = "https" + cdnPath.Substring(4);
                    }
                    this._cdnPathSecureConnection = cdnPath;
                }
                return this._cdnPathSecureConnection;
            }
        }

        public bool CdnSupportsSecureConnection
        {
            get
            {
                return this._cdnSupportsSecureConnection;
            }
            set
            {
                this._cdnSupportsSecureConnection = value;
            }
        }

        public string ContentType
        {
            get
            {
                return this._contentType;
            }
        }

        public bool PerformSubstitution
        {
            get
            {
                return this._performSubstitution;
            }
            set
            {
                this._performSubstitution = value;
            }
        }

        public string WebResource
        {
            get
            {
                return this._webResource;
            }
        }
    }
}

