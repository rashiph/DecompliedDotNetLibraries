namespace System.Web
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.Util;

    public sealed class HttpResponse
    {
        private string _appPathModifier;
        private bool _bufferOutput;
        private string _cacheControl;
        private bool _cacheControlHeaderAdded;
        private CacheDependency _cacheDependencyForResponse;
        private ArrayList _cacheHeaders;
        private ResponseDependencyList _cacheItemDependencyList;
        private HttpCachePolicy _cachePolicy;
        private string _charSet;
        private bool _chunked;
        private bool _clientDisconnected;
        private bool _closeConnectionAfterError;
        private bool _completed;
        private bool _contentLengthSet;
        private string _contentType;
        private bool _contentTypeSetByManagedCaller;
        private bool _contentTypeSetByManagedHandler;
        private HttpContext _context;
        private HttpCookieCollection _cookies;
        private bool _customCharSet;
        private ArrayList _customHeaders;
        private System.Text.Encoder _encoder;
        private Encoding _encoding;
        private bool _ended;
        private DateTime _expiresAbsolute;
        private bool _expiresAbsoluteSet;
        private int _expiresInMinutes;
        private bool _expiresInMinutesSet;
        private ResponseDependencyList _fileDependencyList;
        private bool _filteringCompleted;
        private bool _flushing;
        private bool _handlerHeadersGenerated;
        private Encoding _headerEncoding;
        private HttpHeaderCollection _headers;
        private bool _headersWritten;
        private HttpWriter _httpWriter;
        private bool _isRequestBeingRedirected;
        private ErrorFormatter _overrideErrorFormatter;
        private string _redirectLocation;
        private bool _redirectLocationSet;
        private static readonly string _redirectQueryString = ("?" + RedirectQueryStringAssignment);
        private static readonly string _redirectQueryStringInline = (RedirectQueryStringAssignment + "&");
        private bool _sendCacheControlHeader;
        private int _statusCode;
        private string _statusDescription;
        private bool _statusSet;
        private int _subStatusCode;
        private bool _suppressContent;
        private bool _suppressContentSet;
        private bool _suppressHeaders;
        private bool _transferEncodingSet;
        private bool _useAdaptiveError;
        private CacheDependency[] _userAddedDependencies;
        private bool _versionHeaderSent;
        private ResponseDependencyList _virtualPathDependencyList;
        private HttpWorkerRequest _wr;
        private TextWriter _writer;
        internal static readonly string RedirectQueryStringAssignment = (RedirectQueryStringVariable + "=" + RedirectQueryStringValue);
        internal static readonly string RedirectQueryStringValue = "1";
        internal static readonly string RedirectQueryStringVariable = "__redir";
        private static byte[] s_chunkEnd = new byte[] { 0x30, 13, 10, 13, 10 };
        private static byte[] s_chunkSuffix = new byte[] { 13, 10 };

        internal static  event EventHandler Redirecting;

        public HttpResponse(TextWriter writer)
        {
            this._statusCode = 200;
            this._bufferOutput = true;
            this._contentType = "text/html";
            this._wr = null;
            this._httpWriter = null;
            this._writer = writer;
        }

        internal HttpResponse(HttpWorkerRequest wr, HttpContext context)
        {
            this._statusCode = 200;
            this._bufferOutput = true;
            this._contentType = "text/html";
            this._wr = wr;
            this._context = context;
        }

        public void AddCacheDependency(params CacheDependency[] dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException("dependencies");
            }
            if (dependencies.Length != 0)
            {
                if (this._cacheDependencyForResponse != null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Invalid_operation_cache_dependency"));
                }
                if (this._userAddedDependencies == null)
                {
                    this._userAddedDependencies = (CacheDependency[]) dependencies.Clone();
                }
                else
                {
                    CacheDependency[] dependencyArray = new CacheDependency[this._userAddedDependencies.Length + dependencies.Length];
                    int index = 0;
                    index = 0;
                    while (index < this._userAddedDependencies.Length)
                    {
                        dependencyArray[index] = this._userAddedDependencies[index];
                        index++;
                    }
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        dependencyArray[index + i] = dependencies[i];
                    }
                    this._userAddedDependencies = dependencyArray;
                }
                this.Cache.SetDependencies(true);
            }
        }

        public void AddCacheItemDependencies(ArrayList cacheKeys)
        {
            this._cacheItemDependencyList.AddDependencies(cacheKeys, "cacheKeys");
        }

        public void AddCacheItemDependencies(string[] cacheKeys)
        {
            this._cacheItemDependencyList.AddDependencies(cacheKeys, "cacheKeys");
        }

        public void AddCacheItemDependency(string cacheKey)
        {
            this._cacheItemDependencyList.AddDependency(cacheKey, "cacheKey");
        }

        public void AddFileDependencies(ArrayList filenames)
        {
            this._fileDependencyList.AddDependencies(filenames, "filenames");
        }

        public void AddFileDependencies(string[] filenames)
        {
            this._fileDependencyList.AddDependencies(filenames, "filenames");
        }

        internal void AddFileDependencies(string[] filenames, DateTime utcTime)
        {
            this._fileDependencyList.AddDependencies(filenames, "filenames", false, utcTime);
        }

        public void AddFileDependency(string filename)
        {
            this._fileDependencyList.AddDependency(filename, "filename");
        }

        public void AddHeader(string name, string value)
        {
            this.AppendHeader(name, value);
        }

        internal void AddVirtualPathDependencies(string[] virtualPaths)
        {
            this._virtualPathDependencyList.AddDependencies(virtualPaths, "virtualPaths", false, this.Request.Path);
        }

        internal string AppendCharSetToContentType(string contentType)
        {
            string str = contentType;
            if ((this._customCharSet || ((this._httpWriter != null) && this._httpWriter.ResponseEncodingUsed)) && (contentType.IndexOf("charset=", StringComparison.Ordinal) < 0))
            {
                string charset = this.Charset;
                if (charset.Length > 0)
                {
                    str = contentType + "; charset=" + charset;
                }
            }
            return str;
        }

        public void AppendCookie(HttpCookie cookie)
        {
            if (this._headersWritten)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_append_cookie_after_headers_sent"));
            }
            this.Cookies.AddCookie(cookie, true);
            this.OnCookieAdd(cookie);
        }

        private void AppendHeader(HttpResponseHeader h)
        {
            if (this._customHeaders == null)
            {
                this._customHeaders = new ArrayList();
            }
            this._customHeaders.Add(h);
            if ((this._cachePolicy != null) && System.Web.Util.StringUtil.EqualsIgnoreCase("Set-Cookie", h.Name))
            {
                this._cachePolicy.SetHasSetCookieHeader();
            }
        }

        public void AppendHeader(string name, string value)
        {
            bool flag = false;
            if (this._headersWritten)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_append_header_after_headers_sent"));
            }
            int knownResponseHeaderIndex = HttpWorkerRequest.GetKnownResponseHeaderIndex(name);
            switch (knownResponseHeaderIndex)
            {
                case 0:
                    this._cacheControlHeaderAdded = true;
                    break;

                case 6:
                    this._transferEncodingSet = true;
                    goto Label_0095;

                case 11:
                    this._contentLengthSet = true;
                    goto Label_0095;

                case 12:
                    this.ContentType = value;
                    return;

                case 0x12:
                case 0x13:
                case 0x16:
                case 0x1c:
                    break;

                case 0x17:
                    this.RedirectLocation = value;
                    return;

                default:
                    goto Label_0095;
            }
            flag = true;
        Label_0095:
            if (this._wr is IIS7WorkerRequest)
            {
                this.Headers.Add(name, value);
            }
            else if (flag)
            {
                if (this._cacheHeaders == null)
                {
                    this._cacheHeaders = new ArrayList();
                }
                this._cacheHeaders.Add(new HttpResponseHeader(knownResponseHeaderIndex, value));
            }
            else
            {
                HttpResponseHeader header;
                if (knownResponseHeaderIndex >= 0)
                {
                    header = new HttpResponseHeader(knownResponseHeaderIndex, value);
                }
                else
                {
                    header = new HttpResponseHeader(name, value);
                }
                this.AppendHeader(header);
            }
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
        public void AppendToLog(string param)
        {
            if (this._wr is ISAPIWorkerRequest)
            {
                ((ISAPIWorkerRequest) this._wr).AppendLogParameter(param);
            }
            else if (this._wr is IIS7WorkerRequest)
            {
                this._context.Request.AppendToLogQueryString(param);
            }
        }

        public string ApplyAppPathModifier(string virtualPath)
        {
            CookielessHelperClass cookielessHelper = this._context.CookielessHelper;
            if (virtualPath == null)
            {
                return null;
            }
            if (System.Web.Util.UrlPath.IsRelativeUrl(virtualPath))
            {
                virtualPath = System.Web.Util.UrlPath.Combine(this.Request.ClientBaseDir.VirtualPathString, virtualPath);
            }
            else
            {
                if (!System.Web.Util.UrlPath.IsRooted(virtualPath) || virtualPath.StartsWith("//", StringComparison.Ordinal))
                {
                    return virtualPath;
                }
                virtualPath = System.Web.Util.UrlPath.Reduce(virtualPath);
            }
            if ((this._appPathModifier != null) && (virtualPath.IndexOf(this._appPathModifier, StringComparison.Ordinal) < 0))
            {
                string appDomainAppVirtualPathString = HttpRuntime.AppDomainAppVirtualPathString;
                int length = appDomainAppVirtualPathString.Length;
                bool flag = virtualPath.Length == (appDomainAppVirtualPathString.Length - 1);
                if (flag)
                {
                    length--;
                }
                if (virtualPath.Length < length)
                {
                    return virtualPath;
                }
                if (!System.Web.Util.StringUtil.EqualsIgnoreCase(virtualPath, 0, appDomainAppVirtualPathString, 0, length))
                {
                    return virtualPath;
                }
                if (flag)
                {
                    virtualPath = virtualPath + "/";
                }
                if (virtualPath.Length == appDomainAppVirtualPathString.Length)
                {
                    virtualPath = virtualPath.Substring(0, appDomainAppVirtualPathString.Length) + this._appPathModifier + "/";
                    return virtualPath;
                }
                virtualPath = virtualPath.Substring(0, appDomainAppVirtualPathString.Length) + this._appPathModifier + "/" + virtualPath.Substring(appDomainAppVirtualPathString.Length);
            }
            return virtualPath;
        }

        internal string ApplyRedirectQueryStringIfRequired(string url)
        {
            if ((this.Request != null) && (this.Request.Browser["requiresPostRedirectionHandling"] == "true"))
            {
                Page handler = this._context.Handler as Page;
                if ((handler != null) && !handler.IsPostBack)
                {
                    return url;
                }
                if (url.IndexOf(RedirectQueryStringAssignment, StringComparison.Ordinal) != -1)
                {
                    return url;
                }
                int index = url.IndexOf('?');
                if (index >= 0)
                {
                    url = url.Insert(index + 1, _redirectQueryStringInline);
                    return url;
                }
                url = url + _redirectQueryString;
            }
            return url;
        }

        internal void BeforeCookieCollectionChange()
        {
            if (this._headersWritten)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_modify_cookies_after_headers_sent"));
            }
        }

        internal IAsyncResult BeginExecuteUrlForEntireResponse(string pathOverride, NameValueCollection requestHeaders, AsyncCallback cb, object state)
        {
            string name;
            string authenticationType;
            if ((this._context != null) && (this._context.User != null))
            {
                name = this._context.User.Identity.Name;
                authenticationType = this._context.User.Identity.AuthenticationType;
            }
            else
            {
                name = string.Empty;
                authenticationType = string.Empty;
            }
            string rewrittenUrl = this.Request.RewrittenUrl;
            if (pathOverride != null)
            {
                rewrittenUrl = pathOverride;
            }
            string headers = null;
            if (requestHeaders != null)
            {
                int count = requestHeaders.Count;
                if (count > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < count; i++)
                    {
                        builder.Append(requestHeaders.GetKey(i));
                        builder.Append(": ");
                        builder.Append(requestHeaders.Get(i));
                        builder.Append("\r\n");
                    }
                    headers = builder.ToString();
                }
            }
            byte[] entity = null;
            if ((this._context != null) && (this._context.Request != null))
            {
                entity = this._context.Request.EntityBody;
            }
            IAsyncResult result = this._wr.BeginExecuteUrl(rewrittenUrl, null, headers, true, true, this._wr.GetUserToken(), name, authenticationType, entity, cb, state);
            this._headersWritten = true;
            this._ended = true;
            return result;
        }

        public void BinaryWrite(byte[] buffer)
        {
            this.OutputStream.Write(buffer, 0, buffer.Length);
        }

        public void Clear()
        {
            if (this.UsingHttpWriter)
            {
                this._httpWriter.ClearBuffers();
            }
            IIS7WorkerRequest wr = this._wr as IIS7WorkerRequest;
            if (wr != null)
            {
                this.ClearNativeResponse(true, false, wr);
            }
        }

        internal void ClearAll()
        {
            if (!this._headersWritten)
            {
                this.ClearHeaders();
            }
            this.Clear();
        }

        public void ClearContent()
        {
            this.Clear();
        }

        public void ClearHeaders()
        {
            if (this._headersWritten)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_clear_headers_after_headers_sent"));
            }
            this.StatusCode = 200;
            this._subStatusCode = 0;
            this._statusDescription = null;
            this._contentType = "text/html";
            this._contentTypeSetByManagedCaller = false;
            this._charSet = null;
            this._customCharSet = false;
            this._contentLengthSet = false;
            this._redirectLocation = null;
            this._redirectLocationSet = false;
            this._isRequestBeingRedirected = false;
            this._customHeaders = null;
            if (this._headers != null)
            {
                this._headers.ClearInternal();
            }
            this._transferEncodingSet = false;
            this._chunked = false;
            if (this._cookies != null)
            {
                this._cookies.Reset();
                this.Request.ResetCookies();
            }
            if (this._cachePolicy != null)
            {
                this._cachePolicy.Reset();
            }
            this._cacheControlHeaderAdded = false;
            this._cacheHeaders = null;
            this._suppressHeaders = false;
            this._suppressContent = false;
            this._suppressContentSet = false;
            this._expiresInMinutes = 0;
            this._expiresInMinutesSet = false;
            this._expiresAbsolute = DateTime.MinValue;
            this._expiresAbsoluteSet = false;
            this._cacheControl = null;
            IIS7WorkerRequest wr = this._wr as IIS7WorkerRequest;
            if (wr != null)
            {
                this.ClearNativeResponse(false, true, wr);
                if (this._handlerHeadersGenerated && this._sendCacheControlHeader)
                {
                    this.Headers.Set("Cache-Control", "private");
                }
                this._handlerHeadersGenerated = false;
            }
        }

        private void ClearNativeResponse(bool clearEntity, bool clearHeaders, IIS7WorkerRequest wr)
        {
            wr.ClearResponse(clearEntity, clearHeaders);
            if (clearEntity)
            {
                this._httpWriter.ClearSubstitutionBlocks();
            }
        }

        public void Close()
        {
            if ((!this._clientDisconnected && !this._completed) && (this._wr != null))
            {
                this._wr.CloseConnection();
                this._clientDisconnected = true;
            }
        }

        internal void CloseConnectionAfterError()
        {
            this._closeConnectionAfterError = true;
        }

        private string ConvertToFullyQualifiedRedirectUrlIfRequired(string url)
        {
            if (!RuntimeConfig.GetConfig(this._context).HttpRuntime.UseFullyQualifiedRedirectUrl && ((this.Request == null) || !(this.Request.Browser["requiresFullyQualifiedRedirectUrl"] == "true")))
            {
                return url;
            }
            return new Uri(this.Request.Url, url).AbsoluteUri;
        }

        internal CacheDependency CreateCacheDependencyForResponse()
        {
            if (this._cacheDependencyForResponse == null)
            {
                CacheDependency dependency = this._cacheItemDependencyList.CreateCacheDependency(CacheDependencyType.CacheItems, null);
                dependency = this._fileDependencyList.CreateCacheDependency(CacheDependencyType.Files, dependency);
                dependency = this._virtualPathDependencyList.CreateCacheDependency(CacheDependencyType.VirtualPaths, dependency);
                if (this._userAddedDependencies != null)
                {
                    AggregateCacheDependency dependency2 = new AggregateCacheDependency();
                    dependency2.Add(this._userAddedDependencies);
                    if (dependency != null)
                    {
                        dependency2.Add(new CacheDependency[] { dependency });
                    }
                    this._userAddedDependencies = null;
                    this._cacheDependencyForResponse = dependency2;
                }
                else
                {
                    this._cacheDependencyForResponse = dependency;
                }
            }
            return this._cacheDependencyForResponse;
        }

        public void DisableKernelCache()
        {
            if (this._wr != null)
            {
                this._wr.DisableKernelCache();
            }
        }

        internal void Dispose()
        {
            if (this._httpWriter != null)
            {
                this._httpWriter.RecycleBuffers();
            }
            if (this._cacheDependencyForResponse != null)
            {
                this._cacheDependencyForResponse.Dispose();
                this._cacheDependencyForResponse = null;
            }
            if (this._userAddedDependencies != null)
            {
                foreach (CacheDependency dependency in this._userAddedDependencies)
                {
                    dependency.Dispose();
                }
                this._userAddedDependencies = null;
            }
        }

        public void End()
        {
            if (this._context.IsInCancellablePeriod)
            {
                InternalSecurityPermissions.ControlThread.Assert();
                Thread.CurrentThread.Abort(new HttpApplication.CancelModuleException(false));
            }
            else if (!this._flushing)
            {
                this.Flush();
                this._ended = true;
                if (this._context.ApplicationInstance != null)
                {
                    this._context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        internal void EndExecuteUrlForEntireResponse(IAsyncResult result)
        {
            this._wr.EndExecuteUrl(result);
        }

        internal void FilterOutput()
        {
            if (!this._filteringCompleted)
            {
                try
                {
                    if (this.UsingHttpWriter)
                    {
                        IIS7WorkerRequest wr = this._wr as IIS7WorkerRequest;
                        if (wr != null)
                        {
                            this._httpWriter.FilterIntegrated(true, wr);
                        }
                        else
                        {
                            this._httpWriter.Filter(true);
                        }
                    }
                }
                finally
                {
                    this._filteringCompleted = true;
                }
            }
        }

        internal void FinalFlushAtTheEndOfRequestProcessing()
        {
            this.FinalFlushAtTheEndOfRequestProcessing(false);
        }

        internal void FinalFlushAtTheEndOfRequestProcessing(bool needPipelineCompletion)
        {
            this.Flush(true);
        }

        public void Flush()
        {
            if (this._completed)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_flush_completed_response"));
            }
            this.Flush(false);
        }

        private void Flush(bool finalFlush)
        {
            if (!this._completed && !this._flushing)
            {
                if (this._httpWriter == null)
                {
                    this._writer.Flush();
                }
                else
                {
                    this._flushing = true;
                    try
                    {
                        IIS7WorkerRequest request = this._wr as IIS7WorkerRequest;
                        if (request != null)
                        {
                            this.GenerateResponseHeadersForHandler();
                            this.UpdateNativeResponse(true);
                            request.ExplicitFlush();
                            this._headersWritten = true;
                        }
                        else
                        {
                            long contentLength = 0L;
                            if (!this._headersWritten)
                            {
                                if (!this._suppressHeaders && !this._clientDisconnected)
                                {
                                    if (finalFlush)
                                    {
                                        contentLength = this._httpWriter.GetBufferedLength();
                                        if ((!this._contentLengthSet && (contentLength == 0L)) && (this._httpWriter != null))
                                        {
                                            this._contentType = null;
                                        }
                                        if (((this._cachePolicy != null) && (this._cookies != null)) && (this._cookies.Count != 0))
                                        {
                                            this._cachePolicy.SetHasSetCookieHeader();
                                            this.DisableKernelCache();
                                        }
                                        this.WriteHeaders();
                                        contentLength = this._httpWriter.GetBufferedLength();
                                        if (!this._contentLengthSet && (this._statusCode != 0x130))
                                        {
                                            this._wr.SendCalculatedContentLength(contentLength);
                                        }
                                    }
                                    else
                                    {
                                        if ((!this._contentLengthSet && !this._transferEncodingSet) && (this._statusCode == 200))
                                        {
                                            string httpVersion = this._wr.GetHttpVersion();
                                            if ((httpVersion != null) && httpVersion.Equals("HTTP/1.1"))
                                            {
                                                this.AppendHeader(new HttpResponseHeader(6, "chunked"));
                                                this._chunked = true;
                                            }
                                            contentLength = this._httpWriter.GetBufferedLength();
                                        }
                                        this.WriteHeaders();
                                    }
                                }
                                this._headersWritten = true;
                            }
                            else
                            {
                                contentLength = this._httpWriter.GetBufferedLength();
                            }
                            if (!this._filteringCompleted)
                            {
                                this._httpWriter.Filter(false);
                                contentLength = this._httpWriter.GetBufferedLength();
                            }
                            if ((!this._suppressContentSet && (this.Request != null)) && (this.Request.HttpVerb == HttpVerb.HEAD))
                            {
                                this._suppressContent = true;
                            }
                            if (this._suppressContent || this._ended)
                            {
                                this._httpWriter.ClearBuffers();
                                contentLength = 0L;
                            }
                            if (!this._clientDisconnected)
                            {
                                if ((this._context != null) && (this._context.ApplicationInstance != null))
                                {
                                    this._context.ApplicationInstance.RaiseOnPreSendRequestContent();
                                }
                                if (this._chunked)
                                {
                                    if (contentLength > 0L)
                                    {
                                        byte[] bytes = Encoding.ASCII.GetBytes(Convert.ToString(contentLength, 0x10) + "\r\n");
                                        this._wr.SendResponseFromMemory(bytes, bytes.Length);
                                        this._httpWriter.Send(this._wr);
                                        this._wr.SendResponseFromMemory(s_chunkSuffix, s_chunkSuffix.Length);
                                    }
                                    if (finalFlush)
                                    {
                                        this._wr.SendResponseFromMemory(s_chunkEnd, s_chunkEnd.Length);
                                    }
                                }
                                else
                                {
                                    this._httpWriter.Send(this._wr);
                                }
                                this._wr.FlushResponse(finalFlush);
                                this._wr.UpdateResponseCounters(finalFlush, (int) contentLength);
                                if (!finalFlush)
                                {
                                    this._httpWriter.ClearBuffers();
                                }
                            }
                        }
                    }
                    finally
                    {
                        this._flushing = false;
                        if (finalFlush && this._headersWritten)
                        {
                            this._completed = true;
                        }
                    }
                }
            }
        }

        internal ArrayList GenerateResponseHeaders(bool forCache)
        {
            ArrayList headers = new ArrayList();
            bool sendCacheControlHeader = true;
            if (!forCache && !this._versionHeaderSent)
            {
                string versionHeader = null;
                RuntimeConfig lKGConfig = RuntimeConfig.GetLKGConfig(this._context);
                HttpRuntimeSection httpRuntime = lKGConfig.HttpRuntime;
                if (httpRuntime != null)
                {
                    versionHeader = httpRuntime.VersionHeader;
                    sendCacheControlHeader = httpRuntime.SendCacheControlHeader;
                }
                OutputCacheSection outputCache = lKGConfig.OutputCache;
                if (outputCache != null)
                {
                    sendCacheControlHeader &= outputCache.SendCacheControlHeader;
                }
                if (!string.IsNullOrEmpty(versionHeader))
                {
                    headers.Add(new HttpResponseHeader("X-AspNet-Version", versionHeader));
                }
                this._versionHeaderSent = true;
            }
            if (this._customHeaders != null)
            {
                int count = this._customHeaders.Count;
                for (int i = 0; i < count; i++)
                {
                    headers.Add(this._customHeaders[i]);
                }
            }
            if (this._redirectLocation != null)
            {
                headers.Add(new HttpResponseHeader(0x17, this._redirectLocation));
            }
            if (!forCache)
            {
                if (this._cookies != null)
                {
                    int num3 = this._cookies.Count;
                    for (int j = 0; j < num3; j++)
                    {
                        headers.Add(this._cookies[j].GetSetCookieHeader(this.Context));
                    }
                }
                if ((this._cachePolicy != null) && this._cachePolicy.IsModified())
                {
                    this._cachePolicy.GetHeaders(headers, this);
                }
                else
                {
                    if (this._cacheHeaders != null)
                    {
                        headers.AddRange(this._cacheHeaders);
                    }
                    if (!this._cacheControlHeaderAdded && sendCacheControlHeader)
                    {
                        headers.Add(new HttpResponseHeader(0, "private"));
                    }
                }
            }
            if ((this._statusCode != 0xcc) && (this._contentType != null))
            {
                string str2 = this.AppendCharSetToContentType(this._contentType);
                headers.Add(new HttpResponseHeader(12, str2));
            }
            return headers;
        }

        internal void GenerateResponseHeadersForCookies()
        {
            if ((this._cookies != null) && ((this._cookies.Count != 0) || this._cookies.Changed))
            {
                HttpHeaderCollection headers = this.Headers as HttpHeaderCollection;
                HttpResponseHeader setCookieHeader = null;
                HttpCookie cookie = null;
                bool flag = false;
                if (!this._cookies.Changed)
                {
                    for (int i = 0; i < this._cookies.Count; i++)
                    {
                        cookie = this._cookies[i];
                        if (cookie.Added)
                        {
                            setCookieHeader = cookie.GetSetCookieHeader(this._context);
                            headers.SetHeader(setCookieHeader.Name, setCookieHeader.Value, false);
                            cookie.Added = false;
                            cookie.Changed = false;
                        }
                        else if (cookie.Changed)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (this._cookies.Changed || flag)
                {
                    headers.Remove("Set-Cookie");
                    for (int j = 0; j < this._cookies.Count; j++)
                    {
                        cookie = this._cookies[j];
                        setCookieHeader = cookie.GetSetCookieHeader(this._context);
                        headers.SetHeader(setCookieHeader.Name, setCookieHeader.Value, false);
                        cookie.Added = false;
                        cookie.Changed = false;
                    }
                    this._cookies.Changed = false;
                }
            }
        }

        internal void GenerateResponseHeadersForHandler()
        {
            if (this._wr is IIS7WorkerRequest)
            {
                string versionHeader = null;
                if (!this._headersWritten && !this._handlerHeadersGenerated)
                {
                    try
                    {
                        RuntimeConfig lKGConfig = RuntimeConfig.GetLKGConfig(this._context);
                        HttpRuntimeSection httpRuntime = lKGConfig.HttpRuntime;
                        if (httpRuntime != null)
                        {
                            versionHeader = httpRuntime.VersionHeader;
                            this._sendCacheControlHeader = httpRuntime.SendCacheControlHeader;
                        }
                        OutputCacheSection outputCache = lKGConfig.OutputCache;
                        if (outputCache != null)
                        {
                            this._sendCacheControlHeader &= outputCache.SendCacheControlHeader;
                        }
                        if (this._sendCacheControlHeader && !this._cacheControlHeaderAdded)
                        {
                            this.Headers.Set("Cache-Control", "private");
                        }
                        if (!string.IsNullOrEmpty(versionHeader))
                        {
                            this.Headers.Set("X-AspNet-Version", versionHeader);
                        }
                        this._contentTypeSetByManagedHandler = true;
                    }
                    finally
                    {
                        this._handlerHeadersGenerated = true;
                    }
                }
            }
        }

        internal ArrayList GenerateResponseHeadersIntegrated(bool forCache)
        {
            ArrayList list = new ArrayList();
            HttpHeaderCollection headers = this.Headers as HttpHeaderCollection;
            int knownHeaderIndex = 0;
            foreach (string str in headers)
            {
                knownHeaderIndex = HttpWorkerRequest.GetKnownResponseHeaderIndex(str);
                if (((knownHeaderIndex < 0) || !forCache) || ((((knownHeaderIndex != 0x1a) && (knownHeaderIndex != 0x1b)) && ((knownHeaderIndex != 0) && (knownHeaderIndex != 0x12))) && (((knownHeaderIndex != 0x13) && (knownHeaderIndex != 0x16)) && (knownHeaderIndex != 0x1c))))
                {
                    if (knownHeaderIndex >= 0)
                    {
                        list.Add(new HttpResponseHeader(knownHeaderIndex, headers[str]));
                    }
                    else
                    {
                        list.Add(new HttpResponseHeader(str, headers[str]));
                    }
                }
            }
            return list;
        }

        internal int GetBufferedLength()
        {
            if (this._httpWriter == null)
            {
                return 0;
            }
            return Convert.ToInt32(this._httpWriter.GetBufferedLength());
        }

        internal ErrorFormatter GetErrorFormatter(Exception e)
        {
            ErrorFormatter errorFormatter = null;
            if (this._overrideErrorFormatter != null)
            {
                return this._overrideErrorFormatter;
            }
            errorFormatter = HttpException.GetErrorFormatter(e);
            if (errorFormatter == null)
            {
                ConfigurationException exception = e as ConfigurationException;
                if ((exception != null) && !string.IsNullOrEmpty(exception.Filename))
                {
                    errorFormatter = new ConfigErrorFormatter(exception);
                }
            }
            if (errorFormatter != null)
            {
                return errorFormatter;
            }
            if (this._statusCode == 0x194)
            {
                return new PageNotFoundErrorFormatter(this.Request.Path);
            }
            if (this._statusCode == 0x193)
            {
                return new PageForbiddenErrorFormatter(this.Request.Path);
            }
            if (e is SecurityException)
            {
                return new SecurityErrorFormatter(e);
            }
            return new UnhandledErrorFormatter(e);
        }

        internal string GetHttpHeaderContentEncoding()
        {
            string str = null;
            if (this._wr is IIS7WorkerRequest)
            {
                if (this._headers != null)
                {
                    str = this._headers["Content-Encoding"];
                }
                return str;
            }
            if (this._customHeaders != null)
            {
                int count = this._customHeaders.Count;
                for (int i = 0; i < count; i++)
                {
                    HttpResponseHeader header = (HttpResponseHeader) this._customHeaders[i];
                    if (header.Name == "Content-Encoding")
                    {
                        return header.Value;
                    }
                }
            }
            return str;
        }

        private string GetNormalizedFilename(string fn)
        {
            if (!System.Web.Util.UrlPath.IsAbsolutePhysicalPath(fn))
            {
                if (this.Request != null)
                {
                    fn = this.Request.MapPath(fn);
                    return fn;
                }
                fn = HostingEnvironment.MapPath(fn);
            }
            return fn;
        }

        internal HttpRawResponse GetSnapshot()
        {
            int statusCode = 200;
            string statusDescription = null;
            ArrayList headers = null;
            ArrayList buffers = null;
            bool hasSubstBlocks = false;
            if (!this.IsBuffered())
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_get_snapshot_if_not_buffered"));
            }
            IIS7WorkerRequest wr = this._wr as IIS7WorkerRequest;
            if (!this._suppressContent)
            {
                if (wr != null)
                {
                    buffers = this._httpWriter.GetIntegratedSnapshot(out hasSubstBlocks, wr);
                }
                else
                {
                    buffers = this._httpWriter.GetSnapshot(out hasSubstBlocks);
                }
            }
            if (!this._suppressHeaders)
            {
                statusCode = this._statusCode;
                statusDescription = this._statusDescription;
                if (wr != null)
                {
                    headers = this.GenerateResponseHeadersIntegrated(true);
                }
                else
                {
                    headers = this.GenerateResponseHeaders(true);
                }
            }
            return new HttpRawResponse(statusCode, statusDescription, headers, buffers, hasSubstBlocks);
        }

        internal bool HasCacheItemDependencies()
        {
            return this._cacheItemDependencyList.HasDependencies();
        }

        internal bool HasFileDependencies()
        {
            return this._fileDependencyList.HasDependencies();
        }

        internal void IgnoreFurtherWrites()
        {
            if (this.UsingHttpWriter)
            {
                this._httpWriter.IgnoreFurtherWrites();
            }
        }

        internal void InitResponseWriter()
        {
            if (this._httpWriter == null)
            {
                this._httpWriter = new HttpWriter(this);
                this._writer = this._httpWriter;
            }
        }

        internal bool IsBuffered()
        {
            return (!this._headersWritten && this.UsingHttpWriter);
        }

        private bool IsKernelCacheEnabledForVaryByStar()
        {
            OutputCacheSection outputCache = RuntimeConfig.GetAppConfig().OutputCache;
            return (this._cachePolicy.IsVaryByStar && outputCache.EnableKernelCacheForVaryByStar);
        }

        internal void OnCookieAdd(HttpCookie cookie)
        {
            this.Request.AddResponseCookie(cookie);
        }

        internal void OnCookieCollectionChange()
        {
            this.Request.ResetCookies();
        }

        public void Pics(string value)
        {
            this.AppendHeader("PICS-Label", value);
        }

        public void Redirect(string url)
        {
            this.Redirect(url, true, false);
        }

        public void Redirect(string url, bool endResponse)
        {
            this.Redirect(url, endResponse, false);
        }

        internal void Redirect(string url, bool endResponse, bool permanent)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            if (url.IndexOf('\n') >= 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("Cannot_redirect_to_newline"));
            }
            if (this._headersWritten)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_redirect_after_headers_sent"));
            }
            Page page = this._context.Handler as Page;
            if ((page != null) && page.IsCallback)
            {
                throw new ApplicationException(System.Web.SR.GetString("Redirect_not_allowed_in_callback"));
            }
            url = this.ApplyRedirectQueryStringIfRequired(url);
            url = this.ApplyAppPathModifier(url);
            url = this.ConvertToFullyQualifiedRedirectUrlIfRequired(url);
            url = this.UrlEncodeRedirect(url);
            this.Clear();
            if (((page != null) && page.IsPostBack) && (page.SmartNavigation && (this.Request["__smartNavPostBack"] == "true")))
            {
                this.Write("<BODY><ASP_SMARTNAV_RDIR url=\"");
                this.Write(HttpUtility.HtmlEncode(url));
                this.Write("\"></ASP_SMARTNAV_RDIR>");
                this.Write("</BODY>");
            }
            else
            {
                this.StatusCode = permanent ? 0x12d : 0x12e;
                this.RedirectLocation = url;
                if ((((url.IndexOf(":", StringComparison.Ordinal) == -1) || url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)) || (url.StartsWith("https:", StringComparison.OrdinalIgnoreCase) || url.StartsWith("ftp:", StringComparison.OrdinalIgnoreCase))) || (url.StartsWith("file:", StringComparison.OrdinalIgnoreCase) || url.StartsWith("news:", StringComparison.OrdinalIgnoreCase)))
                {
                    url = HttpUtility.HtmlAttributeEncode(url);
                }
                else
                {
                    url = HttpUtility.HtmlAttributeEncode(HttpUtility.UrlEncode(url));
                }
                this.Write("<html><head><title>Object moved</title></head><body>\r\n");
                this.Write("<h2>Object moved to <a href=\"" + url + "\">here</a>.</h2>\r\n");
                this.Write("</body></html>\r\n");
            }
            this._isRequestBeingRedirected = true;
            EventHandler redirecting = Redirecting;
            if (redirecting != null)
            {
                redirecting(this, EventArgs.Empty);
            }
            if (endResponse)
            {
                this.End();
            }
        }

        public void RedirectPermanent(string url)
        {
            this.Redirect(url, true, true);
        }

        public void RedirectPermanent(string url, bool endResponse)
        {
            this.Redirect(url, endResponse, true);
        }

        internal bool RedirectToErrorPage(string url, CustomErrorsRedirectMode redirectMode)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return false;
                }
                if (this._headersWritten)
                {
                    return false;
                }
                if (this.Request.QueryString["aspxerrorpath"] != null)
                {
                    return false;
                }
                if (redirectMode == CustomErrorsRedirectMode.ResponseRewrite)
                {
                    this.Context.Server.Execute(url);
                }
                else
                {
                    if (url.IndexOf('?') < 0)
                    {
                        url = url + "?aspxerrorpath=" + HttpEncoderUtility.UrlEncodeSpaces(this.Request.Path);
                    }
                    this.Redirect(url, false);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void RedirectToRoute(object routeValues)
        {
            this.RedirectToRoute(new RouteValueDictionary(routeValues));
        }

        public void RedirectToRoute(string routeName)
        {
            this.RedirectToRoute(routeName, null, false);
        }

        public void RedirectToRoute(RouteValueDictionary routeValues)
        {
            this.RedirectToRoute(null, routeValues, false);
        }

        public void RedirectToRoute(string routeName, object routeValues)
        {
            this.RedirectToRoute(routeName, new RouteValueDictionary(routeValues), false);
        }

        public void RedirectToRoute(string routeName, RouteValueDictionary routeValues)
        {
            this.RedirectToRoute(routeName, routeValues, false);
        }

        private void RedirectToRoute(string routeName, RouteValueDictionary routeValues, bool permanent)
        {
            string virtualPath = null;
            VirtualPathData data = RouteTable.Routes.GetVirtualPath(this.Request.RequestContext, routeName, routeValues);
            if (data != null)
            {
                virtualPath = data.VirtualPath;
            }
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("No_Route_Found_For_Redirect"));
            }
            this.Redirect(virtualPath, false, permanent);
        }

        public void RedirectToRoutePermanent(object routeValues)
        {
            this.RedirectToRoutePermanent(new RouteValueDictionary(routeValues));
        }

        public void RedirectToRoutePermanent(string routeName)
        {
            this.RedirectToRoute(routeName, null, true);
        }

        public void RedirectToRoutePermanent(RouteValueDictionary routeValues)
        {
            this.RedirectToRoute(null, routeValues, true);
        }

        public void RedirectToRoutePermanent(string routeName, object routeValues)
        {
            this.RedirectToRoute(routeName, new RouteValueDictionary(routeValues), true);
        }

        public void RedirectToRoutePermanent(string routeName, RouteValueDictionary routeValues)
        {
            this.RedirectToRoute(routeName, routeValues, true);
        }

        internal string RemoveAppPathModifier(string virtualPath)
        {
            if (!string.IsNullOrEmpty(this._appPathModifier))
            {
                int index = virtualPath.IndexOf(this._appPathModifier, StringComparison.Ordinal);
                if ((index > 0) && (virtualPath[index - 1] == '/'))
                {
                    return (virtualPath.Substring(0, index - 1) + virtualPath.Substring(index + this._appPathModifier.Length));
                }
            }
            return virtualPath;
        }

        public static void RemoveOutputCacheItem(string path)
        {
            RemoveOutputCacheItem(path, null);
        }

        public static void RemoveOutputCacheItem(string path, string providerName)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if ((System.Web.Util.StringUtil.StringStartsWith(path, @"\\") || (path.IndexOf(':') >= 0)) || !System.Web.Util.UrlPath.IsRooted(path))
            {
                throw new ArgumentException(System.Web.SR.GetString("Invalid_path_for_remove", new object[] { path }));
            }
            string key = OutputCacheModule.CreateOutputCachedItemKey(path, HttpVerb.GET, null, null);
            if (providerName == null)
            {
                OutputCache.Remove(key, null);
            }
            else
            {
                OutputCache.RemoveFromProvider(key, providerName);
            }
            key = OutputCacheModule.CreateOutputCachedItemKey(path, HttpVerb.POST, null, null);
            if (providerName == null)
            {
                OutputCache.Remove(key, null);
            }
            else
            {
                OutputCache.RemoveFromProvider(key, providerName);
            }
        }

        internal void ReportRuntimeError(Exception e, bool canThrow, bool localExecute)
        {
            CustomErrorsSection settings = null;
            bool dontShowSensitiveErrors = false;
            int code = -1;
            if (!this._completed)
            {
                if (this._wr != null)
                {
                    this._wr.TrySkipIisCustomErrors = true;
                }
                if (!localExecute)
                {
                    code = HttpException.GetHttpCodeForException(e);
                    if (code != 0x194)
                    {
                        WebBaseEvent.RaiseRuntimeError(e, this);
                    }
                    settings = CustomErrorsSection.GetSettings(this._context, canThrow);
                    if (settings != null)
                    {
                        dontShowSensitiveErrors = settings.CustomErrorsEnabled(this.Request);
                    }
                    else
                    {
                        dontShowSensitiveErrors = true;
                    }
                }
                if (!this._headersWritten)
                {
                    if (code == -1)
                    {
                        code = HttpException.GetHttpCodeForException(e);
                    }
                    if ((code == 0x191) && !this._context.IsClientImpersonationConfigured)
                    {
                        code = 500;
                    }
                    if (this._context.TraceIsEnabled)
                    {
                        this._context.Trace.StatusCode = code;
                    }
                    if (localExecute || !dontShowSensitiveErrors)
                    {
                        this.ClearAll();
                        this.StatusCode = code;
                        this.WriteErrorMessage(e, false);
                    }
                    else
                    {
                        string url = (settings != null) ? settings.GetRedirectString(code) : null;
                        if ((url == null) || !this.RedirectToErrorPage(url, settings.RedirectMode))
                        {
                            this.ClearAll();
                            this.StatusCode = code;
                            this.WriteErrorMessage(e, true);
                        }
                    }
                }
                else
                {
                    this.Clear();
                    if ((this._contentType != null) && this._contentType.Equals("text/html"))
                    {
                        this.Write("\r\n\r\n</pre></table></table></table></table></table>");
                        this.Write("</font></font></font></font></font>");
                        this.Write("</i></i></i></i></i></b></b></b></b></b></u></u></u></u></u>");
                        this.Write("<p>&nbsp;</p><hr>\r\n\r\n");
                    }
                    this.WriteErrorMessage(e, dontShowSensitiveErrors);
                }
            }
        }

        internal void SetAppPathModifier(string appPathModifier)
        {
            if ((appPathModifier != null) && (((appPathModifier.Length == 0) || (appPathModifier[0] == '/')) || (appPathModifier[appPathModifier.Length - 1] == '/')))
            {
                throw new ArgumentException(System.Web.SR.GetString("InvalidArgumentValue", new object[] { "appPathModifier" }));
            }
            this._appPathModifier = appPathModifier;
        }

        public void SetCookie(HttpCookie cookie)
        {
            if (this._headersWritten)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_append_cookie_after_headers_sent"));
            }
            this.Cookies.AddCookie(cookie, false);
            this.OnCookieCollectionChange();
        }

        internal void SetOverrideErrorFormatter(ErrorFormatter errorFormatter)
        {
            this._overrideErrorFormatter = errorFormatter;
        }

        internal string SetupKernelCaching(string originalCacheUrl)
        {
            if ((this._cookies != null) && (this._cookies.Count != 0))
            {
                this._cachePolicy.SetHasSetCookieHeader();
                return null;
            }
            bool enableKernelCacheForVaryByStar = this.IsKernelCacheEnabledForVaryByStar();
            if (!this._cachePolicy.IsKernelCacheable(this.Request, enableKernelCacheForVaryByStar))
            {
                return null;
            }
            HttpRuntimeSection httpRuntime = RuntimeConfig.GetLKGConfig(this._context).HttpRuntime;
            if ((httpRuntime == null) || !httpRuntime.EnableKernelOutputCache)
            {
                return null;
            }
            TimeSpan span = (TimeSpan) (this._cachePolicy.UtcGetAbsoluteExpiration() - DateTime.UtcNow);
            double totalSeconds = span.TotalSeconds;
            if (totalSeconds <= 0.0)
            {
                return null;
            }
            int secondsToLive = (totalSeconds < 2147483647.0) ? ((int) totalSeconds) : 0x7fffffff;
            string str = this._wr.SetupKernelCaching(secondsToLive, originalCacheUrl, enableKernelCacheForVaryByStar);
            if (str != null)
            {
                this._cachePolicy.SetNoMaxAgeInCacheControl();
            }
            return str;
        }

        internal TextWriter SwitchWriter(TextWriter writer)
        {
            TextWriter writer2 = this._writer;
            this._writer = writer;
            return writer2;
        }

        internal void SynchronizeHeader(int knownHeaderIndex, string name, string value)
        {
            (this.Headers as HttpHeaderCollection).SynchronizeHeader(name, value);
            if (knownHeaderIndex >= 0)
            {
                bool headersWritten = this.HeadersWritten;
                this.HeadersWritten = false;
                try
                {
                    switch (knownHeaderIndex)
                    {
                        case 0x17:
                            this._redirectLocation = value;
                            this._redirectLocationSet = false;
                            return;

                        case 0x1b:
                            break;

                        case 0:
                            this._cacheControlHeaderAdded = true;
                            return;

                        case 12:
                            this._contentType = value;
                            return;

                        default:
                            return;
                    }
                    if (value != null)
                    {
                        HttpCookie cookie = HttpRequest.CreateCookieFromString(value);
                        this.Cookies.Set(cookie);
                        cookie.Changed = false;
                        cookie.Added = false;
                    }
                }
                finally
                {
                    this.HeadersWritten = headersWritten;
                }
            }
        }

        internal void SynchronizeStatus(int statusCode, int subStatusCode, string description)
        {
            this._statusCode = statusCode;
            this._subStatusCode = subStatusCode;
            this._statusDescription = description;
        }

        internal void SyncStatusIntegrated()
        {
            if (!this._headersWritten && this._statusSet)
            {
                this._wr.SendStatus(this._statusCode, this._subStatusCode, this.StatusDescription);
                this._statusSet = false;
            }
        }

        public void TransmitFile(string filename)
        {
            this.TransmitFile(filename, 0L, -1L);
        }

        public void TransmitFile(string filename, long offset, long length)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            if (offset < 0L)
            {
                throw new ArgumentException(System.Web.SR.GetString("Invalid_range"), "offset");
            }
            if (length < -1L)
            {
                throw new ArgumentException(System.Web.SR.GetString("Invalid_range"), "length");
            }
            filename = this.GetNormalizedFilename(filename);
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long num = stream.Length;
                if (length == -1L)
                {
                    length = num - offset;
                }
                if (num < offset)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Invalid_range"), "offset");
                }
                if ((num - offset) < length)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Invalid_range"), "length");
                }
                if (!this.UsingHttpWriter)
                {
                    this.WriteStreamAsText(stream, offset, length);
                    return;
                }
            }
            if (length > 0L)
            {
                bool supportsLongTransmitFile = (this._wr != null) && this._wr.SupportsLongTransmitFile;
                this._httpWriter.TransmitFile(filename, offset, length, this._context.IsClientImpersonationConfigured || HttpRuntime.IsOnUNCShareInternal, supportsLongTransmitFile);
            }
        }

        internal void UpdateNativeResponse(bool sendHeaders)
        {
            IIS7WorkerRequest wr = this._wr as IIS7WorkerRequest;
            if (wr != null)
            {
                if (((this._suppressContent && (this.Request != null)) && (this.Request.HttpVerb != HttpVerb.HEAD)) || this._ended)
                {
                    this.Clear();
                }
                bool flag = false;
                long bufferedLength = this._httpWriter.GetBufferedLength();
                if (!this._headersWritten)
                {
                    if (this.UseAdaptiveError)
                    {
                        int statusCode = this.StatusCode;
                        if ((statusCode >= 400) && (statusCode < 600))
                        {
                            this.StatusCode = 200;
                        }
                    }
                    if (this._statusSet)
                    {
                        this._wr.SendStatus(this.StatusCode, this.SubStatusCode, this.StatusDescription);
                        this._statusSet = false;
                    }
                    if (!this._suppressHeaders && !this._clientDisconnected)
                    {
                        if ((this._redirectLocation != null) && this._redirectLocationSet)
                        {
                            (this.Headers as HttpHeaderCollection).Set("Location", this._redirectLocation);
                            this._redirectLocationSet = false;
                        }
                        bool flag2 = (bufferedLength > 0L) || wr.IsResponseBuffered();
                        if ((this._contentType != null) && (this._contentTypeSetByManagedCaller || (this._contentTypeSetByManagedHandler && flag2)))
                        {
                            HttpHeaderCollection headers = this.Headers as HttpHeaderCollection;
                            string str = this.AppendCharSetToContentType(this._contentType);
                            headers.Set("Content-Type", str);
                        }
                        this.GenerateResponseHeadersForCookies();
                        if (sendHeaders)
                        {
                            if (this._cachePolicy != null)
                            {
                                if ((this._cookies != null) && (this._cookies.Count != 0))
                                {
                                    this._cachePolicy.SetHasSetCookieHeader();
                                    this.DisableKernelCache();
                                }
                                if (this._cachePolicy.IsModified())
                                {
                                    ArrayList list = new ArrayList();
                                    this._cachePolicy.GetHeaders(list, this);
                                    HttpHeaderCollection headers3 = this.Headers as HttpHeaderCollection;
                                    foreach (HttpResponseHeader header in list)
                                    {
                                        headers3.Set(header.Name, header.Value);
                                    }
                                }
                            }
                            flag = true;
                        }
                    }
                }
                if (this._flushing && !this._filteringCompleted)
                {
                    this._httpWriter.FilterIntegrated(false, wr);
                    bufferedLength = this._httpWriter.GetBufferedLength();
                }
                if ((!this._clientDisconnected && ((bufferedLength > 0L) || flag)) && ((bufferedLength != 0L) || !this._httpWriter.IgnoringFurtherWrites))
                {
                    this._httpWriter.Send(this._wr);
                    wr.PushResponseToNative();
                    this._httpWriter.DisposeIntegratedBuffers();
                }
            }
        }

        private string UrlEncodeRedirect(string url)
        {
            int index = url.IndexOf('?');
            if (index >= 0)
            {
                Encoding e = (this.Request != null) ? this.Request.ContentEncoding : this.ContentEncoding;
                url = HttpEncoderUtility.UrlEncodeSpaces(HttpUtility.UrlEncodeNonAscii(url.Substring(0, index), Encoding.UTF8)) + HttpUtility.UrlEncodeNonAscii(url.Substring(index), e);
                return url;
            }
            url = HttpEncoderUtility.UrlEncodeSpaces(HttpUtility.UrlEncodeNonAscii(url, Encoding.UTF8));
            return url;
        }

        internal void UseSnapshot(HttpRawResponse rawResponse, bool sendBody)
        {
            if (this._headersWritten)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_use_snapshot_after_headers_sent"));
            }
            if (this._httpWriter == null)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_use_snapshot_for_TextWriter"));
            }
            this.ClearAll();
            this.StatusCode = rawResponse.StatusCode;
            this.StatusDescription = rawResponse.StatusDescription;
            ArrayList headers = rawResponse.Headers;
            int num = (headers != null) ? headers.Count : 0;
            for (int i = 0; i < num; i++)
            {
                HttpResponseHeader header = (HttpResponseHeader) headers[i];
                this.AppendHeader(header.Name, header.Value);
            }
            this._httpWriter.UseSnapshot(rawResponse.Buffers);
            this._suppressContent = !sendBody;
        }

        private void ValidateFileRange(string filename, long offset, long length)
        {
            FileStream stream = null;
            try
            {
                stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                long num = stream.Length;
                if (length == -1L)
                {
                    length = num - offset;
                }
                if ((offset < 0L) || (length > (num - offset)))
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_range"));
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        public void Write(char ch)
        {
            this._writer.Write(ch);
        }

        public void Write(object obj)
        {
            this._writer.Write(obj);
        }

        public void Write(string s)
        {
            this._writer.Write(s);
        }

        public void Write(char[] buffer, int index, int count)
        {
            this._writer.Write(buffer, index, count);
        }

        private void WriteErrorMessage(Exception e, bool dontShowSensitiveErrors)
        {
            ErrorFormatter errorFormatter = null;
            CultureInfo dynamicUICulture = null;
            CultureInfo currentUICulture = null;
            bool flag = false;
            if (this._context.DynamicUICulture != null)
            {
                dynamicUICulture = this._context.DynamicUICulture;
            }
            else
            {
                GlobalizationSection globalization = RuntimeConfig.GetLKGConfig(this._context).Globalization;
                if ((globalization != null) && !string.IsNullOrEmpty(globalization.UICulture))
                {
                    try
                    {
                        dynamicUICulture = HttpServerUtility.CreateReadOnlyCultureInfo(globalization.UICulture);
                    }
                    catch
                    {
                    }
                }
            }
            this.GenerateResponseHeadersForHandler();
            if (dynamicUICulture != null)
            {
                currentUICulture = Thread.CurrentThread.CurrentUICulture;
                Thread.CurrentThread.CurrentUICulture = dynamicUICulture;
                flag = true;
            }
            try
            {
                try
                {
                    errorFormatter = this.GetErrorFormatter(e);
                    if (dontShowSensitiveErrors && !errorFormatter.CanBeShownToAllUsers)
                    {
                        errorFormatter = new GenericApplicationErrorFormatter(this.Request.IsLocal);
                    }
                    if (ErrorFormatter.RequiresAdaptiveErrorReporting(this.Context))
                    {
                        this._writer.Write(errorFormatter.GetAdaptiveErrorMessage(this.Context, dontShowSensitiveErrors));
                    }
                    else
                    {
                        this._writer.Write(errorFormatter.GetHtmlErrorMessage(dontShowSensitiveErrors));
                        if (!dontShowSensitiveErrors && HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium))
                        {
                            this._writer.Write("<!-- \r\n");
                            this.WriteExceptionStack(e);
                            this._writer.Write("-->");
                        }
                        if (!dontShowSensitiveErrors && !this.Request.IsLocal)
                        {
                            this._writer.Write("<!-- \r\n");
                            this._writer.Write(System.Web.SR.GetString("Information_Disclosure_Warning"));
                            this._writer.Write("-->");
                        }
                    }
                    if (this._closeConnectionAfterError)
                    {
                        this.Flush();
                        this.Close();
                    }
                }
                finally
                {
                    if (flag)
                    {
                        Thread.CurrentThread.CurrentUICulture = currentUICulture;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void WriteExceptionStack(Exception e)
        {
            ConfigurationErrorsException exception = e as ConfigurationErrorsException;
            if (exception == null)
            {
                this.WriteOneExceptionStack(e);
            }
            else
            {
                this.WriteOneExceptionStack(e);
                ICollection errors = exception.Errors;
                if (errors.Count > 1)
                {
                    bool flag = false;
                    foreach (ConfigurationException exception2 in errors)
                    {
                        if (!flag)
                        {
                            flag = true;
                        }
                        else
                        {
                            this._writer.WriteLine("---");
                            this.WriteOneExceptionStack(exception2);
                        }
                    }
                }
            }
        }

        public void WriteFile(string filename)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            this.WriteFile(filename, false);
        }

        public void WriteFile(string filename, bool readIntoMemory)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            filename = this.GetNormalizedFilename(filename);
            FileStream f = null;
            try
            {
                f = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (this.UsingHttpWriter)
                {
                    long length = f.Length;
                    if (length > 0L)
                    {
                        if (readIntoMemory)
                        {
                            byte[] buffer = new byte[(int) length];
                            int count = f.Read(buffer, 0, (int) length);
                            this._httpWriter.WriteBytes(buffer, 0, count);
                        }
                        else
                        {
                            f.Close();
                            f = null;
                            this._httpWriter.WriteFile(filename, 0L, length);
                        }
                    }
                }
                else
                {
                    this.WriteStreamAsText(f, 0L, -1L);
                }
            }
            finally
            {
                if (f != null)
                {
                    f.Close();
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public void WriteFile(IntPtr fileHandle, long offset, long size)
        {
            if (size > 0L)
            {
                FileStream f = null;
                try
                {
                    f = new FileStream(new SafeFileHandle(fileHandle, false), FileAccess.Read);
                    if (this.UsingHttpWriter)
                    {
                        long length = f.Length;
                        if (size == -1L)
                        {
                            size = length - offset;
                        }
                        if ((offset < 0L) || (size > (length - offset)))
                        {
                            throw new HttpException(System.Web.SR.GetString("Invalid_range"));
                        }
                        if (offset > 0L)
                        {
                            f.Seek(offset, SeekOrigin.Begin);
                        }
                        byte[] buffer = new byte[(int) size];
                        int count = f.Read(buffer, 0, (int) size);
                        this._httpWriter.WriteBytes(buffer, 0, count);
                    }
                    else
                    {
                        this.WriteStreamAsText(f, offset, size);
                    }
                }
                finally
                {
                    if (f != null)
                    {
                        f.Close();
                    }
                }
            }
        }

        public void WriteFile(string filename, long offset, long size)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            if (size != 0L)
            {
                filename = this.GetNormalizedFilename(filename);
                this.ValidateFileRange(filename, offset, size);
                if (this.UsingHttpWriter)
                {
                    InternalSecurityPermissions.FileReadAccess(filename).Demand();
                    this._httpWriter.WriteFile(filename, offset, size);
                }
                else
                {
                    FileStream f = null;
                    try
                    {
                        f = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                        this.WriteStreamAsText(f, offset, size);
                    }
                    finally
                    {
                        if (f != null)
                        {
                            f.Close();
                        }
                    }
                }
            }
        }

        private void WriteHeaders()
        {
            if (this._wr != null)
            {
                if ((this._context != null) && (this._context.ApplicationInstance != null))
                {
                    this._context.ApplicationInstance.RaiseOnPreSendRequestHeaders();
                }
                if (this.UseAdaptiveError)
                {
                    int statusCode = this.StatusCode;
                    if ((statusCode >= 400) && (statusCode < 600))
                    {
                        this.StatusCode = 200;
                    }
                }
                ArrayList list = this.GenerateResponseHeaders(false);
                this._wr.SendStatus(this.StatusCode, this.StatusDescription);
                this._wr.SetHeaderEncoding(this.HeaderEncoding);
                int num2 = (list != null) ? list.Count : 0;
                for (int i = 0; i < num2; i++)
                {
                    (list[i] as HttpResponseHeader).Send(this._wr);
                }
            }
        }

        private void WriteOneExceptionStack(Exception e)
        {
            Exception innerException = e.InnerException;
            if (innerException != null)
            {
                this.WriteOneExceptionStack(innerException);
            }
            string str = "[" + e.GetType().Name + "]";
            if ((e.Message != null) && (e.Message.Length > 0))
            {
                str = str + ": " + HttpUtility.HtmlEncode(e.Message);
            }
            this._writer.WriteLine(str);
            if (e.StackTrace != null)
            {
                this._writer.WriteLine(e.StackTrace);
            }
        }

        private void WriteStreamAsText(Stream f, long offset, long size)
        {
            if (size < 0L)
            {
                size = f.Length - offset;
            }
            if (size > 0L)
            {
                if (offset > 0L)
                {
                    f.Seek(offset, SeekOrigin.Begin);
                }
                byte[] buffer = new byte[(int) size];
                int count = f.Read(buffer, 0, (int) size);
                this._writer.Write(Encoding.Default.GetChars(buffer, 0, count));
            }
        }

        public void WriteSubstitution(HttpResponseSubstitutionCallback callback)
        {
            if ((callback.Target != null) && (callback.Target is Control))
            {
                throw new ArgumentException(System.Web.SR.GetString("Invalid_substitution_callback"), "callback");
            }
            if (this.UsingHttpWriter)
            {
                this._httpWriter.WriteSubstBlock(callback, this._wr as IIS7WorkerRequest);
            }
            else
            {
                this._writer.Write(callback(this._context));
            }
            if ((this._cachePolicy != null) && (this._cachePolicy.GetCacheability() == HttpCacheability.Public))
            {
                this._cachePolicy.SetCacheability(HttpCacheability.Server);
            }
        }

        internal void WriteVirtualFile(VirtualFile vf)
        {
            using (Stream stream = vf.Open())
            {
                if (this.UsingHttpWriter)
                {
                    long length = stream.Length;
                    if (length > 0L)
                    {
                        byte[] buffer = new byte[(int) length];
                        int count = stream.Read(buffer, 0, (int) length);
                        this._httpWriter.WriteBytes(buffer, 0, count);
                    }
                }
                else
                {
                    this.WriteStreamAsText(stream, 0L, -1L);
                }
            }
        }

        public bool Buffer
        {
            get
            {
                return this.BufferOutput;
            }
            set
            {
                this.BufferOutput = value;
            }
        }

        public bool BufferOutput
        {
            get
            {
                return this._bufferOutput;
            }
            set
            {
                if (this._bufferOutput != value)
                {
                    this._bufferOutput = value;
                    if (this._httpWriter != null)
                    {
                        this._httpWriter.UpdateResponseBuffering();
                    }
                }
            }
        }

        public HttpCachePolicy Cache
        {
            get
            {
                if (this._cachePolicy == null)
                {
                    this._cachePolicy = new HttpCachePolicy();
                }
                return this._cachePolicy;
            }
        }

        public string CacheControl
        {
            get
            {
                if (this._cacheControl == null)
                {
                    return "private";
                }
                return this._cacheControl;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this._cacheControl = null;
                    this.Cache.SetCacheability(HttpCacheability.NoCache);
                }
                else if (System.Web.Util.StringUtil.EqualsIgnoreCase(value, "private"))
                {
                    this._cacheControl = value;
                    this.Cache.SetCacheability(HttpCacheability.Private);
                }
                else if (System.Web.Util.StringUtil.EqualsIgnoreCase(value, "public"))
                {
                    this._cacheControl = value;
                    this.Cache.SetCacheability(HttpCacheability.Public);
                }
                else
                {
                    if (!System.Web.Util.StringUtil.EqualsIgnoreCase(value, "no-cache"))
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Invalid_value_for_CacheControl", new object[] { value }));
                    }
                    this._cacheControl = value;
                    this.Cache.SetCacheability(HttpCacheability.NoCache);
                }
            }
        }

        internal bool CanExecuteUrlForEntireResponse
        {
            get
            {
                if (this._headersWritten)
                {
                    return false;
                }
                if ((this._wr == null) || !this._wr.SupportsExecuteUrl)
                {
                    return false;
                }
                if (!this.UsingHttpWriter)
                {
                    return false;
                }
                if (this._httpWriter.GetBufferedLength() != 0L)
                {
                    return false;
                }
                if (this._httpWriter.FilterInstalled)
                {
                    return false;
                }
                if ((this._cachePolicy != null) && this._cachePolicy.IsModified())
                {
                    return false;
                }
                return true;
            }
        }

        public string Charset
        {
            get
            {
                if (this._charSet == null)
                {
                    this._charSet = this.ContentEncoding.WebName;
                }
                return this._charSet;
            }
            set
            {
                if (this._headersWritten)
                {
                    throw new HttpException(System.Web.SR.GetString("Cannot_set_content_type_after_headers_sent"));
                }
                if (value != null)
                {
                    this._charSet = value;
                }
                else
                {
                    this._charSet = string.Empty;
                }
                this._customCharSet = true;
            }
        }

        internal System.Text.Encoder ContentEncoder
        {
            get
            {
                if (this._encoder == null)
                {
                    Encoding contentEncoding = this.ContentEncoding;
                    this._encoder = contentEncoding.GetEncoder();
                    if (!contentEncoding.Equals(Encoding.UTF8))
                    {
                        bool enableBestFitResponseEncoding = false;
                        GlobalizationSection globalization = RuntimeConfig.GetLKGConfig(this._context).Globalization;
                        if (globalization != null)
                        {
                            enableBestFitResponseEncoding = globalization.EnableBestFitResponseEncoding;
                        }
                        if (!enableBestFitResponseEncoding)
                        {
                            this._encoder.Fallback = new EncoderReplacementFallback();
                        }
                    }
                }
                return this._encoder;
            }
        }

        public Encoding ContentEncoding
        {
            get
            {
                if (this._encoding == null)
                {
                    GlobalizationSection globalization = RuntimeConfig.GetLKGConfig(this._context).Globalization;
                    if (globalization != null)
                    {
                        this._encoding = globalization.ResponseEncoding;
                    }
                    if (this._encoding == null)
                    {
                        this._encoding = Encoding.Default;
                    }
                }
                return this._encoding;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if ((this._encoding == null) || !this._encoding.Equals(value))
                {
                    this._encoding = value;
                    this._encoder = null;
                    if (this._httpWriter != null)
                    {
                        this._httpWriter.UpdateResponseEncoding();
                    }
                }
            }
        }

        public string ContentType
        {
            get
            {
                return this._contentType;
            }
            set
            {
                if (this._headersWritten)
                {
                    if (this._contentType != value)
                    {
                        throw new HttpException(System.Web.SR.GetString("Cannot_set_content_type_after_headers_sent"));
                    }
                }
                else
                {
                    this._contentTypeSetByManagedCaller = true;
                    this._contentType = value;
                }
            }
        }

        internal HttpContext Context
        {
            get
            {
                return this._context;
            }
            set
            {
                this._context = value;
            }
        }

        public HttpCookieCollection Cookies
        {
            get
            {
                if (this._cookies == null)
                {
                    this._cookies = new HttpCookieCollection(this, false);
                }
                return this._cookies;
            }
        }

        public int Expires
        {
            get
            {
                return this._expiresInMinutes;
            }
            set
            {
                if (!this._expiresInMinutesSet || (value < this._expiresInMinutes))
                {
                    this._expiresInMinutes = value;
                    this.Cache.SetExpires(this._context.Timestamp + new TimeSpan(0, this._expiresInMinutes, 0));
                }
            }
        }

        public DateTime ExpiresAbsolute
        {
            get
            {
                return this._expiresAbsolute;
            }
            set
            {
                if (!this._expiresAbsoluteSet || (value < this._expiresAbsolute))
                {
                    this._expiresAbsolute = value;
                    this.Cache.SetExpires(this._expiresAbsolute);
                }
            }
        }

        public Stream Filter
        {
            get
            {
                if (this.UsingHttpWriter)
                {
                    return this._httpWriter.GetCurrentFilter();
                }
                return null;
            }
            set
            {
                if (!this.UsingHttpWriter)
                {
                    throw new HttpException(System.Web.SR.GetString("Filtering_not_allowed"));
                }
                this._httpWriter.InstallFilter(value);
                IIS7WorkerRequest request = this._wr as IIS7WorkerRequest;
                if (request != null)
                {
                    request.ResponseFilterInstalled();
                }
            }
        }

        internal bool HasCachePolicy
        {
            get
            {
                return (this._cachePolicy != null);
            }
        }

        public Encoding HeaderEncoding
        {
            get
            {
                if (this._headerEncoding == null)
                {
                    GlobalizationSection globalization = RuntimeConfig.GetLKGConfig(this._context).Globalization;
                    if (globalization != null)
                    {
                        this._headerEncoding = globalization.ResponseHeaderEncoding;
                    }
                    if ((this._headerEncoding == null) || this._headerEncoding.Equals(Encoding.Unicode))
                    {
                        this._headerEncoding = Encoding.UTF8;
                    }
                }
                return this._headerEncoding;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Equals(Encoding.Unicode))
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_header_encoding", new object[] { value.WebName }));
                }
                if ((this._headerEncoding == null) || !this._headerEncoding.Equals(value))
                {
                    if (this._headersWritten)
                    {
                        throw new HttpException(System.Web.SR.GetString("Cannot_set_header_encoding_after_headers_sent"));
                    }
                    this._headerEncoding = value;
                }
            }
        }

        public NameValueCollection Headers
        {
            get
            {
                if (!(this._wr is IIS7WorkerRequest))
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
                }
                if (this._headers == null)
                {
                    this._headers = new HttpHeaderCollection(this._wr, this, 0x10);
                }
                return this._headers;
            }
        }

        internal bool HeadersWritten
        {
            get
            {
                return this._headersWritten;
            }
            set
            {
                this._headersWritten = value;
            }
        }

        public bool IsClientConnected
        {
            get
            {
                if (this._clientDisconnected)
                {
                    return false;
                }
                if ((this._wr != null) && !this._wr.IsClientConnected())
                {
                    this._clientDisconnected = true;
                    return false;
                }
                return true;
            }
        }

        public bool IsRequestBeingRedirected
        {
            get
            {
                return this._isRequestBeingRedirected;
            }
        }

        public TextWriter Output
        {
            get
            {
                return this._writer;
            }
            set
            {
                this._writer = value;
            }
        }

        public Stream OutputStream
        {
            get
            {
                if (!this.UsingHttpWriter)
                {
                    throw new HttpException(System.Web.SR.GetString("OutputStream_NotAvail"));
                }
                return this._httpWriter.OutputStream;
            }
        }

        public string RedirectLocation
        {
            get
            {
                return this._redirectLocation;
            }
            set
            {
                if (this._headersWritten)
                {
                    throw new HttpException(System.Web.SR.GetString("Cannot_append_header_after_headers_sent"));
                }
                this._redirectLocation = value;
                this._redirectLocationSet = true;
            }
        }

        internal HttpRequest Request
        {
            get
            {
                if (this._context == null)
                {
                    return null;
                }
                return this._context.Request;
            }
        }

        public string Status
        {
            get
            {
                return (this.StatusCode.ToString(NumberFormatInfo.InvariantInfo) + " " + this.StatusDescription);
            }
            set
            {
                int num = 200;
                string str = "OK";
                try
                {
                    int index = value.IndexOf(' ');
                    num = int.Parse(value.Substring(0, index), CultureInfo.InvariantCulture);
                    str = value.Substring(index + 1);
                }
                catch
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_status_string"));
                }
                this.StatusCode = num;
                this.StatusDescription = str;
            }
        }

        public int StatusCode
        {
            get
            {
                return this._statusCode;
            }
            set
            {
                if (this._headersWritten)
                {
                    throw new HttpException(System.Web.SR.GetString("Cannot_set_status_after_headers_sent"));
                }
                if (this._statusCode != value)
                {
                    this._statusCode = value;
                    this._subStatusCode = 0;
                    this._statusDescription = null;
                    this._statusSet = true;
                }
            }
        }

        public string StatusDescription
        {
            get
            {
                if (this._statusDescription == null)
                {
                    this._statusDescription = HttpWorkerRequest.GetStatusDescription(this._statusCode);
                }
                return this._statusDescription;
            }
            set
            {
                if (this._headersWritten)
                {
                    throw new HttpException(System.Web.SR.GetString("Cannot_set_status_after_headers_sent"));
                }
                if ((value != null) && (value.Length > 0x200))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._statusDescription = value;
                this._statusSet = true;
            }
        }

        public int SubStatusCode
        {
            get
            {
                if (!(this._wr is IIS7WorkerRequest))
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
                }
                return this._subStatusCode;
            }
            set
            {
                if (!(this._wr is IIS7WorkerRequest))
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
                }
                if (this._headersWritten)
                {
                    throw new HttpException(System.Web.SR.GetString("Cannot_set_status_after_headers_sent"));
                }
                this._subStatusCode = value;
                this._statusSet = true;
            }
        }

        public bool SuppressContent
        {
            get
            {
                return this._suppressContent;
            }
            set
            {
                this._suppressContent = value;
                this._suppressContentSet = true;
            }
        }

        public bool TrySkipIisCustomErrors
        {
            get
            {
                return ((this._wr != null) && this._wr.TrySkipIisCustomErrors);
            }
            set
            {
                if (this._wr != null)
                {
                    this._wr.TrySkipIisCustomErrors = value;
                }
            }
        }

        internal bool UseAdaptiveError
        {
            get
            {
                return this._useAdaptiveError;
            }
            set
            {
                this._useAdaptiveError = value;
            }
        }

        private bool UsingHttpWriter
        {
            get
            {
                return ((this._httpWriter != null) && (this._writer == this._httpWriter));
            }
        }
    }
}

