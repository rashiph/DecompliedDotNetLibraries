namespace System.Net
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public sealed class HttpListenerResponse : IDisposable
    {
        private System.Net.BoundaryType m_BoundaryType;
        private Encoding m_ContentEncoding;
        private long m_ContentLength;
        private CookieCollection m_Cookies;
        private System.Net.HttpListenerContext m_HttpContext;
        private bool m_KeepAlive;
        private UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE m_NativeResponse;
        private ResponseState m_ResponseState;
        private HttpResponseStream m_ResponseStream;
        private string m_StatusDescription;
        private WebHeaderCollection m_WebHeaders;
        private static readonly string[][] s_HTTPStatusDescriptions;
        private static readonly int[] s_NoResponseBody;

        static HttpListenerResponse()
        {
            string[][] strArray = new string[6][];
            strArray[1] = new string[] { "Continue", "Switching Protocols", "Processing" };
            strArray[2] = new string[] { "OK", "Created", "Accepted", "Non-Authoritative Information", "No Content", "Reset Content", "Partial Content", "Multi-Status" };
            string[] strArray4 = new string[8];
            strArray4[0] = "Multiple Choices";
            strArray4[1] = "Moved Permanently";
            strArray4[2] = "Found";
            strArray4[3] = "See Other";
            strArray4[4] = "Not Modified";
            strArray4[5] = "Use Proxy";
            strArray4[7] = "Temporary Redirect";
            strArray[3] = strArray4;
            string[] strArray5 = new string[0x19];
            strArray5[0] = "Bad Request";
            strArray5[1] = "Unauthorized";
            strArray5[2] = "Payment Required";
            strArray5[3] = "Forbidden";
            strArray5[4] = "Not Found";
            strArray5[5] = "Method Not Allowed";
            strArray5[6] = "Not Acceptable";
            strArray5[7] = "Proxy Authentication Required";
            strArray5[8] = "Request Timeout";
            strArray5[9] = "Conflict";
            strArray5[10] = "Gone";
            strArray5[11] = "Length Required";
            strArray5[12] = "Precondition Failed";
            strArray5[13] = "Request Entity Too Large";
            strArray5[14] = "Request-Uri Too Long";
            strArray5[15] = "Unsupported Media Type";
            strArray5[0x10] = "Requested Range Not Satisfiable";
            strArray5[0x11] = "Expectation Failed";
            strArray5[0x16] = "Unprocessable Entity";
            strArray5[0x17] = "Locked";
            strArray5[0x18] = "Failed Dependency";
            strArray[4] = strArray5;
            string[] strArray6 = new string[8];
            strArray6[0] = "Internal Server Error";
            strArray6[1] = "Not Implemented";
            strArray6[2] = "Bad Gateway";
            strArray6[3] = "Service Unavailable";
            strArray6[4] = "Gateway Timeout";
            strArray6[5] = "Http Version Not Supported";
            strArray6[7] = "Insufficient Storage";
            strArray[5] = strArray6;
            s_HTTPStatusDescriptions = strArray;
            s_NoResponseBody = new int[] { 100, 0x65, 0xcc, 0xcd, 0x130 };
        }

        internal HttpListenerResponse()
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.HttpListener, this, ".ctor", "");
            }
            this.m_NativeResponse = new UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE();
            this.m_WebHeaders = new WebHeaderCollection(WebHeaderCollectionType.HttpListenerResponse);
            this.m_BoundaryType = System.Net.BoundaryType.None;
            this.m_NativeResponse.StatusCode = 200;
            this.m_NativeResponse.Version.MajorVersion = 1;
            this.m_NativeResponse.Version.MinorVersion = 1;
            this.m_KeepAlive = true;
            this.m_ResponseState = ResponseState.Created;
        }

        internal HttpListenerResponse(System.Net.HttpListenerContext httpContext) : this()
        {
            if (Logging.On)
            {
                Logging.Associate(Logging.HttpListener, this, httpContext);
            }
            this.m_HttpContext = httpContext;
        }

        public void Abort()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.HttpListener, this, "abort", "");
            }
            try
            {
                if (this.m_ResponseState < ResponseState.Closed)
                {
                    this.m_ResponseState = ResponseState.Closed;
                    this.HttpListenerContext.Abort();
                }
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.HttpListener, this, "abort", "");
                }
            }
        }

        public void AddHeader(string name, string value)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.HttpListener, this, "AddHeader", " name=" + name + " value=" + value);
            }
            this.Headers.SetInternal(name, value);
        }

        public void AppendCookie(Cookie cookie)
        {
            if (cookie == null)
            {
                throw new ArgumentNullException("cookie");
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.HttpListener, this, "AppendCookie", " cookie#" + ValidationHelper.HashString(cookie));
            }
            this.Cookies.Add(cookie);
        }

        public void AppendHeader(string name, string value)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.HttpListener, this, "AppendHeader", " name=" + name + " value=" + value);
            }
            this.Headers.Add(name, value);
        }

        private bool CanSendResponseBody(int responseCode)
        {
            for (int i = 0; i < s_NoResponseBody.Length; i++)
            {
                if (responseCode == s_NoResponseBody[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void CheckDisposed()
        {
            if (this.m_ResponseState >= ResponseState.Closed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
        }

        public void Close()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.HttpListener, this, "Close", "");
            }
            try
            {
                ((IDisposable) this).Dispose();
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.HttpListener, this, "Close", "");
                }
            }
        }

        public void Close(byte[] responseEntity, bool willBlock)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.HttpListener, this, "Close", string.Concat(new object[] { " responseEntity=", ValidationHelper.HashString(responseEntity), " willBlock=", willBlock }));
            }
            try
            {
                this.CheckDisposed();
                if (responseEntity == null)
                {
                    throw new ArgumentNullException("responseEntity");
                }
                if ((this.m_ResponseState < ResponseState.SentHeaders) && (this.m_BoundaryType != System.Net.BoundaryType.Chunked))
                {
                    this.ContentLength64 = responseEntity.Length;
                }
                this.EnsureResponseStream();
                if (willBlock)
                {
                    try
                    {
                        try
                        {
                            this.m_ResponseStream.Write(responseEntity, 0, responseEntity.Length);
                        }
                        catch (Win32Exception)
                        {
                        }
                        return;
                    }
                    finally
                    {
                        this.m_ResponseStream.Close();
                        this.m_ResponseState = ResponseState.Closed;
                        this.HttpListenerContext.Close();
                    }
                }
                this.m_ResponseStream.BeginWrite(responseEntity, 0, responseEntity.Length, new AsyncCallback(this.NonBlockingCloseCallback), null);
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.HttpListener, this, "Close", "");
                }
            }
        }

        internal void ComputeCookies()
        {
            if (this.m_Cookies != null)
            {
                string str = null;
                string str2 = null;
                for (int i = 0; i < this.m_Cookies.Count; i++)
                {
                    Cookie cookie = this.m_Cookies[i];
                    string str3 = cookie.ToServerString();
                    if ((str3 != null) && (str3.Length != 0))
                    {
                        if ((cookie.Variant == CookieVariant.Rfc2965) || (this.HttpListenerContext.PromoteCookiesToRfc2965 && (cookie.Variant == CookieVariant.Rfc2109)))
                        {
                            str = (str == null) ? str3 : (str + ", " + str3);
                        }
                        else
                        {
                            str2 = (str2 == null) ? str3 : (str2 + ", " + str3);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(str2))
                {
                    this.Headers.Set(HttpResponseHeader.SetCookie, str2);
                    if (string.IsNullOrEmpty(str))
                    {
                        this.Headers.Remove("Set-Cookie2");
                    }
                }
                if (!string.IsNullOrEmpty(str))
                {
                    this.Headers.Set("Set-Cookie2", str);
                    if (string.IsNullOrEmpty(str2))
                    {
                        this.Headers.Remove("Set-Cookie");
                    }
                }
            }
        }

        internal UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS ComputeHeaders()
        {
            UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS nONE = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE;
            this.m_ResponseState = ResponseState.ComputedHeaders;
            if ((this.HttpListenerContext.MutualAuthentication != null) && (this.HttpListenerContext.MutualAuthentication.Length > 0))
            {
                this.Headers.SetInternal(HttpResponseHeader.WwwAuthenticate, this.HttpListenerContext.MutualAuthentication);
            }
            this.ComputeCookies();
            if (this.m_BoundaryType == System.Net.BoundaryType.None)
            {
                if (this.HttpListenerRequest.ProtocolVersion.Minor == 0)
                {
                    this.m_KeepAlive = false;
                }
                else
                {
                    this.m_BoundaryType = System.Net.BoundaryType.Chunked;
                }
                if (this.CanSendResponseBody(this.m_HttpContext.Response.StatusCode))
                {
                    this.m_ContentLength = -1L;
                }
                else
                {
                    this.ContentLength64 = 0L;
                }
            }
            if (this.m_BoundaryType == System.Net.BoundaryType.ContentLength)
            {
                this.Headers.SetInternal(HttpResponseHeader.ContentLength, this.m_ContentLength.ToString("D", NumberFormatInfo.InvariantInfo));
                if (this.m_ContentLength == 0L)
                {
                    nONE = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE;
                }
            }
            else if (this.m_BoundaryType == System.Net.BoundaryType.Chunked)
            {
                this.Headers.SetInternal(HttpResponseHeader.TransferEncoding, "chunked");
            }
            else if (this.m_BoundaryType == System.Net.BoundaryType.None)
            {
                nONE = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE;
            }
            else
            {
                this.m_KeepAlive = false;
            }
            if (!this.m_KeepAlive)
            {
                this.Headers.Add(HttpResponseHeader.Connection, "close");
                if (nONE == UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE)
                {
                    nONE = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_INITIALIZE_SERVER;
                }
                return nONE;
            }
            if (this.HttpListenerRequest.ProtocolVersion.Minor == 0)
            {
                this.Headers.SetInternal(HttpResponseHeader.KeepAlive, "true");
            }
            return nONE;
        }

        public void CopyFrom(HttpListenerResponse templateResponse)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.HttpListener, this, "CopyFrom", "templateResponse#" + ValidationHelper.HashString(templateResponse));
            }
            this.m_NativeResponse = new UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE();
            this.m_ResponseState = ResponseState.Created;
            this.m_WebHeaders = templateResponse.m_WebHeaders;
            this.m_BoundaryType = templateResponse.m_BoundaryType;
            this.m_ContentLength = templateResponse.m_ContentLength;
            this.m_NativeResponse.StatusCode = templateResponse.m_NativeResponse.StatusCode;
            this.m_NativeResponse.Version.MajorVersion = templateResponse.m_NativeResponse.Version.MajorVersion;
            this.m_NativeResponse.Version.MinorVersion = templateResponse.m_NativeResponse.Version.MinorVersion;
            this.m_StatusDescription = templateResponse.m_StatusDescription;
            this.m_KeepAlive = templateResponse.m_KeepAlive;
        }

        private void Dispose(bool disposing)
        {
            if (this.m_ResponseState < ResponseState.Closed)
            {
                this.EnsureResponseStream();
                this.m_ResponseStream.Close();
                this.m_ResponseState = ResponseState.Closed;
                this.HttpListenerContext.Close();
            }
        }

        private void EnsureResponseStream()
        {
            if (this.m_ResponseStream == null)
            {
                this.m_ResponseStream = new HttpResponseStream(this.HttpListenerContext);
            }
        }

        private void FreePinnedHeaders(List<GCHandle> pinnedHeaders)
        {
            if (pinnedHeaders != null)
            {
                foreach (GCHandle handle in pinnedHeaders)
                {
                    if (handle.IsAllocated)
                    {
                        handle.Free();
                    }
                }
            }
        }

        internal static string GetStatusDescription(int code)
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
            return null;
        }

        private void NonBlockingCloseCallback(IAsyncResult asyncResult)
        {
            try
            {
                this.m_ResponseStream.EndWrite(asyncResult);
            }
            catch (Win32Exception)
            {
            }
            finally
            {
                this.m_ResponseStream.Close();
                this.HttpListenerContext.Close();
                this.m_ResponseState = ResponseState.Closed;
            }
        }

        public void Redirect(string url)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.HttpListener, this, "Redirect", " url=" + url);
            }
            this.Headers.SetInternal(HttpResponseHeader.Location, url);
            this.StatusCode = 0x12e;
            this.StatusDescription = GetStatusDescription(this.StatusCode);
        }

        internal unsafe uint SendHeaders(UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK* pDataChunk, HttpResponseStreamAsyncResult asyncResult, UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS flags)
        {
            uint num2;
            if (Logging.On)
            {
                StringBuilder builder = new StringBuilder("HttpListenerResponse Headers:\n");
                for (int i = 0; i < this.Headers.Count; i++)
                {
                    builder.Append("\t");
                    builder.Append(this.Headers.GetKey(i));
                    builder.Append(" : ");
                    builder.Append(this.Headers.Get(i));
                    builder.Append("\n");
                }
                Logging.PrintInfo(Logging.HttpListener, this, ".ctor", builder.ToString());
            }
            this.m_ResponseState = ResponseState.SentHeaders;
            List<GCHandle> pinnedHeaders = this.SerializeHeaders(ref this.m_NativeResponse.Headers);
            try
            {
                if (pDataChunk != null)
                {
                    this.m_NativeResponse.EntityChunkCount = 1;
                    this.m_NativeResponse.pEntityChunks = pDataChunk;
                }
                else if ((asyncResult != null) && (asyncResult.pDataChunks != null))
                {
                    this.m_NativeResponse.EntityChunkCount = asyncResult.dataChunkCount;
                    this.m_NativeResponse.pEntityChunks = asyncResult.pDataChunks;
                }
                else
                {
                    this.m_NativeResponse.EntityChunkCount = 0;
                    this.m_NativeResponse.pEntityChunks = null;
                }
                if (this.StatusDescription.Length > 0)
                {
                    ref byte pinned numRef;
                    byte[] bytes = new byte[WebHeaderCollection.HeaderEncoding.GetByteCount(this.StatusDescription)];
                    try
                    {
                        byte[] buffer2;
                        if (((buffer2 = bytes) == null) || (buffer2.Length == 0))
                        {
                            numRef = null;
                        }
                        else
                        {
                            numRef = buffer2;
                        }
                        this.m_NativeResponse.ReasonLength = (ushort) bytes.Length;
                        WebHeaderCollection.HeaderEncoding.GetBytes(this.StatusDescription, 0, bytes.Length, bytes, 0);
                        this.m_NativeResponse.pReason = (sbyte*) numRef;
                        fixed (UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE* http_responseRef = &this.m_NativeResponse)
                        {
                            if (asyncResult != null)
                            {
                                this.HttpListenerContext.EnsureBoundHandle();
                            }
                            num2 = UnsafeNclNativeMethods.HttpApi.HttpSendHttpResponse(this.HttpListenerContext.RequestQueueHandle, this.HttpListenerRequest.RequestId, (uint) flags, http_responseRef, null, null, SafeLocalFree.Zero, 0, (asyncResult == null) ? null : asyncResult.m_pOverlapped, null);
                        }
                        return num2;
                    }
                    finally
                    {
                        numRef = null;
                    }
                }
                fixed (UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE* http_responseRef2 = &this.m_NativeResponse)
                {
                    if (asyncResult != null)
                    {
                        this.HttpListenerContext.EnsureBoundHandle();
                    }
                    num2 = UnsafeNclNativeMethods.HttpApi.HttpSendHttpResponse(this.HttpListenerContext.RequestQueueHandle, this.HttpListenerRequest.RequestId, (uint) flags, http_responseRef2, null, null, SafeLocalFree.Zero, 0, (asyncResult == null) ? null : asyncResult.m_pOverlapped, null);
                }
            }
            finally
            {
                this.FreePinnedHeaders(pinnedHeaders);
            }
            return num2;
        }

        private unsafe List<GCHandle> SerializeHeaders(ref UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADERS headers)
        {
            UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER[] http_unknown_headerArray = null;
            int num;
            if (this.Headers.Count == 0)
            {
                return null;
            }
            byte[] bytes = null;
            List<GCHandle> pinnedHeaders = new List<GCHandle>();
            int num2 = 0;
            for (int i = 0; i < this.Headers.Count; i++)
            {
                num = UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.IndexOfKnownHeader(this.Headers.GetKey(i));
                switch (num)
                {
                    case 0x1b:
                        num = -1;
                        break;

                    case -1:
                    {
                        string[] values = this.Headers.GetValues(i);
                        num2 += values.Length;
                        break;
                    }
                }
            }
            try
            {
                fixed (UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER* http_known_headerRef = &headers.KnownHeaders)
                {
                    for (int j = 0; j < this.Headers.Count; j++)
                    {
                        GCHandle handle;
                        string key = this.Headers.GetKey(j);
                        string myString = this.Headers.Get(j);
                        num = UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.IndexOfKnownHeader(key);
                        switch (num)
                        {
                            case 0x1b:
                                num = -1;
                                break;

                            case -1:
                            {
                                if (http_unknown_headerArray == null)
                                {
                                    http_unknown_headerArray = new UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER[num2];
                                    handle = GCHandle.Alloc(http_unknown_headerArray, GCHandleType.Pinned);
                                    pinnedHeaders.Add(handle);
                                    headers.pUnknownHeaders = (UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER*) handle.AddrOfPinnedObject();
                                }
                                string[] strArray2 = this.Headers.GetValues(j);
                                for (int k = 0; k < strArray2.Length; k++)
                                {
                                    bytes = new byte[WebHeaderCollection.HeaderEncoding.GetByteCount(key)];
                                    http_unknown_headerArray[headers.UnknownHeaderCount].NameLength = (ushort) bytes.Length;
                                    WebHeaderCollection.HeaderEncoding.GetBytes(key, 0, bytes.Length, bytes, 0);
                                    handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                                    pinnedHeaders.Add(handle);
                                    http_unknown_headerArray[headers.UnknownHeaderCount].pName = (sbyte*) handle.AddrOfPinnedObject();
                                    myString = strArray2[k];
                                    bytes = new byte[WebHeaderCollection.HeaderEncoding.GetByteCount(myString)];
                                    http_unknown_headerArray[headers.UnknownHeaderCount].RawValueLength = (ushort) bytes.Length;
                                    WebHeaderCollection.HeaderEncoding.GetBytes(myString, 0, bytes.Length, bytes, 0);
                                    handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                                    pinnedHeaders.Add(handle);
                                    http_unknown_headerArray[headers.UnknownHeaderCount].pRawValue = (sbyte*) handle.AddrOfPinnedObject();
                                    headers.UnknownHeaderCount = (ushort) (headers.UnknownHeaderCount + 1);
                                }
                                continue;
                            }
                        }
                        if (myString != null)
                        {
                            bytes = new byte[WebHeaderCollection.HeaderEncoding.GetByteCount(myString)];
                            http_known_headerRef[num].RawValueLength = (ushort) bytes.Length;
                            WebHeaderCollection.HeaderEncoding.GetBytes(myString, 0, bytes.Length, bytes, 0);
                            handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                            pinnedHeaders.Add(handle);
                            http_known_headerRef[num].pRawValue = (sbyte*) handle.AddrOfPinnedObject();
                        }
                    }
                }
            }
            catch
            {
                this.FreePinnedHeaders(pinnedHeaders);
                throw;
            }
            return pinnedHeaders;
        }

        public void SetCookie(Cookie cookie)
        {
            if (cookie == null)
            {
                throw new ArgumentNullException("cookie");
            }
            Cookie cookie2 = cookie.Clone();
            int num = this.Cookies.InternalAdd(cookie2, true);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.HttpListener, this, "SetCookie", " cookie#" + ValidationHelper.HashString(cookie));
            }
            if (num != 1)
            {
                throw new ArgumentException(SR.GetString("net_cookie_exists"), "cookie");
            }
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal System.Net.BoundaryType BoundaryType
        {
            get
            {
                return this.m_BoundaryType;
            }
        }

        internal bool ComputedHeaders
        {
            get
            {
                return (this.m_ResponseState >= ResponseState.ComputedHeaders);
            }
        }

        public Encoding ContentEncoding
        {
            get
            {
                return this.m_ContentEncoding;
            }
            set
            {
                this.m_ContentEncoding = value;
            }
        }

        public long ContentLength64
        {
            get
            {
                return this.m_ContentLength;
            }
            set
            {
                this.CheckDisposed();
                if (this.m_ResponseState >= ResponseState.SentHeaders)
                {
                    throw new InvalidOperationException(SR.GetString("net_rspsubmitted"));
                }
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_clsmall"));
                }
                this.m_ContentLength = value;
                this.m_BoundaryType = System.Net.BoundaryType.ContentLength;
            }
        }

        public string ContentType
        {
            get
            {
                return this.Headers[HttpResponseHeader.ContentType];
            }
            set
            {
                this.CheckDisposed();
                if (string.IsNullOrEmpty(value))
                {
                    this.Headers.Remove(HttpResponseHeader.ContentType);
                }
                else
                {
                    this.Headers.Set(HttpResponseHeader.ContentType, value);
                }
            }
        }

        public CookieCollection Cookies
        {
            get
            {
                if (this.m_Cookies == null)
                {
                    this.m_Cookies = new CookieCollection(false);
                }
                return this.m_Cookies;
            }
            set
            {
                this.m_Cookies = value;
            }
        }

        internal System.Net.EntitySendFormat EntitySendFormat
        {
            get
            {
                return (System.Net.EntitySendFormat) this.m_BoundaryType;
            }
            set
            {
                this.CheckDisposed();
                if (this.m_ResponseState >= ResponseState.SentHeaders)
                {
                    throw new InvalidOperationException(SR.GetString("net_rspsubmitted"));
                }
                if ((value == System.Net.EntitySendFormat.Chunked) && (this.HttpListenerRequest.ProtocolVersion.Minor == 0))
                {
                    throw new ProtocolViolationException(SR.GetString("net_nochunkuploadonhttp10"));
                }
                this.m_BoundaryType = (System.Net.BoundaryType) value;
                if (value != System.Net.EntitySendFormat.ContentLength)
                {
                    this.m_ContentLength = -1L;
                }
            }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return this.m_WebHeaders;
            }
            set
            {
                this.m_WebHeaders.Clear();
                foreach (string str in value.AllKeys)
                {
                    this.m_WebHeaders.Add(str, value[str]);
                }
            }
        }

        private System.Net.HttpListenerContext HttpListenerContext
        {
            get
            {
                return this.m_HttpContext;
            }
        }

        private System.Net.HttpListenerRequest HttpListenerRequest
        {
            get
            {
                return this.HttpListenerContext.Request;
            }
        }

        public bool KeepAlive
        {
            get
            {
                return this.m_KeepAlive;
            }
            set
            {
                this.CheckDisposed();
                this.m_KeepAlive = value;
            }
        }

        public Stream OutputStream
        {
            get
            {
                this.CheckDisposed();
                this.EnsureResponseStream();
                return this.m_ResponseStream;
            }
        }

        public Version ProtocolVersion
        {
            get
            {
                return new Version(this.m_NativeResponse.Version.MajorVersion, this.m_NativeResponse.Version.MinorVersion);
            }
            set
            {
                this.CheckDisposed();
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if ((value.Major != 1) || ((value.Minor != 0) && (value.Minor != 1)))
                {
                    throw new ArgumentException(SR.GetString("net_wrongversion"), "value");
                }
                this.m_NativeResponse.Version.MajorVersion = (ushort) value.Major;
                this.m_NativeResponse.Version.MinorVersion = (ushort) value.Minor;
            }
        }

        public string RedirectLocation
        {
            get
            {
                return this.Headers[HttpResponseHeader.Location];
            }
            set
            {
                this.CheckDisposed();
                if (string.IsNullOrEmpty(value))
                {
                    this.Headers.Remove(HttpResponseHeader.Location);
                }
                else
                {
                    this.Headers.Set(HttpResponseHeader.Location, value);
                }
            }
        }

        public bool SendChunked
        {
            get
            {
                return (this.EntitySendFormat == System.Net.EntitySendFormat.Chunked);
            }
            set
            {
                if (value)
                {
                    this.EntitySendFormat = System.Net.EntitySendFormat.Chunked;
                }
                else
                {
                    this.EntitySendFormat = System.Net.EntitySendFormat.ContentLength;
                }
            }
        }

        internal bool SentHeaders
        {
            get
            {
                return (this.m_ResponseState >= ResponseState.SentHeaders);
            }
        }

        public int StatusCode
        {
            get
            {
                return this.m_NativeResponse.StatusCode;
            }
            set
            {
                this.CheckDisposed();
                if ((value < 100) || (value > 0x3e7))
                {
                    throw new ProtocolViolationException(SR.GetString("net_invalidstatus"));
                }
                this.m_NativeResponse.StatusCode = (ushort) value;
            }
        }

        public string StatusDescription
        {
            get
            {
                if (this.m_StatusDescription == null)
                {
                    this.m_StatusDescription = GetStatusDescription(this.StatusCode);
                }
                if (this.m_StatusDescription == null)
                {
                    this.m_StatusDescription = string.Empty;
                }
                return this.m_StatusDescription;
            }
            set
            {
                this.CheckDisposed();
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                for (int i = 0; i < value.Length; i++)
                {
                    char ch = (char) ('\x00ff' & value[i]);
                    if (((ch <= '\x001f') && (ch != '\t')) || (ch == '\x007f'))
                    {
                        throw new ArgumentException(SR.GetString("net_WebHeaderInvalidControlChars"), "name");
                    }
                }
                this.m_StatusDescription = value;
            }
        }

        private enum ResponseState
        {
            Created,
            ComputedHeaders,
            SentHeaders,
            Closed
        }
    }
}

