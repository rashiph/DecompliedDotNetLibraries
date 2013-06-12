namespace System.Net
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    [Serializable]
    public class HttpWebResponse : WebResponse, ISerializable
    {
        internal X509Certificate m_Certificate;
        private string m_CharacterSet;
        private Stream m_ConnectStream;
        private long m_ContentLength;
        private CookieCollection m_cookies;
        private bool m_disposed;
        private WebHeaderCollection m_HttpResponseHeaders;
        private bool m_IsMutuallyAuthenticated;
        private bool m_IsVersionHttp11;
        private string m_MediaType;
        private bool m_propertiesDisposed;
        private HttpStatusCode m_StatusCode;
        private string m_StatusDescription;
        private Uri m_Uri;
        private bool m_UsesProxySemantics;
        private KnownHttpVerb m_Verb;

        [Obsolete("Serialization is obsoleted for this type.  http://go.microsoft.com/fwlink/?linkid=14202")]
        protected HttpWebResponse(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            this.m_HttpResponseHeaders = (WebHeaderCollection) serializationInfo.GetValue("m_HttpResponseHeaders", typeof(WebHeaderCollection));
            this.m_Uri = (Uri) serializationInfo.GetValue("m_Uri", typeof(Uri));
            this.m_Certificate = (X509Certificate) serializationInfo.GetValue("m_Certificate", typeof(X509Certificate));
            this.m_IsVersionHttp11 = ((Version) serializationInfo.GetValue("m_Version", typeof(Version))).Equals(HttpVersion.Version11);
            this.m_StatusCode = (HttpStatusCode) serializationInfo.GetInt32("m_StatusCode");
            this.m_ContentLength = serializationInfo.GetInt64("m_ContentLength");
            this.m_Verb = KnownHttpVerb.Parse(serializationInfo.GetString("m_Verb"));
            this.m_StatusDescription = serializationInfo.GetString("m_StatusDescription");
            this.m_MediaType = serializationInfo.GetString("m_MediaType");
        }

        internal HttpWebResponse(Uri responseUri, KnownHttpVerb verb, CoreResponseData coreData, string mediaType, bool usesProxySemantics, DecompressionMethods decompressionMethod)
        {
            this.m_Uri = responseUri;
            this.m_Verb = verb;
            this.m_MediaType = mediaType;
            this.m_UsesProxySemantics = usesProxySemantics;
            this.m_ConnectStream = coreData.m_ConnectStream;
            this.m_HttpResponseHeaders = coreData.m_ResponseHeaders;
            this.m_ContentLength = coreData.m_ContentLength;
            this.m_StatusCode = coreData.m_StatusCode;
            this.m_StatusDescription = coreData.m_StatusDescription;
            this.m_IsVersionHttp11 = coreData.m_IsVersionHttp11;
            if ((this.m_ContentLength == 0L) && (this.m_ConnectStream is ConnectStream))
            {
                ((ConnectStream) this.m_ConnectStream).CallDone();
            }
            string relativeUri = this.m_HttpResponseHeaders["Content-Location"];
            if (relativeUri != null)
            {
                try
                {
                    this.m_Uri = new Uri(this.m_Uri, relativeUri);
                }
                catch (UriFormatException)
                {
                }
            }
            if (decompressionMethod != DecompressionMethods.None)
            {
                string str2 = this.m_HttpResponseHeaders["Content-Encoding"];
                if (str2 != null)
                {
                    if (((decompressionMethod & DecompressionMethods.GZip) != DecompressionMethods.None) && (str2.IndexOf("gzip") != -1))
                    {
                        this.m_ConnectStream = new GZipWrapperStream(this.m_ConnectStream, CompressionMode.Decompress);
                        this.m_ContentLength = -1L;
                        this.m_HttpResponseHeaders["Content-Encoding"] = null;
                    }
                    else if (((decompressionMethod & DecompressionMethods.Deflate) != DecompressionMethods.None) && (str2.IndexOf("deflate") != -1))
                    {
                        this.m_ConnectStream = new DeflateWrapperStream(this.m_ConnectStream, CompressionMode.Decompress);
                        this.m_ContentLength = -1L;
                        this.m_HttpResponseHeaders["Content-Encoding"] = null;
                    }
                }
            }
        }

        internal void Abort()
        {
            Stream connectStream = this.m_ConnectStream;
            ICloseEx ex = connectStream as ICloseEx;
            try
            {
                if (ex != null)
                {
                    ex.CloseEx(CloseExState.Abort);
                }
                else if (connectStream != null)
                {
                    connectStream.Close();
                }
            }
            catch
            {
            }
        }

        private void CheckDisposed()
        {
            if (this.m_propertiesDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
        }

        public override void Close()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "Close", "");
            }
            if (!this.m_disposed)
            {
                this.m_disposed = true;
                Stream connectStream = this.m_ConnectStream;
                ICloseEx ex = connectStream as ICloseEx;
                if (ex != null)
                {
                    ex.CloseEx(CloseExState.Normal);
                }
                else if (connectStream != null)
                {
                    connectStream.Close();
                }
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "Close", "");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(true);
                this.m_propertiesDisposed = true;
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            serializationInfo.AddValue("m_HttpResponseHeaders", this.m_HttpResponseHeaders, typeof(WebHeaderCollection));
            serializationInfo.AddValue("m_Uri", this.m_Uri, typeof(Uri));
            serializationInfo.AddValue("m_Certificate", this.m_Certificate, typeof(X509Certificate));
            serializationInfo.AddValue("m_Version", this.ProtocolVersion, typeof(Version));
            serializationInfo.AddValue("m_StatusCode", this.m_StatusCode);
            serializationInfo.AddValue("m_ContentLength", this.m_ContentLength);
            serializationInfo.AddValue("m_Verb", this.m_Verb.Name);
            serializationInfo.AddValue("m_StatusDescription", this.m_StatusDescription);
            serializationInfo.AddValue("m_MediaType", this.m_MediaType);
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public string GetResponseHeader(string headerName)
        {
            this.CheckDisposed();
            string str = this.m_HttpResponseHeaders[headerName];
            if (str != null)
            {
                return str;
            }
            return string.Empty;
        }

        public override Stream GetResponseStream()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "GetResponseStream", "");
            }
            this.CheckDisposed();
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, "ContentLength=" + this.m_ContentLength);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "GetResponseStream", this.m_ConnectStream);
            }
            return this.m_ConnectStream;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        public string CharacterSet
        {
            get
            {
                this.CheckDisposed();
                string contentType = this.m_HttpResponseHeaders.ContentType;
                if ((this.m_CharacterSet == null) && !ValidationHelper.IsBlankString(contentType))
                {
                    this.m_CharacterSet = string.Empty;
                    string str2 = contentType.ToLower(CultureInfo.InvariantCulture);
                    if (str2.Trim().StartsWith("text/"))
                    {
                        this.m_CharacterSet = "ISO-8859-1";
                    }
                    int index = str2.IndexOf(";");
                    if (index > 0)
                    {
                        while ((index = str2.IndexOf("charset", index)) >= 0)
                        {
                            index += 7;
                            if ((str2[index - 8] == ';') || (str2[index - 8] == ' '))
                            {
                                while ((index < str2.Length) && (str2[index] == ' '))
                                {
                                    index++;
                                }
                                if ((index < (str2.Length - 1)) && (str2[index] == '='))
                                {
                                    index++;
                                    int num2 = str2.IndexOf(';', index);
                                    if (num2 > index)
                                    {
                                        this.m_CharacterSet = contentType.Substring(index, num2 - index).Trim();
                                    }
                                    else
                                    {
                                        this.m_CharacterSet = contentType.Substring(index).Trim();
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                return this.m_CharacterSet;
            }
        }

        public string ContentEncoding
        {
            get
            {
                this.CheckDisposed();
                string str = this.m_HttpResponseHeaders["Content-Encoding"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
        }

        public override long ContentLength
        {
            get
            {
                this.CheckDisposed();
                return this.m_ContentLength;
            }
        }

        public override string ContentType
        {
            get
            {
                this.CheckDisposed();
                string contentType = this.m_HttpResponseHeaders.ContentType;
                if (contentType != null)
                {
                    return contentType;
                }
                return string.Empty;
            }
        }

        public CookieCollection Cookies
        {
            get
            {
                this.CheckDisposed();
                if (this.m_cookies == null)
                {
                    this.m_cookies = new CookieCollection();
                }
                return this.m_cookies;
            }
            set
            {
                this.CheckDisposed();
                this.m_cookies = value;
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                this.CheckDisposed();
                return this.m_HttpResponseHeaders;
            }
        }

        internal bool InternalSetIsMutuallyAuthenticated
        {
            set
            {
                this.m_IsMutuallyAuthenticated = value;
            }
        }

        public override bool IsMutuallyAuthenticated
        {
            get
            {
                this.CheckDisposed();
                return this.m_IsMutuallyAuthenticated;
            }
        }

        internal bool KeepAlive
        {
            get
            {
                if (this.m_UsesProxySemantics)
                {
                    string str = this.Headers["Proxy-Connection"];
                    if (str != null)
                    {
                        if (str.ToLower(CultureInfo.InvariantCulture).IndexOf("close") >= 0)
                        {
                            return (str.ToLower(CultureInfo.InvariantCulture).IndexOf("keep-alive") >= 0);
                        }
                        return true;
                    }
                }
                string str2 = this.Headers["Connection"];
                if (str2 != null)
                {
                    str2 = str2.ToLower(CultureInfo.InvariantCulture);
                }
                if (this.ProtocolVersion == HttpVersion.Version10)
                {
                    return ((str2 != null) && (str2.IndexOf("keep-alive") >= 0));
                }
                if (this.ProtocolVersion < HttpVersion.Version11)
                {
                    return false;
                }
                if ((str2 != null) && (str2.IndexOf("close") >= 0))
                {
                    return (str2.IndexOf("keep-alive") >= 0);
                }
                return true;
            }
        }

        public DateTime LastModified
        {
            get
            {
                this.CheckDisposed();
                string lastModified = this.m_HttpResponseHeaders.LastModified;
                if (lastModified == null)
                {
                    return DateTime.Now;
                }
                return HttpProtocolUtils.string2date(lastModified);
            }
        }

        public string Method
        {
            get
            {
                this.CheckDisposed();
                return this.m_Verb.Name;
            }
        }

        public Version ProtocolVersion
        {
            get
            {
                this.CheckDisposed();
                if (!this.m_IsVersionHttp11)
                {
                    return HttpVersion.Version10;
                }
                return HttpVersion.Version11;
            }
        }

        internal Stream ResponseStream
        {
            get
            {
                return this.m_ConnectStream;
            }
            set
            {
                this.m_ConnectStream = value;
            }
        }

        public override Uri ResponseUri
        {
            get
            {
                this.CheckDisposed();
                return this.m_Uri;
            }
        }

        public string Server
        {
            get
            {
                this.CheckDisposed();
                string server = this.m_HttpResponseHeaders.Server;
                if (server != null)
                {
                    return server;
                }
                return string.Empty;
            }
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                this.CheckDisposed();
                return this.m_StatusCode;
            }
        }

        public string StatusDescription
        {
            get
            {
                this.CheckDisposed();
                return this.m_StatusDescription;
            }
        }

        public override bool SupportsHeaders
        {
            get
            {
                return true;
            }
        }
    }
}

