namespace System.Web.Hosting
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Security;
    using System.Web.Util;

    internal sealed class IIS7WorkerRequest : HttpWorkerRequest
    {
        private string _appPath;
        private string _appPathTranslated;
        private ArrayList _cachedResponseBodyBytes;
        private int _cachedResponseBodyLength;
        private string _cacheUrl;
        private ChannelBinding _channelBindingToken;
        private byte[] _clientCert;
        private byte[] _clientCertBinaryIssuer;
        private int _clientCertEncoding;
        private bool _clientCertFetched;
        private byte[] _clientCertPublicKey;
        private DateTime _clientCertValidFrom;
        private DateTime _clientCertValidUntil;
        private bool _connectionClosed;
        private int _contentTotalLength;
        private int _contentType;
        private IntPtr _context;
        private bool _disconnected;
        private string _filePath;
        private Encoding _headerEncoding = Encoding.UTF8;
        private bool _headersSent;
        private string[] _knownRequestHeaders;
        private string _path;
        private string _pathInfo;
        private string _pathTranslated;
        private int _preloadedLength;
        private bool _preloadedLengthRead;
        private string _queryString;
        private bool _rebaseClientPath;
        private bool _requestHeadersAvailable;
        private bool _rewriteNotifyDisabled;
        private bool _traceEnabled;
        private Guid _traceId;
        private bool _trySkipIisCustomErrors;
        private string[][] _unknownRequestHeaders;
        private const int CONTENT_FORM = 1;
        private const int CONTENT_MULTIPART = 2;
        private const int CONTENT_NONE = 0;
        private const int CONTENT_OTHER = 3;
        private const int IisHeaderTranslate = 0x27;
        private const string IisHeaderTranslateName = "Translate";
        private const int IisHeaderUserAgent = 40;
        private const int IisRequestHeaderMaximum = 0x29;
        private const int MIN_ASYNC_SIZE = 0x800;
        private static readonly char[] s_ColonOrNL = new char[] { ':', '\n' };

        internal IIS7WorkerRequest(IntPtr requestContext, bool etwProviderEnabled)
        {
            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_TOTAL);
            if (IntPtr.Zero == requestContext)
            {
                throw new ArgumentNullException("requestContext");
            }
            this._context = requestContext;
            this._traceEnabled = etwProviderEnabled;
            if (this._traceEnabled)
            {
                EtwTrace.TraceEnableCheck(EtwTraceConfigType.IIS7_INTEGRATED, requestContext);
                UnsafeIISMethods.MgdGetRequestTraceGuid(this._context, out this._traceId);
                if (EtwTrace.IsTraceEnabled(5, 1))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_APPDOMAIN_ENTER, this, Thread.GetDomain().FriendlyName);
                }
            }
        }

        private void AddBodyToCachedResponse(MemoryBytes bytes)
        {
            if (this._cachedResponseBodyBytes == null)
            {
                this._cachedResponseBodyBytes = new ArrayList();
            }
            this._cachedResponseBodyBytes.Add(bytes);
            this._cachedResponseBodyLength += bytes.Size;
        }

        internal IntPtr AllocateRequestMemory(int size)
        {
            if (size > 0)
            {
                return UnsafeIISMethods.MgdAllocateRequestMemory(this._context, size);
            }
            return IntPtr.Zero;
        }

        internal void ClearResponse(bool clearEntity, bool clearHeaders)
        {
            UnsafeIISMethods.MgdClearResponse(this._context, clearEntity, clearHeaders);
        }

        public override void CloseConnection()
        {
            UnsafeIISMethods.MgdCloseConnection(this._context);
            this._connectionClosed = true;
        }

        internal static IIS7WorkerRequest CreateWorkerRequest(IntPtr requestContext, bool etwProviderEnabled)
        {
            IIS7WorkerRequest request = new IIS7WorkerRequest(requestContext, etwProviderEnabled);
            if (request != null)
            {
                request.Initialize();
            }
            return request;
        }

        internal void DisableIISCache()
        {
            UnsafeIISMethods.MgdDisableKernelCache(this._context);
            UnsafeIISMethods.MgdDisableUserCache(this._context);
        }

        internal override void DisableKernelCache()
        {
            UnsafeIISMethods.MgdDisableKernelCache(this._context);
        }

        internal void DisableNotifications(RequestNotification notifications, RequestNotification postNotifications)
        {
            UnsafeIISMethods.MgdDisableNotifications(this._context, notifications, postNotifications);
        }

        internal void Dispose()
        {
            this._context = IntPtr.Zero;
            if ((this._channelBindingToken != null) && !this._channelBindingToken.IsInvalid)
            {
                this._channelBindingToken.Dispose();
            }
        }

        public override void EndOfRequest()
        {
        }

        internal void ExplicitFlush()
        {
            int result = UnsafeIISMethods.MgdExplicitFlush(this._context);
            if (result < 0)
            {
                this.RaiseCommunicationError(result, true);
            }
            this._headersSent = true;
        }

        private void FetchClientCertificate()
        {
            if (!this._clientCertFetched)
            {
                IntPtr ptr;
                int num;
                IntPtr ptr2;
                int num2;
                IntPtr ptr3;
                int num3;
                uint num4;
                long num5;
                long num6;
                this._clientCertFetched = true;
                Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetClientCertificate(this._context, out ptr, out num, out ptr2, out num2, out ptr3, out num3, out num4, out num5, out num6));
                this._clientCertEncoding = (int) num4;
                if (num > 0)
                {
                    this._clientCert = new byte[num];
                    Misc.CopyMemory(ptr, 0, this._clientCert, 0, num);
                }
                if (num2 > 0)
                {
                    this._clientCertBinaryIssuer = new byte[num2];
                    Misc.CopyMemory(ptr2, 0, this._clientCertBinaryIssuer, 0, num2);
                }
                if (num3 > 0)
                {
                    this._clientCertPublicKey = new byte[num3];
                    Misc.CopyMemory(ptr3, 0, this._clientCertPublicKey, 0, num3);
                }
                this._clientCertValidFrom = (num5 != 0L) ? DateTime.FromFileTime(num5) : DateTime.Now;
                this._clientCertValidUntil = (num6 != 0L) ? DateTime.FromFileTime(num6) : DateTime.Now;
            }
        }

        private void FlushCachedResponse(bool isFinal)
        {
            if (!this._connectionClosed && (this._context != IntPtr.Zero))
            {
                int minimumLength = 0;
                IntPtr[] bodyFragments = null;
                int[] bodyFragmentLengths = null;
                long num2 = 0L;
                int[] bodyFragmentTypes = null;
                try
                {
                    if (this._cachedResponseBodyLength > 0)
                    {
                        minimumLength = this._cachedResponseBodyBytes.Count;
                        bodyFragments = RecyclableArrayHelper.GetIntPtrArray(minimumLength);
                        bodyFragmentLengths = RecyclableArrayHelper.GetIntegerArray(minimumLength);
                        bodyFragmentTypes = RecyclableArrayHelper.GetIntegerArray(minimumLength);
                        for (int i = 0; i < minimumLength; i++)
                        {
                            MemoryBytes bytes = (MemoryBytes) this._cachedResponseBodyBytes[i];
                            bodyFragments[i] = bytes.LockMemory();
                            bodyFragmentTypes[i] = (int) bytes.BufferType;
                            bodyFragmentLengths[i] = bytes.Size;
                            if (bytes.UseTransmitFile)
                            {
                                num2 += bytes.FileSize;
                            }
                            else
                            {
                                num2 += bytes.Size;
                            }
                        }
                    }
                    int delta = (int) num2;
                    if (delta > 0)
                    {
                        PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_OUT, delta);
                    }
                    this.FlushCore(true, minimumLength, bodyFragments, bodyFragmentLengths, bodyFragmentTypes);
                }
                finally
                {
                    this.UnlockCachedResponseBytes();
                    RecyclableArrayHelper.ReuseIntPtrArray(bodyFragments);
                    RecyclableArrayHelper.ReuseIntegerArray(bodyFragmentLengths);
                    RecyclableArrayHelper.ReuseIntegerArray(bodyFragmentTypes);
                }
            }
        }

        private void FlushCore(bool keepConnected, int numBodyFragments, IntPtr[] bodyFragments, int[] bodyFragmentLengths, int[] bodyFragmentTypes)
        {
            if (!this._connectionClosed && (this._context != IntPtr.Zero))
            {
                int result = UnsafeIISMethods.MgdFlushCore(this._context, keepConnected, numBodyFragments, bodyFragments, bodyFragmentLengths, bodyFragmentTypes);
                if (result < 0)
                {
                    this.RaiseCommunicationError(result, false);
                }
            }
        }

        public override void FlushResponse(bool finalFlush)
        {
            if (!this._connectionClosed)
            {
                this.FlushCachedResponse(finalFlush);
            }
        }

        public override string GetAppPath()
        {
            return this._appPath;
        }

        public override string GetAppPathTranslated()
        {
            return this._appPathTranslated;
        }

        internal ArrayList GetBufferedResponseChunks(bool disableRecycling, ArrayList substElements, ref bool hasSubstBlocks)
        {
            int minimumLength = 0x20;
            IntPtr[] intPtrArray = RecyclableArrayHelper.GetIntPtrArray(minimumLength);
            int[] integerArray = RecyclableArrayHelper.GetIntegerArray(minimumLength);
            int[] fragmentChunkType = RecyclableArrayHelper.GetIntegerArray(minimumLength);
            int hresult = UnsafeIISMethods.MgdGetResponseChunks(this._context, ref minimumLength, intPtrArray, integerArray, fragmentChunkType);
            if (hresult < 0)
            {
                if (hresult == -2147024774)
                {
                    RecyclableArrayHelper.ReuseIntPtrArray(intPtrArray);
                    RecyclableArrayHelper.ReuseIntegerArray(integerArray);
                    RecyclableArrayHelper.ReuseIntegerArray(fragmentChunkType);
                    intPtrArray = RecyclableArrayHelper.GetIntPtrArray(minimumLength);
                    integerArray = RecyclableArrayHelper.GetIntegerArray(minimumLength);
                    fragmentChunkType = RecyclableArrayHelper.GetIntegerArray(minimumLength);
                    hresult = UnsafeIISMethods.MgdGetResponseChunks(this._context, ref minimumLength, intPtrArray, integerArray, fragmentChunkType);
                }
                if (hresult == -2147024883)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Invalid_http_data_chunk"));
                }
                Misc.ThrowIfFailedHr(hresult);
            }
            ArrayList list = new ArrayList();
            HttpResponseUnmanagedBufferElement element = null;
            HttpSubstBlockResponseElement[] elementArray = null;
            if (substElements != null)
            {
                elementArray = (HttpSubstBlockResponseElement[]) substElements.ToArray(typeof(HttpSubstBlockResponseElement));
            }
            int size = 0;
            for (int i = 0; i < minimumLength; i++)
            {
                if (fragmentChunkType[i] == 0)
                {
                    if (elementArray != null)
                    {
                        int index = -1;
                        for (int j = 0; j < elementArray.Length; j++)
                        {
                            if (elementArray[j].PointerEquals(intPtrArray[i]))
                            {
                                index = j;
                                break;
                            }
                        }
                        if (index != -1)
                        {
                            if (element != null)
                            {
                                list.Add(element);
                                element = null;
                            }
                            list.Add(elementArray[index]);
                            hasSubstBlocks = true;
                            continue;
                        }
                    }
                    if (element == null)
                    {
                        element = new HttpResponseUnmanagedBufferElement();
                        if (disableRecycling)
                        {
                            element.DisableRecycling();
                        }
                    }
                    size = integerArray[i];
                    if (size <= element.FreeBytes)
                    {
                        element.Append(intPtrArray[i], 0, size);
                    }
                    else
                    {
                        int offset = 0;
                        do
                        {
                            int num8 = element.Append(intPtrArray[i], offset, size);
                            size -= num8;
                            offset += num8;
                            if (element.FreeBytes == 0)
                            {
                                list.Add(element);
                                element = new HttpResponseUnmanagedBufferElement();
                                if (disableRecycling)
                                {
                                    element.DisableRecycling();
                                }
                            }
                        }
                        while (size > 0);
                    }
                    if (element.FreeBytes == 0)
                    {
                        list.Add(element);
                        element = null;
                    }
                    continue;
                }
                if (fragmentChunkType[i] == 1)
                {
                    long num9 = 0L;
                    long length = 0L;
                    Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetFileChunkInfo(this._context, i, out num9, out length));
                    while ((length > 0L) && (num9 >= 0L))
                    {
                        if ((element == null) || (element.FreeBytes == 0))
                        {
                            if (element != null)
                            {
                                list.Add(element);
                            }
                            element = new HttpResponseUnmanagedBufferElement();
                            if (disableRecycling)
                            {
                                element.DisableRecycling();
                            }
                        }
                        int freeBytes = element.FreeBytes;
                        if (element.FreeBytes > length)
                        {
                            freeBytes = (int) length;
                        }
                        Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdReadChunkHandle(this._context, intPtrArray[i], num9, ref freeBytes, element.FreeLocation));
                        element.AdjustSize(freeBytes);
                        length -= freeBytes;
                        num9 += freeBytes;
                    }
                }
            }
            if (element != null)
            {
                list.Add(element);
            }
            RecyclableArrayHelper.ReuseIntPtrArray(intPtrArray);
            RecyclableArrayHelper.ReuseIntegerArray(integerArray);
            RecyclableArrayHelper.ReuseIntegerArray(fragmentChunkType);
            return list;
        }

        public override long GetBytesRead()
        {
            throw new HttpException(System.Web.SR.GetString("Not_supported"));
        }

        public override byte[] GetClientCertificate()
        {
            if (!this._clientCertFetched)
            {
                this.FetchClientCertificate();
            }
            return this._clientCert;
        }

        public override byte[] GetClientCertificateBinaryIssuer()
        {
            if (!this._clientCertFetched)
            {
                this.FetchClientCertificate();
            }
            return this._clientCertBinaryIssuer;
        }

        public override int GetClientCertificateEncoding()
        {
            if (!this._clientCertFetched)
            {
                this.FetchClientCertificate();
            }
            return this._clientCertEncoding;
        }

        public override byte[] GetClientCertificatePublicKey()
        {
            if (!this._clientCertFetched)
            {
                this.FetchClientCertificate();
            }
            return this._clientCertPublicKey;
        }

        public override DateTime GetClientCertificateValidFrom()
        {
            if (!this._clientCertFetched)
            {
                this.FetchClientCertificate();
            }
            return this._clientCertValidFrom;
        }

        public override DateTime GetClientCertificateValidUntil()
        {
            if (!this._clientCertFetched)
            {
                this.FetchClientCertificate();
            }
            return this._clientCertValidUntil;
        }

        private string GetCookieHeaderInternal()
        {
            IntPtr ptr;
            int num;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetCookieHeader(this._context, out ptr, out num));
            if (ptr != IntPtr.Zero)
            {
                return StringUtil.StringFromCharPtr(ptr, num);
            }
            return null;
        }

        internal string GetCurrentModuleName()
        {
            IntPtr ptr;
            int num;
            string str = null;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetCurrentModuleName(this._context, out ptr, out num));
            if (num > 0)
            {
                str = StringUtil.StringFromWCharPtr(ptr, num);
            }
            return str;
        }

        public override string GetFilePath()
        {
            return this._filePath;
        }

        public override string GetFilePathTranslated()
        {
            return this._pathTranslated;
        }

        private unsafe void GetHeaderChanges(HttpContext ctx, bool forRequest)
        {
            IntPtr ptr;
            int num;
            IntPtr ptr2;
            IntPtr ptr3;
            IntPtr ptr4;
            int num2;
            IntPtr ptr5;
            int num3 = forRequest ? 0x29 : 30;
            int knownHeaderIndex = -1;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetHeaderChanges(this._context, forRequest, out ptr, out num, out ptr2, out ptr3, out ptr4, out num2, out ptr5));
            int* numPtr = (int*) ptr4.ToPointer();
            IntPtr* ptrPtr = (IntPtr*) ptr.ToPointer();
            for (int i = 0; i < (num3 + 1); i++)
            {
                string knownRequestHeaderName;
                int index = numPtr[i];
                if (index < 0)
                {
                    break;
                }
                if (forRequest)
                {
                    if (index > 40)
                    {
                        throw new NotSupportedException();
                    }
                    if (index < 0x27)
                    {
                        knownRequestHeaderName = HttpWorkerRequest.GetKnownRequestHeaderName(index);
                    }
                    else if (index == 0x27)
                    {
                        knownRequestHeaderName = "Translate";
                    }
                    else
                    {
                        knownRequestHeaderName = HttpWorkerRequest.GetKnownRequestHeaderName(0x27);
                    }
                }
                else
                {
                    if (index >= 30)
                    {
                        throw new NotSupportedException();
                    }
                    if (index == 0x1a)
                    {
                        continue;
                    }
                    knownRequestHeaderName = HttpWorkerRequest.GetKnownResponseHeaderName(index);
                    knownHeaderIndex = index;
                }
                IntPtr ip = ptrPtr[index];
                string str2 = null;
                if (ip != IntPtr.Zero)
                {
                    str2 = StringUtil.StringFromCharPtr(ip, System.Web.UnsafeNativeMethods.lstrlenA(ip));
                }
                if (forRequest)
                {
                    ctx.Request.SynchronizeHeader(knownRequestHeaderName, str2);
                }
                else
                {
                    ctx.Response.SynchronizeHeader(knownHeaderIndex, knownRequestHeaderName, str2);
                }
            }
            if (num2 != 0)
            {
                int* numPtr2 = (int*) ptr5.ToPointer();
                IntPtr* ptrPtr2 = (IntPtr*) ptr2.ToPointer();
                IntPtr* ptrPtr3 = (IntPtr*) ptr3.ToPointer();
                for (int j = 0; j < num2; j++)
                {
                    int num9 = numPtr2[j];
                    IntPtr ptr7 = ptrPtr2[num9];
                    IntPtr ptr8 = (num9 < num) ? ptrPtr3[num9] : IntPtr.Zero;
                    string name = StringUtil.StringFromCharPtr(ptr7, System.Web.UnsafeNativeMethods.lstrlenA(ptr7));
                    string str4 = null;
                    if (ptr8 != IntPtr.Zero)
                    {
                        str4 = StringUtil.StringFromCharPtr(ptr8, System.Web.UnsafeNativeMethods.lstrlenA(ptr8));
                    }
                    if (forRequest)
                    {
                        ctx.Request.SynchronizeHeader(name, str4);
                    }
                    else
                    {
                        int num10 = -1;
                        if (StringUtil.EqualsIgnoreCase(name, "Set-Cookie"))
                        {
                            num10 = 0x1b;
                        }
                        ctx.Response.SynchronizeHeader(num10, name, str4);
                    }
                }
            }
        }

        public override string GetHttpVerbName()
        {
            return this.GetMethodInternal();
        }

        public override string GetHttpVersion()
        {
            return this.GetServerVariable("SERVER_PROTOCOL");
        }

        public override string GetKnownRequestHeader(int index)
        {
            if (!this._requestHeadersAvailable)
            {
                switch (index)
                {
                    case 11:
                        if (this._contentType == 0)
                        {
                            break;
                        }
                        return this._contentTotalLength.ToString(CultureInfo.InvariantCulture);

                    case 12:
                        if (this._contentType != 1)
                        {
                            break;
                        }
                        return "application/x-www-form-urlencoded";

                    case 0x19:
                        return this.GetCookieHeaderInternal();

                    case 0x27:
                        return this.GetUserAgentInternal();
                }
                this.ReadRequestHeaders();
            }
            return this._knownRequestHeaders[index];
        }

        public override string GetLocalAddress()
        {
            return this.GetServerVariable("LOCAL_ADDR");
        }

        public override int GetLocalPort()
        {
            return UnsafeIISMethods.MgdGetLocalPort(this._context);
        }

        internal override string GetLocalPortAsString()
        {
            return this.GetServerVariable("SERVER_PORT");
        }

        internal string GetManagedHandlerType()
        {
            IntPtr ptr;
            int num;
            string str = null;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetHandlerTypeString(this._context, out ptr, out num));
            if (num > 0)
            {
                str = StringUtil.StringFromWCharPtr(ptr, num);
            }
            return str;
        }

        private string GetMethodInternal()
        {
            IntPtr ptr;
            int num;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetMethod(this._context, out ptr, out num));
            return StringUtil.StringFromCharPtr(ptr, num);
        }

        public override string GetPathInfo()
        {
            return this._pathInfo;
        }

        private int GetPreloadedContentInternal(byte[] buffer, int offset, int length)
        {
            if (offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((length + offset) > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            int pcbReceived = 0;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetPreloadedContent(this._context, buffer, offset, length, out pcbReceived));
            if (pcbReceived > 0)
            {
                PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, pcbReceived);
            }
            return pcbReceived;
        }

        public override byte[] GetPreloadedEntityBody()
        {
            byte[] buffer = null;
            int preloadedEntityBodyLength = this.GetPreloadedEntityBodyLength();
            if (preloadedEntityBodyLength > 0)
            {
                buffer = new byte[preloadedEntityBodyLength];
                this.GetPreloadedContentInternal(buffer, 0, preloadedEntityBodyLength);
            }
            return buffer;
        }

        public override int GetPreloadedEntityBody(byte[] buffer, int offset)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (this.GetPreloadedEntityBodyLength() == 0)
            {
                return 0;
            }
            int length = buffer.Length - offset;
            return this.GetPreloadedContentInternal(buffer, offset, length);
        }

        public override int GetPreloadedEntityBodyLength()
        {
            if (!this._preloadedLengthRead)
            {
                int pcbAvailable = 0;
                Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetPreloadedSize(this._context, out pcbAvailable));
                this._preloadedLength = pcbAvailable;
                this._preloadedLengthRead = true;
            }
            return this._preloadedLength;
        }

        public override string GetQueryString()
        {
            IntPtr ptr;
            int num;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetQueryString(this._context, out ptr, out num));
            if (ptr == IntPtr.Zero)
            {
                return string.Empty;
            }
            return StringUtil.StringFromWCharPtr(ptr, num);
        }

        public override unsafe byte[] GetQueryStringRawBytes()
        {
            IntPtr ptr;
            int num;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetQueryString(this._context, out ptr, out num));
            if (num == 0)
            {
                return null;
            }
            byte[] buffer = new byte[num];
            char* chPtr = (char*) ptr;
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte) chPtr[i];
            }
            return buffer;
        }

        public override string GetRawUrl()
        {
            return HttpWorkerRequest.GetRawUrlHelper(this._cacheUrl);
        }

        public override string GetRemoteAddress()
        {
            return this.GetServerVariable("REMOTE_ADDR");
        }

        public override string GetRemoteName()
        {
            return this.GetServerVariable("REMOTE_HOST");
        }

        public override int GetRemotePort()
        {
            return UnsafeIISMethods.MgdGetRemotePort(this._context);
        }

        public override string GetServerName()
        {
            return this.GetServerVariable("SERVER_NAME");
        }

        private unsafe void GetServerVarChanges(HttpContext ctx)
        {
            int num;
            IntPtr ptr;
            IntPtr ptr2;
            int num2;
            IntPtr ptr3;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetServerVarChanges(this._context, out num, out ptr, out ptr2, out num2, out ptr3));
            if (num2 != 0)
            {
                int* numPtr = (int*) ptr3.ToPointer();
                IntPtr* ptrPtr = (IntPtr*) ptr.ToPointer();
                IntPtr* ptrPtr2 = (IntPtr*) ptr2.ToPointer();
                for (int i = 0; i < num2; i++)
                {
                    int index = numPtr[i];
                    IntPtr ip = ptrPtr[index];
                    IntPtr ptr5 = ptrPtr2[index];
                    string name = StringUtil.StringFromCharPtr(ip, System.Web.UnsafeNativeMethods.lstrlenA(ip));
                    string str2 = null;
                    if (ptr5 != IntPtr.Zero)
                    {
                        str2 = StringUtil.StringFromWCharPtr(ptr5, System.Web.UnsafeNativeMethods.lstrlenW(ptr5));
                    }
                    ctx.Request.SynchronizeServerVariable(name, str2);
                }
            }
        }

        public override string GetServerVariable(string name)
        {
            if (StringUtil.StringStartsWith(name, "HTTP_"))
            {
                return this.GetServerVariableInternalAnsi(name);
            }
            return this.GetServerVariableInternal(name);
        }

        private string GetServerVariableInternal(string name)
        {
            IntPtr ptr;
            int num;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetServerVariableW(this._context, name, out ptr, out num));
            if (ptr != IntPtr.Zero)
            {
                return StringUtil.StringFromWCharPtr(ptr, num);
            }
            return null;
        }

        private string GetServerVariableInternalAnsi(string name)
        {
            IntPtr ptr;
            int num;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetServerVariableA(this._context, name, out ptr, out num));
            if (ptr != IntPtr.Zero)
            {
                return StringUtil.StringFromCharPtr(ptr, num);
            }
            return null;
        }

        private void GetStatusChanges(HttpContext ctx)
        {
            ushort num;
            ushort num2;
            IntPtr ptr;
            ushort num3;
            string description = null;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetStatusChanges(this._context, out num, out num2, out ptr, out num3));
            if (ptr != IntPtr.Zero)
            {
                description = StringUtil.StringFromCharPtr(ptr, num3);
            }
            this._trySkipIisCustomErrors = false;
            ctx.Response.SynchronizeStatus(num, num2, description);
        }

        public override int GetTotalEntityBodyLength()
        {
            return this._contentTotalLength;
        }

        public override string GetUnknownRequestHeader(string name)
        {
            if (!this._requestHeadersAvailable)
            {
                this.ReadRequestHeaders();
            }
            int length = this._unknownRequestHeaders.Length;
            for (int i = 0; i < length; i++)
            {
                if (StringUtil.EqualsIgnoreCase(name, this._unknownRequestHeaders[i][0]))
                {
                    return this._unknownRequestHeaders[i][1];
                }
            }
            return null;
        }

        public override string[][] GetUnknownRequestHeaders()
        {
            if (!this._requestHeadersAvailable)
            {
                this.ReadRequestHeaders();
            }
            return this._unknownRequestHeaders;
        }

        public override string GetUriPath()
        {
            return this._path;
        }

        internal string GetUriPathInternal(bool includePathInfo, bool useParentContext)
        {
            IntPtr ptr;
            int num;
            string str = string.Empty;
            if (UnsafeIISMethods.MgdGetUriPath(this._context, out ptr, out num, includePathInfo, useParentContext) < 0)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_retrieve_request_data"));
            }
            if (num > 0)
            {
                str = StringUtil.StringFromWCharPtr(ptr, num);
            }
            return str;
        }

        private string GetUserAgentInternal()
        {
            IntPtr ptr;
            int num;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetUserAgent(this._context, out ptr, out num));
            if (ptr != IntPtr.Zero)
            {
                return StringUtil.StringFromCharPtr(ptr, num);
            }
            return null;
        }

        private IPrincipal GetUserPrincipal()
        {
            IntPtr ptr;
            IntPtr ptr2;
            IntPtr ptr3;
            int pcchAuthType = 0;
            int pcchUserName = 0;
            IIdentity identity = null;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetPrincipal(this._context, out ptr, out ptr2, ref pcchAuthType, out ptr3, ref pcchUserName));
            string str = string.Empty;
            if ((ptr3 != IntPtr.Zero) && (pcchUserName > 0))
            {
                str = StringUtil.StringFromWCharPtr(ptr3, pcchUserName);
            }
            string type = string.Empty;
            if ((ptr2 != IntPtr.Zero) && (pcchAuthType > 0))
            {
                type = StringUtil.StringFromWCharPtr(ptr2, pcchAuthType);
            }
            if (string.IsNullOrEmpty(str))
            {
                return WindowsAuthenticationModule.AnonymousPrincipal;
            }
            if (ptr != IntPtr.Zero)
            {
                identity = new WindowsIdentity(ptr, type, WindowsAccountType.Normal, true);
                return new WindowsPrincipal((WindowsIdentity) identity);
            }
            return new IIS7UserPrincipal(this, new GenericIdentity(str, type));
        }

        public override IntPtr GetUserToken()
        {
            IntPtr zero = IntPtr.Zero;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetUserToken(this._context, out zero));
            return zero;
        }

        public override IntPtr GetVirtualPathToken()
        {
            IntPtr zero = IntPtr.Zero;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetVirtualToken(this._context, out zero));
            return zero;
        }

        public override bool HeadersSent()
        {
            return this._headersSent;
        }

        internal void InitAppVars()
        {
            IntPtr ptr;
            IntPtr ptr2;
            int num;
            int num2;
            if (UnsafeIISMethods.MgdGetApplicationInfo(this._context, out ptr, out num, out ptr2, out num2) < 0)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_retrieve_request_data"));
            }
            this._appPath = StringUtil.StringFromWCharPtr(ptr, num);
            this._appPathTranslated = StringUtil.StringFromWCharPtr(ptr2, num2);
            if (((this._appPathTranslated != null) && (this._appPathTranslated.Length > 2)) && !StringUtil.StringEndsWith(this._appPathTranslated, '\\'))
            {
                this._appPathTranslated = this._appPathTranslated + @"\";
            }
        }

        internal void Initialize()
        {
            this.ReadRequestBasics();
            this.InitAppVars();
        }

        internal void InsertEntityBody(byte[] buffer, int offset, int count)
        {
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdInsertEntityBody(this._context, buffer, offset, count));
        }

        public override bool IsClientConnected()
        {
            return (!this._connectionClosed && UnsafeIISMethods.MgdIsClientConnected(this._context));
        }

        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return (this.GetTotalEntityBodyLength() == this.GetPreloadedEntityBodyLength());
        }

        internal bool IsHandlerExecutionDenied()
        {
            return UnsafeIISMethods.MgdIsHandlerExecutionDenied(this._context);
        }

        internal bool IsResponseBuffered()
        {
            int fragmentCount = 0;
            int hresult = UnsafeIISMethods.MgdGetResponseChunks(this._context, ref fragmentCount, null, null, null);
            if (hresult != -2147024774)
            {
                Misc.ThrowIfFailedHr(hresult);
            }
            return (fragmentCount > 0);
        }

        public override bool IsSecure()
        {
            string serverVariable = this.GetServerVariable("HTTPS");
            return ((serverVariable != null) && serverVariable.Equals("on"));
        }

        internal bool IsUserInRole(string role)
        {
            bool pfIsInRole = false;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdIsInRole(this._context, role, out pfIsInRole));
            return pfIsInRole;
        }

        private static bool IsValidUsername(string username)
        {
            return (AppSettings.AllowRelaxedHttpUserName || (username.IndexOf('\0') == -1));
        }

        internal string MapHandlerAndGetHandlerTypeString(string method, string path, bool convertNativeStaticFileModule)
        {
            IntPtr ptr;
            int num;
            string str = null;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdMapHandler(this._context, method, path, out ptr, out num, convertNativeStaticFileModule));
            if (num > 0)
            {
                str = StringUtil.StringFromWCharPtr(ptr, num);
            }
            return str;
        }

        public override string MapPath(string path)
        {
            return HostingEnvironment.MapPathInternal(path);
        }

        internal void PushResponseToNative()
        {
            this.FlushCachedResponse(false);
        }

        private void RaiseCommunicationError(int result, bool throwOnDisconnect)
        {
            if (UnsafeIISMethods.MgdIsClientConnected(this._context))
            {
                throw new HttpException(System.Web.SR.GetString("Server_Support_Function_Error", new object[] { result.ToString("X8", CultureInfo.InvariantCulture) }), Marshal.GetExceptionForHR(result));
            }
            if (!this._disconnected)
            {
                PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.REQUESTS_DISCONNECTED);
                this._disconnected = true;
            }
            if (throwOnDisconnect)
            {
                throw new HttpException(System.Web.SR.GetString("Server_Support_Function_Error_Disconnect", new object[] { result.ToString("X8", CultureInfo.InvariantCulture) }), result);
            }
        }

        internal override void RaiseTraceEvent(WebBaseEvent webEvent)
        {
            if ((this._traceEnabled && (this._context != IntPtr.Zero)) && EtwTrace.IsTraceEnabled(webEvent.InferEtwTraceVerbosity(), 1))
            {
                int num;
                string[] strArray;
                int[] numArray;
                string[] strArray2;
                int num2;
                webEvent.DeconstructWebEvent(out num2, out num, out strArray, out numArray, out strArray2);
                UnsafeIISMethods.MgdEmitWebEventTrace(this._context, num2, num, strArray, numArray, strArray2);
            }
        }

        internal override void RaiseTraceEvent(IntegratedTraceType traceType, string eventData)
        {
            if (this._traceEnabled && (this._context != IntPtr.Zero))
            {
                int flag = (traceType < IntegratedTraceType.DiagCritical) ? 4 : 2;
                if (EtwTrace.IsTraceEnabled(EtwTrace.InferVerbosity(traceType), flag))
                {
                    string str = string.IsNullOrEmpty(eventData) ? string.Empty : eventData;
                    UnsafeIISMethods.MgdEmitSimpleTrace(this._context, (int) traceType, str);
                }
            }
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            if (size > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            return this.ReadEntityCoreSync(buffer, 0, size);
        }

        public override int ReadEntityBody(byte[] buffer, int offset, int size)
        {
            if ((buffer.Length - offset) < size)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            return this.ReadEntityCoreSync(buffer, offset, size);
        }

        private int ReadEntityCoreSync(byte[] buffer, int offset, int size)
        {
            int pBytesRead = 0;
            int result = UnsafeIISMethods.MgdSyncReadRequest(this._context, buffer, offset, size, out pBytesRead);
            if (result < 0)
            {
                this.RaiseCommunicationError(result, false);
            }
            if (pBytesRead > 0)
            {
                PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, pBytesRead);
            }
            return pBytesRead;
        }

        internal void ReadRequestBasics()
        {
            IntPtr ptr;
            int num;
            IntPtr ptr2;
            int num2;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdGetRequestBasics(this._context, out this._contentType, out this._contentTotalLength, out ptr, out num, out ptr2, out num2));
            this._cacheUrl = (num2 <= 0) ? null : StringUtil.StringFromWCharPtr(ptr2, num2);
            this._pathTranslated = (num <= 0) ? string.Empty : StringUtil.StringFromWCharPtr(ptr, num);
            this._path = this.GetUriPathInternal(true, false);
            this._filePath = this.GetUriPathInternal(false, false);
            int num4 = this._path.Length - this._filePath.Length;
            if (num4 > 0)
            {
                this._pathInfo = this._path.Substring(this._filePath.Length);
                int length = this._pathTranslated.Length - num4;
                if (length > 0)
                {
                    this._pathTranslated = this._pathTranslated.Substring(0, length);
                }
            }
            else
            {
                this._filePath = this._path;
                this._pathInfo = string.Empty;
            }
            this._queryString = this.GetQueryString();
        }

        private void ReadRequestHeaders()
        {
            if (!this._requestHeadersAvailable)
            {
                this._knownRequestHeaders = new string[40];
                ArrayList list = new ArrayList();
                string serverVariable = this.GetServerVariable("ALL_RAW");
                int num = (serverVariable != null) ? serverVariable.Length : 0;
                int startIndex = 0;
                while (startIndex < num)
                {
                    int num3 = serverVariable.IndexOfAny(s_ColonOrNL, startIndex);
                    if (num3 < 0)
                    {
                        break;
                    }
                    if (serverVariable[num3] == '\n')
                    {
                        startIndex = num3 + 1;
                    }
                    else
                    {
                        if (num3 == startIndex)
                        {
                            startIndex++;
                            continue;
                        }
                        string header = serverVariable.Substring(startIndex, num3 - startIndex).Trim();
                        int index = serverVariable.IndexOf('\n', num3 + 1);
                        if (index < 0)
                        {
                            index = num;
                        }
                        while ((index < (num - 1)) && (serverVariable[index + 1] == ' '))
                        {
                            index = serverVariable.IndexOf('\n', index + 1);
                            if (index < 0)
                            {
                                index = num;
                            }
                        }
                        string str3 = serverVariable.Substring(num3 + 1, (index - num3) - 1).Trim();
                        int knownRequestHeaderIndex = HttpWorkerRequest.GetKnownRequestHeaderIndex(header);
                        if (knownRequestHeaderIndex >= 0)
                        {
                            this._knownRequestHeaders[knownRequestHeaderIndex] = str3;
                        }
                        else
                        {
                            list.Add(header);
                            list.Add(str3);
                        }
                        startIndex = index + 1;
                    }
                }
                int num6 = list.Count / 2;
                this._unknownRequestHeaders = new string[num6][];
                int num7 = 0;
                for (startIndex = 0; startIndex < num6; startIndex++)
                {
                    this._unknownRequestHeaders[startIndex] = new string[] { (string) list[num7++], (string) list[num7++] };
                }
                this._requestHeadersAvailable = true;
            }
        }

        internal string ReMapHandlerAndGetHandlerTypeString(HttpContext httpContext, string path, out bool handlerExists)
        {
            IntPtr ptr;
            int num;
            string str = null;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdReMapHandler(this._context, path, out ptr, out num, out handlerExists));
            if (num > 0)
            {
                str = StringUtil.StringFromWCharPtr(ptr, num);
            }
            if (handlerExists)
            {
                this.ReadRequestBasics();
                httpContext.ConfigurationPath = null;
                try
                {
                    this._rewriteNotifyDisabled = true;
                    httpContext.Request.InternalRewritePath(VirtualPath.CreateAllowNull(this._filePath), VirtualPath.CreateAllowNull(this._pathInfo), this._queryString, this._rebaseClientPath);
                }
                finally
                {
                    this._rewriteNotifyDisabled = false;
                }
            }
            return str;
        }

        private void ResetCachedResponse()
        {
            this._cachedResponseBodyLength = 0;
            this._cachedResponseBodyBytes = null;
        }

        internal void ResponseFilterInstalled()
        {
            UnsafeIISMethods.MgdSetResponseFilter(this._context);
        }

        internal void RewriteNotifyPipeline(string newPath, string newQueryString, bool rebaseClientPath)
        {
            if (!this._rewriteNotifyDisabled && (IntPtr.Zero != this._context))
            {
                string pszUrl = newPath;
                if (newQueryString != null)
                {
                    pszUrl = newPath + "?" + newQueryString;
                }
                UnsafeIISMethods.MgdRewriteUrl(this._context, pszUrl, null != newQueryString);
                this._rebaseClientPath = rebaseClientPath;
            }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal void ScheduleExecuteUrl(string url, string queryString, string method, bool preserveForm, byte[] entity, NameValueCollection headers, bool preserveUser)
        {
            string[] headersNames = null;
            string[] headersValues = null;
            int numHeaders = 0;
            if ((headers != null) && (headers.Count > 0))
            {
                numHeaders = headers.Count;
                headersNames = new string[numHeaders];
                headersValues = new string[numHeaders];
                for (int i = 0; i < numHeaders; i++)
                {
                    headersNames[i] = headers.GetKey(i);
                    headersValues[i] = headers.Get(i);
                }
            }
            bool resetQuerystring = !string.IsNullOrEmpty(queryString);
            if (resetQuerystring)
            {
                url = url + "?" + queryString;
            }
            int hresult = UnsafeIISMethods.MgdExecuteUrl(this._context, url, resetQuerystring, preserveForm, entity, (entity == null) ? 0 : ((uint) entity.Length), method, numHeaders, headersNames, headersValues, preserveUser);
            if (hresult == -2147024846)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("TransferRequest_cannot_be_invoked_more_than_once"));
            }
            Misc.ThrowIfFailedHr(hresult);
        }

        public override void SendCalculatedContentLength(int contentLength)
        {
            this.SendKnownResponseHeader(11, contentLength.ToString(CultureInfo.InvariantCulture));
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            if ((index < 0) || (index >= 30))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this.SetKnownResponseHeader(index, value, false);
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            if (!this._connectionClosed && (length != 0L))
            {
                FileStream f = null;
                try
                {
                    f = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read, 0, false);
                    this.SendResponseFromFileStream(f, offset, length);
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

        public override void SendResponseFromFile(string name, long offset, long length)
        {
            if (!this._connectionClosed && (length != 0L))
            {
                FileStream f = null;
                try
                {
                    f = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read);
                    this.SendResponseFromFileStream(f, offset, length);
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

        private void SendResponseFromFileStream(FileStream f, long offset, long length)
        {
            long num = f.Length;
            if (length == -1L)
            {
                length = num - offset;
            }
            if ((offset < 0L) || (length > (num - offset)))
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_range"));
            }
            if (length > 0L)
            {
                if (offset > 0L)
                {
                    f.Seek(offset, SeekOrigin.Begin);
                }
                byte[] buffer = new byte[(int) length];
                int size = f.Read(buffer, 0, (int) length);
                if (size > 0)
                {
                    this.AddBodyToCachedResponse(new MemoryBytes(buffer, size));
                }
            }
        }

        internal void SendResponseFromIISAllocatedRequestMemory(IntPtr data, int length)
        {
            if ((data != IntPtr.Zero) && (length >= 0))
            {
                this.AddBodyToCachedResponse(new MemoryBytes(data, length, BufferType.IISAllocatedRequestMemory));
            }
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            if (!this._connectionClosed && (length > 0))
            {
                this.AddBodyToCachedResponse(new MemoryBytes(data, length));
            }
        }

        public override void SendResponseFromMemory(IntPtr data, int length)
        {
            if (!this._connectionClosed)
            {
                this.SendResponseFromMemory(data, length, false);
            }
        }

        internal override void SendResponseFromMemory(IntPtr data, int length, bool isBufferFromUnmanagedPool)
        {
            if (length > 0)
            {
                this.AddBodyToCachedResponse(new MemoryBytes(data, length, isBufferFromUnmanagedPool ? BufferType.UnmanagedPool : BufferType.Managed));
            }
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            this.SendStatus(statusCode, 0, statusDescription);
        }

        internal override void SendStatus(int statusCode, int subStatusCode, string statusDescription)
        {
            if (statusDescription == null)
            {
                statusDescription = string.Empty;
            }
            int hresult = UnsafeIISMethods.MgdSetStatusW(this._context, statusCode, subStatusCode, statusDescription, null, this._trySkipIisCustomErrors);
            this._trySkipIisCustomErrors = false;
            Misc.ThrowIfFailedHr(hresult);
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.SetUnknownResponseHeader(name, value, false);
        }

        internal override void SetHeaderEncoding(Encoding encoding)
        {
            this._headerEncoding = encoding;
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
        private void SetKnownRequestHeader(int index, string value, bool replace)
        {
            if (index == 0x27)
            {
                index = 40;
            }
            byte[] buffer = (value != null) ? this._headerEncoding.GetBytes(value) : null;
            int num = (buffer != null) ? buffer.Length : 0;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdSetKnownHeader(this._context, true, replace, (ushort) index, buffer, (ushort) num));
        }

        private void SetKnownResponseHeader(int index, string value, bool replace)
        {
            if (((index == 0x1d) || (index == 0x1b)) || (index == 0x1a))
            {
                this.SetUnknownResponseHeader(HttpWorkerRequest.GetKnownResponseHeaderName(index), value, replace);
            }
            else
            {
                byte[] buffer = (value != null) ? this._headerEncoding.GetBytes(value) : null;
                int num = (buffer != null) ? buffer.Length : 0;
                Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdSetKnownHeader(this._context, false, replace, (ushort) index, buffer, (ushort) num));
            }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal void SetPrincipal(IPrincipal user, IntPtr pManagedPrincipal)
        {
            string username = null;
            string authType = null;
            IntPtr zero = IntPtr.Zero;
            if (user != null)
            {
                if (user.Identity != null)
                {
                    username = user.Identity.Name;
                    authType = user.Identity.AuthenticationType;
                    WindowsIdentity identity = user.Identity as WindowsIdentity;
                    if (identity != null)
                    {
                        zero = identity.Token;
                    }
                }
                if (username == null)
                {
                    username = string.Empty;
                }
                if (authType == null)
                {
                    authType = string.Empty;
                }
                if (!IsValidUsername(username))
                {
                    throw new ArgumentException();
                }
            }
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdSetRequestPrincipal(this._context, pManagedPrincipal, username, authType, zero));
        }

        internal void SetRemapHandler(string handlerType, string handlerName)
        {
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdSetRemapHandler(this._context, handlerName, handlerType));
        }

        internal void SetRequestHeader(string name, string value, bool replace)
        {
            int knownRequestHeaderIndex = HttpWorkerRequest.GetKnownRequestHeaderIndex(name);
            if (knownRequestHeaderIndex >= 0)
            {
                this.SetKnownRequestHeader(knownRequestHeaderIndex, value, replace);
            }
            else
            {
                this.SetUnknownRequestHeader(name, value, replace);
            }
        }

        internal void SetResponseHeader(string name, string value, bool replace)
        {
            int knownResponseHeaderIndex = HttpWorkerRequest.GetKnownResponseHeaderIndex(name);
            if (knownResponseHeaderIndex >= 0)
            {
                this.SetKnownResponseHeader(knownResponseHeaderIndex, value, replace);
            }
            else
            {
                this.SetUnknownResponseHeader(name, value, replace);
            }
        }

        internal void SetServerVariable(string name, string value)
        {
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdSetServerVariableW(this._context, name, value));
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
        private void SetUnknownRequestHeader(string name, string value, bool replace)
        {
            byte[] buffer = (value != null) ? this._headerEncoding.GetBytes(value) : null;
            int num = (buffer != null) ? buffer.Length : 0;
            int byteCount = this._headerEncoding.GetByteCount(name);
            byte[] bytes = new byte[byteCount + 1];
            this._headerEncoding.GetBytes(name, 0, name.Length, bytes, 0);
            bytes[byteCount] = 0;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdSetUnknownHeader(this._context, true, replace, bytes, buffer, (ushort) num));
        }

        private void SetUnknownResponseHeader(string name, string value, bool replace)
        {
            if (StringUtil.EqualsIgnoreCase(name, "Set-Cookie"))
            {
                this.DisableIISCache();
            }
            byte[] buffer = (value != null) ? this._headerEncoding.GetBytes(value) : null;
            int num = (buffer != null) ? buffer.Length : 0;
            int byteCount = this._headerEncoding.GetByteCount(name);
            byte[] bytes = new byte[byteCount + 1];
            this._headerEncoding.GetBytes(name, 0, name.Length, bytes, 0);
            bytes[byteCount] = 0;
            Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdSetUnknownHeader(this._context, false, replace, bytes, buffer, (ushort) num));
        }

        internal override string SetupKernelCaching(int secondsToLive, string originalCacheUrl, bool enableKernelCacheForVaryByStar)
        {
            string str = this._cacheUrl;
            if ((originalCacheUrl != null) && (originalCacheUrl != str))
            {
                return null;
            }
            if (string.IsNullOrEmpty(str) || (!enableKernelCacheForVaryByStar && (str.IndexOf('?') != -1)))
            {
                return null;
            }
            if (UnsafeIISMethods.MgdSetKernelCachePolicy(this._context, secondsToLive) < 0)
            {
                return null;
            }
            return str;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal void SynchronizeVariables(HttpContext context)
        {
            if (context.IsChangeInServerVars)
            {
                this.GetServerVarChanges(context);
            }
            if (context.IsChangeInRequestHeaders)
            {
                this.GetHeaderChanges(context, true);
            }
            if (context.IsChangeInResponseHeaders)
            {
                this.GetHeaderChanges(context, false);
            }
            if (context.IsChangeInResponseStatus)
            {
                this.GetStatusChanges(context);
            }
            if (context.IsChangeInUserPrincipal && WindowsAuthenticationModule.IsEnabled)
            {
                context.SetPrincipalNoDemand(this.GetUserPrincipal(), false);
            }
            if (context.AreResponseHeadersSent)
            {
                context.Response.HeadersWritten = true;
            }
        }

        internal override void TransmitFile(string filename, long offset, long length, bool isImpersonating)
        {
            if (!this._connectionClosed && (length > 0L))
            {
                this.AddBodyToCachedResponse(new MemoryBytes(filename, offset, length));
            }
        }

        internal void UnlockCachedResponseBytes()
        {
            if (this._cachedResponseBodyBytes != null)
            {
                int count = this._cachedResponseBodyBytes.Count;
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        ((MemoryBytes) this._cachedResponseBodyBytes[i]).UnlockMemory();
                    }
                    catch
                    {
                    }
                }
            }
            this.ResetCachedResponse();
        }

        internal ChannelBinding HttpChannelBindingToken
        {
            get
            {
                if (this._channelBindingToken == null)
                {
                    IntPtr zero = IntPtr.Zero;
                    int pcbTokenSize = 0;
                    int hresult = 0;
                    hresult = UnsafeIISMethods.MgdGetChannelBindingToken(this._context, out zero, out pcbTokenSize);
                    if (hresult == -2147467263)
                    {
                        throw new PlatformNotSupportedException();
                    }
                    Misc.ThrowIfFailedHr(hresult);
                    this._channelBindingToken = new System.Web.HttpChannelBindingToken(zero, pcbTokenSize);
                }
                return this._channelBindingToken;
            }
        }

        public override string MachineConfigPath
        {
            get
            {
                return HttpConfigurationSystem.MachineConfigurationFilePath;
            }
        }

        public override string MachineInstallDirectory
        {
            get
            {
                return HttpRuntime.AspInstallDirectory;
            }
        }

        internal IntPtr RequestContext
        {
            get
            {
                if (this._context == IntPtr.Zero)
                {
                    return IntPtr.Zero;
                }
                return this._context;
            }
        }

        public override Guid RequestTraceIdentifier
        {
            get
            {
                return this._traceId;
            }
        }

        public override string RootWebConfigPath
        {
            get
            {
                return HttpConfigurationSystem.RootWebConfigurationFilePath;
            }
        }

        internal override bool SupportsExecuteUrl
        {
            get
            {
                return false;
            }
        }

        internal override bool SupportsLongTransmitFile
        {
            get
            {
                return true;
            }
        }

        internal override bool TrySkipIisCustomErrors
        {
            get
            {
                return this._trySkipIisCustomErrors;
            }
            set
            {
                this._trySkipIisCustomErrors = value;
            }
        }
    }
}

