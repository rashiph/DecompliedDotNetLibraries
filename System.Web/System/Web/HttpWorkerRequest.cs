namespace System.Web
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Management;
    using System.Web.Util;

    [ComVisible(false)]
    public abstract class HttpWorkerRequest
    {
        private DateTime _startTime = DateTime.UtcNow;
        private Guid _traceId;
        public const int HeaderAccept = 20;
        public const int HeaderAcceptCharset = 0x15;
        public const int HeaderAcceptEncoding = 0x16;
        public const int HeaderAcceptLanguage = 0x17;
        public const int HeaderAcceptRanges = 20;
        public const int HeaderAge = 0x15;
        public const int HeaderAllow = 10;
        public const int HeaderAuthorization = 0x18;
        public const int HeaderCacheControl = 0;
        public const int HeaderConnection = 1;
        public const int HeaderContentEncoding = 13;
        public const int HeaderContentLanguage = 14;
        public const int HeaderContentLength = 11;
        public const int HeaderContentLocation = 15;
        public const int HeaderContentMd5 = 0x10;
        public const int HeaderContentRange = 0x11;
        public const int HeaderContentType = 12;
        public const int HeaderCookie = 0x19;
        public const int HeaderDate = 2;
        public const int HeaderEtag = 0x16;
        public const int HeaderExpect = 0x1a;
        public const int HeaderExpires = 0x12;
        public const int HeaderFrom = 0x1b;
        public const int HeaderHost = 0x1c;
        public const int HeaderIfMatch = 0x1d;
        public const int HeaderIfModifiedSince = 30;
        public const int HeaderIfNoneMatch = 0x1f;
        public const int HeaderIfRange = 0x20;
        public const int HeaderIfUnmodifiedSince = 0x21;
        public const int HeaderKeepAlive = 3;
        public const int HeaderLastModified = 0x13;
        public const int HeaderLocation = 0x17;
        public const int HeaderMaxForwards = 0x22;
        public const int HeaderPragma = 4;
        public const int HeaderProxyAuthenticate = 0x18;
        public const int HeaderProxyAuthorization = 0x23;
        public const int HeaderRange = 0x25;
        public const int HeaderReferer = 0x24;
        public const int HeaderRetryAfter = 0x19;
        public const int HeaderServer = 0x1a;
        public const int HeaderSetCookie = 0x1b;
        public const int HeaderTe = 0x26;
        public const int HeaderTrailer = 5;
        public const int HeaderTransferEncoding = 6;
        public const int HeaderUpgrade = 7;
        public const int HeaderUserAgent = 0x27;
        public const int HeaderVary = 0x1c;
        public const int HeaderVia = 8;
        public const int HeaderWarning = 9;
        public const int HeaderWwwAuthenticate = 0x1d;
        public const int ReasonCachePolicy = 2;
        public const int ReasonCacheSecurity = 3;
        public const int ReasonClientDisconnect = 4;
        public const int ReasonDefault = 0;
        public const int ReasonFileHandleCacheMiss = 1;
        public const int ReasonResponseCacheMiss = 0;
        public const int RequestHeaderMaximum = 40;
        public const int ResponseHeaderMaximum = 30;
        private static readonly string[][] s_HTTPStatusDescriptions;
        private static string[] s_requestHeaderNames;
        private static Hashtable s_requestHeadersLoookupTable;
        private static string[] s_responseHeaderNames;
        private static Hashtable s_responseHeadersLoookupTable;
        private static string[] s_serverVarFromRequestHeaderNames;

        static HttpWorkerRequest()
        {
            string[][] strArray = new string[6][];
            strArray[1] = new string[] { "Continue", "Switching Protocols", "Processing" };
            strArray[2] = new string[] { "OK", "Created", "Accepted", "Non-Authoritative Information", "No Content", "Reset Content", "Partial Content", "Multi-Status" };
            strArray[3] = new string[] { "Multiple Choices", "Moved Permanently", "Found", "See Other", "Not Modified", "Use Proxy", string.Empty, "Temporary Redirect" };
            strArray[4] = new string[] { 
                "Bad Request", "Unauthorized", "Payment Required", "Forbidden", "Not Found", "Method Not Allowed", "Not Acceptable", "Proxy Authentication Required", "Request Timeout", "Conflict", "Gone", "Length Required", "Precondition Failed", "Request Entity Too Large", "Request-Uri Too Long", "Unsupported Media Type", 
                "Requested Range Not Satisfiable", "Expectation Failed", string.Empty, string.Empty, string.Empty, string.Empty, "Unprocessable Entity", "Locked", "Failed Dependency"
             };
            strArray[5] = new string[] { "Internal Server Error", "Not Implemented", "Bad Gateway", "Service Unavailable", "Gateway Timeout", "Http Version Not Supported", string.Empty, "Insufficient Storage" };
            s_HTTPStatusDescriptions = strArray;
            s_serverVarFromRequestHeaderNames = new string[40];
            s_requestHeaderNames = new string[40];
            s_responseHeaderNames = new string[30];
            s_requestHeadersLoookupTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            s_responseHeadersLoookupTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            DefineHeader(true, true, 0, "Cache-Control", "HTTP_CACHE_CONTROL");
            DefineHeader(true, true, 1, "Connection", "HTTP_CONNECTION");
            DefineHeader(true, true, 2, "Date", "HTTP_DATE");
            DefineHeader(true, true, 3, "Keep-Alive", "HTTP_KEEP_ALIVE");
            DefineHeader(true, true, 4, "Pragma", "HTTP_PRAGMA");
            DefineHeader(true, true, 5, "Trailer", "HTTP_TRAILER");
            DefineHeader(true, true, 6, "Transfer-Encoding", "HTTP_TRANSFER_ENCODING");
            DefineHeader(true, true, 7, "Upgrade", "HTTP_UPGRADE");
            DefineHeader(true, true, 8, "Via", "HTTP_VIA");
            DefineHeader(true, true, 9, "Warning", "HTTP_WARNING");
            DefineHeader(true, true, 10, "Allow", "HTTP_ALLOW");
            DefineHeader(true, true, 11, "Content-Length", "HTTP_CONTENT_LENGTH");
            DefineHeader(true, true, 12, "Content-Type", "HTTP_CONTENT_TYPE");
            DefineHeader(true, true, 13, "Content-Encoding", "HTTP_CONTENT_ENCODING");
            DefineHeader(true, true, 14, "Content-Language", "HTTP_CONTENT_LANGUAGE");
            DefineHeader(true, true, 15, "Content-Location", "HTTP_CONTENT_LOCATION");
            DefineHeader(true, true, 0x10, "Content-MD5", "HTTP_CONTENT_MD5");
            DefineHeader(true, true, 0x11, "Content-Range", "HTTP_CONTENT_RANGE");
            DefineHeader(true, true, 0x12, "Expires", "HTTP_EXPIRES");
            DefineHeader(true, true, 0x13, "Last-Modified", "HTTP_LAST_MODIFIED");
            DefineHeader(true, false, 20, "Accept", "HTTP_ACCEPT");
            DefineHeader(true, false, 0x15, "Accept-Charset", "HTTP_ACCEPT_CHARSET");
            DefineHeader(true, false, 0x16, "Accept-Encoding", "HTTP_ACCEPT_ENCODING");
            DefineHeader(true, false, 0x17, "Accept-Language", "HTTP_ACCEPT_LANGUAGE");
            DefineHeader(true, false, 0x18, "Authorization", "HTTP_AUTHORIZATION");
            DefineHeader(true, false, 0x19, "Cookie", "HTTP_COOKIE");
            DefineHeader(true, false, 0x1a, "Expect", "HTTP_EXPECT");
            DefineHeader(true, false, 0x1b, "From", "HTTP_FROM");
            DefineHeader(true, false, 0x1c, "Host", "HTTP_HOST");
            DefineHeader(true, false, 0x1d, "If-Match", "HTTP_IF_MATCH");
            DefineHeader(true, false, 30, "If-Modified-Since", "HTTP_IF_MODIFIED_SINCE");
            DefineHeader(true, false, 0x1f, "If-None-Match", "HTTP_IF_NONE_MATCH");
            DefineHeader(true, false, 0x20, "If-Range", "HTTP_IF_RANGE");
            DefineHeader(true, false, 0x21, "If-Unmodified-Since", "HTTP_IF_UNMODIFIED_SINCE");
            DefineHeader(true, false, 0x22, "Max-Forwards", "HTTP_MAX_FORWARDS");
            DefineHeader(true, false, 0x23, "Proxy-Authorization", "HTTP_PROXY_AUTHORIZATION");
            DefineHeader(true, false, 0x24, "Referer", "HTTP_REFERER");
            DefineHeader(true, false, 0x25, "Range", "HTTP_RANGE");
            DefineHeader(true, false, 0x26, "TE", "HTTP_TE");
            DefineHeader(true, false, 0x27, "User-Agent", "HTTP_USER_AGENT");
            DefineHeader(false, true, 20, "Accept-Ranges", null);
            DefineHeader(false, true, 0x15, "Age", null);
            DefineHeader(false, true, 0x16, "ETag", null);
            DefineHeader(false, true, 0x17, "Location", null);
            DefineHeader(false, true, 0x18, "Proxy-Authenticate", null);
            DefineHeader(false, true, 0x19, "Retry-After", null);
            DefineHeader(false, true, 0x1a, "Server", null);
            DefineHeader(false, true, 0x1b, "Set-Cookie", null);
            DefineHeader(false, true, 0x1c, "Vary", null);
            DefineHeader(false, true, 0x1d, "WWW-Authenticate", null);
        }

        protected HttpWorkerRequest()
        {
        }

        internal virtual IAsyncResult BeginExecuteUrl(string url, string method, string headers, bool sendHeaders, bool addUserIndo, IntPtr token, string name, string authType, byte[] entity, AsyncCallback cb, object state)
        {
            throw new NotSupportedException(System.Web.SR.GetString("ExecuteUrl_not_supported"));
        }

        public virtual void CloseConnection()
        {
        }

        private static void DefineHeader(bool isRequest, bool isResponse, int index, string headerName, string serverVarName)
        {
            int num = 0;
            if (isRequest)
            {
                num = index;
                s_serverVarFromRequestHeaderNames[index] = serverVarName;
                s_requestHeaderNames[index] = headerName;
                s_requestHeadersLoookupTable.Add(headerName, num);
            }
            if (isResponse)
            {
                num = index;
                s_responseHeaderNames[index] = headerName;
                s_responseHeadersLoookupTable.Add(headerName, num);
            }
        }

        internal virtual void DisableKernelCache()
        {
        }

        internal virtual void EndExecuteUrl(IAsyncResult result)
        {
        }

        public abstract void EndOfRequest();
        public abstract void FlushResponse(bool finalFlush);
        public virtual string GetAppPath()
        {
            return null;
        }

        public virtual string GetAppPathTranslated()
        {
            return null;
        }

        public virtual string GetAppPoolID()
        {
            return null;
        }

        public virtual long GetBytesRead()
        {
            return 0L;
        }

        public virtual byte[] GetClientCertificate()
        {
            return new byte[0];
        }

        public virtual byte[] GetClientCertificateBinaryIssuer()
        {
            return new byte[0];
        }

        public virtual int GetClientCertificateEncoding()
        {
            return 0;
        }

        public virtual byte[] GetClientCertificatePublicKey()
        {
            return new byte[0];
        }

        public virtual DateTime GetClientCertificateValidFrom()
        {
            return DateTime.Now;
        }

        public virtual DateTime GetClientCertificateValidUntil()
        {
            return DateTime.Now;
        }

        public virtual long GetConnectionID()
        {
            return 0L;
        }

        public virtual string GetFilePath()
        {
            return this.GetUriPath();
        }

        internal VirtualPath GetFilePathObject()
        {
            return VirtualPath.Create(this.GetFilePath(), VirtualPathOptions.AllowAbsolutePath | VirtualPathOptions.AllowNull);
        }

        public virtual string GetFilePathTranslated()
        {
            return null;
        }

        public abstract string GetHttpVerbName();
        public abstract string GetHttpVersion();
        public virtual string GetKnownRequestHeader(int index)
        {
            return null;
        }

        public static int GetKnownRequestHeaderIndex(string header)
        {
            object obj2 = s_requestHeadersLoookupTable[header];
            if (obj2 != null)
            {
                return (int) obj2;
            }
            return -1;
        }

        public static string GetKnownRequestHeaderName(int index)
        {
            return s_requestHeaderNames[index];
        }

        public static int GetKnownResponseHeaderIndex(string header)
        {
            object obj2 = s_responseHeadersLoookupTable[header];
            if (obj2 != null)
            {
                return (int) obj2;
            }
            return -1;
        }

        public static string GetKnownResponseHeaderName(int index)
        {
            return s_responseHeaderNames[index];
        }

        public abstract string GetLocalAddress();
        public abstract int GetLocalPort();
        internal virtual string GetLocalPortAsString()
        {
            return this.GetLocalPort().ToString(NumberFormatInfo.InvariantInfo);
        }

        public virtual string GetPathInfo()
        {
            return string.Empty;
        }

        public virtual byte[] GetPreloadedEntityBody()
        {
            return null;
        }

        public virtual int GetPreloadedEntityBody(byte[] buffer, int offset)
        {
            int count = 0;
            byte[] preloadedEntityBody = this.GetPreloadedEntityBody();
            if (preloadedEntityBody != null)
            {
                count = preloadedEntityBody.Length;
                Buffer.BlockCopy(preloadedEntityBody, 0, buffer, offset, count);
            }
            return count;
        }

        public virtual int GetPreloadedEntityBodyLength()
        {
            byte[] preloadedEntityBody = this.GetPreloadedEntityBody();
            if (preloadedEntityBody == null)
            {
                return 0;
            }
            return preloadedEntityBody.Length;
        }

        public virtual string GetProtocol()
        {
            if (!this.IsSecure())
            {
                return "http";
            }
            return "https";
        }

        public abstract string GetQueryString();
        public virtual byte[] GetQueryStringRawBytes()
        {
            return null;
        }

        public abstract string GetRawUrl();
        internal static string GetRawUrlHelper(string cacheUrl)
        {
            if (cacheUrl != null)
            {
                int num = 0;
                for (int i = 0; i < cacheUrl.Length; i++)
                {
                    if ((cacheUrl[i] == '/') && (++num == 3))
                    {
                        return cacheUrl.Substring(i);
                    }
                }
            }
            throw new HttpException(System.Web.SR.GetString("Cache_url_invalid"));
        }

        public abstract string GetRemoteAddress();
        public virtual string GetRemoteName()
        {
            return this.GetRemoteAddress();
        }

        public abstract int GetRemotePort();
        public virtual int GetRequestReason()
        {
            return 0;
        }

        public virtual string GetServerName()
        {
            return this.GetLocalAddress();
        }

        public virtual string GetServerVariable(string name)
        {
            return null;
        }

        internal static string GetServerVariableNameFromKnownRequestHeaderIndex(int index)
        {
            return s_serverVarFromRequestHeaderNames[index];
        }

        internal virtual DateTime GetStartTime()
        {
            return this._startTime;
        }

        public static string GetStatusDescription(int code)
        {
            if ((code >= 100) && (code < 600))
            {
                int index = code / 100;
                int num2 = code % 100;
                if (num2 < s_HTTPStatusDescriptions[index].Length)
                {
                    return s_HTTPStatusDescriptions[index][num2];
                }
            }
            return string.Empty;
        }

        public virtual int GetTotalEntityBodyLength()
        {
            int num = 0;
            string knownRequestHeader = this.GetKnownRequestHeader(11);
            if (knownRequestHeader != null)
            {
                try
                {
                    num = int.Parse(knownRequestHeader, CultureInfo.InvariantCulture);
                }
                catch
                {
                }
            }
            return num;
        }

        public virtual string GetUnknownRequestHeader(string name)
        {
            return null;
        }

        [CLSCompliant(false)]
        public virtual string[][] GetUnknownRequestHeaders()
        {
            return null;
        }

        public abstract string GetUriPath();
        public virtual long GetUrlContextID()
        {
            return 0L;
        }

        public virtual IntPtr GetUserToken()
        {
            return IntPtr.Zero;
        }

        public virtual IntPtr GetVirtualPathToken()
        {
            return IntPtr.Zero;
        }

        public bool HasEntityBody()
        {
            string knownRequestHeader = this.GetKnownRequestHeader(11);
            return (((knownRequestHeader != null) && !knownRequestHeader.Equals("0")) || ((this.GetKnownRequestHeader(6) != null) || ((this.GetPreloadedEntityBody() != null) || (this.IsEntireEntityBodyIsPreloaded() && false))));
        }

        public virtual bool HeadersSent()
        {
            return true;
        }

        public virtual bool IsClientConnected()
        {
            return true;
        }

        public virtual bool IsEntireEntityBodyIsPreloaded()
        {
            return false;
        }

        public virtual bool IsSecure()
        {
            return false;
        }

        public virtual string MapPath(string virtualPath)
        {
            return null;
        }

        internal virtual void RaiseTraceEvent(WebBaseEvent webEvent)
        {
        }

        internal virtual void RaiseTraceEvent(IntegratedTraceType traceType, string eventData)
        {
        }

        public virtual int ReadEntityBody(byte[] buffer, int size)
        {
            return 0;
        }

        public virtual int ReadEntityBody(byte[] buffer, int offset, int size)
        {
            byte[] buffer2 = new byte[size];
            int count = this.ReadEntityBody(buffer2, size);
            if (count > 0)
            {
                Buffer.BlockCopy(buffer2, 0, buffer, offset, count);
            }
            return count;
        }

        internal virtual void ResetStartTime()
        {
            this._startTime = DateTime.UtcNow;
        }

        public virtual void SendCalculatedContentLength(int contentLength)
        {
        }

        public virtual void SendCalculatedContentLength(long contentLength)
        {
            this.SendCalculatedContentLength(Convert.ToInt32(contentLength));
        }

        public abstract void SendKnownResponseHeader(int index, string value);
        public abstract void SendResponseFromFile(IntPtr handle, long offset, long length);
        public abstract void SendResponseFromFile(string filename, long offset, long length);
        public abstract void SendResponseFromMemory(byte[] data, int length);
        public virtual void SendResponseFromMemory(IntPtr data, int length)
        {
            if (length > 0)
            {
                InternalSecurityPermissions.UnmanagedCode.Demand();
                byte[] dest = new byte[length];
                Misc.CopyMemory(data, 0, dest, 0, length);
                this.SendResponseFromMemory(dest, length);
            }
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        internal virtual void SendResponseFromMemory(IntPtr data, int length, bool isBufferFromUnmanagedPool)
        {
            this.SendResponseFromMemory(data, length);
        }

        public abstract void SendStatus(int statusCode, string statusDescription);
        internal virtual void SendStatus(int statusCode, int subStatusCode, string statusDescription)
        {
            this.SendStatus(statusCode, statusDescription);
        }

        public abstract void SendUnknownResponseHeader(string name, string value);
        public virtual void SetEndOfSendNotification(EndOfSendNotification callback, object extraData)
        {
        }

        internal virtual void SetHeaderEncoding(Encoding encoding)
        {
        }

        internal virtual string SetupKernelCaching(int secondsToLive, string originalCacheUrl, bool enableKernelCacheForVaryByStar)
        {
            return null;
        }

        internal virtual void TransmitFile(string filename, long length, bool isImpersonating)
        {
            this.TransmitFile(filename, 0L, length, isImpersonating);
        }

        internal virtual void TransmitFile(string filename, long offset, long length, bool isImpersonating)
        {
            this.SendResponseFromFile(filename, offset, length);
        }

        internal virtual void UpdateInitialCounters()
        {
        }

        internal virtual void UpdateRequestCounters(int bytesIn)
        {
        }

        internal virtual void UpdateResponseCounters(bool finalFlush, int bytesOut)
        {
        }

        public virtual string MachineConfigPath
        {
            get
            {
                return null;
            }
        }

        public virtual string MachineInstallDirectory
        {
            get
            {
                return null;
            }
        }

        public virtual Guid RequestTraceIdentifier
        {
            get
            {
                return this._traceId;
            }
        }

        public virtual string RootWebConfigPath
        {
            get
            {
                return null;
            }
        }

        internal virtual bool SupportsExecuteUrl
        {
            get
            {
                return false;
            }
        }

        internal virtual bool SupportsLongTransmitFile
        {
            get
            {
                return false;
            }
        }

        internal virtual bool TrySkipIisCustomErrors
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public delegate void EndOfSendNotification(HttpWorkerRequest wr, object extraData);
    }
}

