namespace System.Web.Hosting
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    internal abstract class ISAPIWorkerRequest : HttpWorkerRequest
    {
        protected string _appPath;
        protected string _appPathTranslated;
        private ArrayList _cachedResponseBodyBytes;
        private int _cachedResponseBodyBytesIoLockCount;
        private int _cachedResponseBodyLength;
        private byte[] _cachedResponseHeaders;
        private int _cachedResponseKeepConnected;
        private byte[] _cachedResponseStatus;
        private bool _chunked;
        private byte[] _clientCert;
        private byte[] _clientCertBinaryIssuer;
        private int _clientCertEncoding;
        private bool _clientCertFetched;
        private byte[] _clientCertPublicKey;
        private DateTime _clientCertValidFrom;
        private DateTime _clientCertValidUntil;
        protected int _contentAvailLength;
        private bool _contentLengthSent;
        protected int _contentTotalLength;
        protected int _contentType;
        protected IntPtr _ecb;
        private HttpWorkerRequest.EndOfSendNotification _endOfRequestCallback;
        private object _endOfRequestCallbackArg;
        private int _endOfRequestCallbackLockCount;
        protected string _filePath;
        private Encoding _headerEncoding;
        private RecyclableCharBuffer _headers = new RecyclableCharBuffer();
        private bool _headersSent;
        protected bool _ignoreMinAsyncSize;
        private string[] _knownRequestHeaders;
        protected string _method;
        protected string _path;
        protected string _pathInfo;
        protected string _pathTranslated;
        private byte[] _preloadedContent;
        private bool _preloadedContentRead;
        protected int _queryStringLength;
        private bool _requestHeadersAvailable;
        protected bool _requiresAsyncFlushCallback;
        private RecyclableCharBuffer _status = new RecyclableCharBuffer();
        private bool _statusSet = true;
        protected IntPtr _token;
        protected Guid _traceId;
        private string[][] _unknownRequestHeaders;
        private const int CONTENT_FORM = 1;
        private const int CONTENT_MULTIPART = 2;
        private const int CONTENT_NONE = 0;
        private const int CONTENT_OTHER = 3;
        private static readonly char[] s_ColonOrNL = new char[] { ':', '\n' };
        private const int STATUS_ERROR = 4;
        private const int STATUS_PENDING = 3;
        private const int STATUS_SUCCESS = 1;
        private const int STATUS_SUCCESS_AND_KEEP_CONN = 2;

        internal ISAPIWorkerRequest(IntPtr ecb)
        {
            this._ecb = ecb;
            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_TOTAL);
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

        private void AddHeadersToCachedResponse(byte[] status, byte[] header, int keepConnected)
        {
            this._cachedResponseStatus = status;
            this._cachedResponseHeaders = header;
            this._cachedResponseKeepConnected = keepConnected;
        }

        internal void AppendLogParameter(string logParam)
        {
            this.AppendLogParameterCore(logParam);
        }

        internal abstract int AppendLogParameterCore(string logParam);
        internal void CallEndOfRequestCallbackOnceAfterAllIoComplete()
        {
            if ((this._endOfRequestCallback != null) && (Interlocked.Decrement(ref this._endOfRequestCallbackLockCount) == 0))
            {
                try
                {
                    this._endOfRequestCallback(this, this._endOfRequestCallbackArg);
                }
                catch
                {
                }
            }
        }

        internal abstract int CallISAPI(UnsafeNativeMethods.CallISAPIFunc iFunction, byte[] bufIn, byte[] bufOut);
        internal virtual void Close()
        {
        }

        public override void CloseConnection()
        {
            this.CloseConnectionCore();
        }

        internal abstract int CloseConnectionCore();
        internal static ISAPIWorkerRequest CreateWorkerRequest(IntPtr ecb, bool useOOP)
        {
            if (useOOP)
            {
                EtwTrace.TraceEnableCheck(EtwTraceConfigType.DOWNLEVEL, IntPtr.Zero);
                if (EtwTrace.IsTraceEnabled(5, 1))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_APPDOMAIN_ENTER, ecb, Thread.GetDomain().FriendlyName, null, false);
                }
                return new ISAPIWorkerRequestOutOfProc(ecb);
            }
            int num = UnsafeNativeMethods.EcbGetVersion(ecb) >> 0x10;
            if (num >= 7)
            {
                EtwTrace.TraceEnableCheck(EtwTraceConfigType.IIS7_ISAPI, ecb);
            }
            else
            {
                EtwTrace.TraceEnableCheck(EtwTraceConfigType.DOWNLEVEL, IntPtr.Zero);
            }
            if (EtwTrace.IsTraceEnabled(5, 1))
            {
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_APPDOMAIN_ENTER, ecb, Thread.GetDomain().FriendlyName, null, true);
            }
            if (num >= 7)
            {
                return new ISAPIWorkerRequestInProcForIIS7(ecb);
            }
            if (num == 6)
            {
                return new ISAPIWorkerRequestInProcForIIS6(ecb);
            }
            return new ISAPIWorkerRequestInProc(ecb);
        }

        public override void EndOfRequest()
        {
            this.FlushCachedResponse(true);
            if (this._headers != null)
            {
                this._headers.Dispose();
                this._headers = null;
            }
            if (this._status != null)
            {
                this._status.Dispose();
                this._status = null;
            }
            this.CallEndOfRequestCallbackOnceAfterAllIoComplete();
        }

        private void FetchClientCertificate()
        {
            if (!this._clientCertFetched)
            {
                this._clientCertFetched = true;
                byte[] buffer = new byte[0x2000];
                int[] pInts = new int[4];
                long[] pDates = new long[2];
                int num = this.GetClientCertificateCore(buffer, pInts, pDates);
                if ((num < 0) && (-num > 0x2000))
                {
                    num = -num + 100;
                    buffer = new byte[num];
                    num = this.GetClientCertificateCore(buffer, pInts, pDates);
                }
                if (num > 0)
                {
                    this._clientCertEncoding = pInts[0];
                    if ((pInts[1] < buffer.Length) && (pInts[1] > 0))
                    {
                        this._clientCert = new byte[pInts[1]];
                        Array.Copy(buffer, this._clientCert, pInts[1]);
                        if (((pInts[2] + pInts[1]) < buffer.Length) && (pInts[2] > 0))
                        {
                            this._clientCertBinaryIssuer = new byte[pInts[2]];
                            Array.Copy(buffer, pInts[1], this._clientCertBinaryIssuer, 0, pInts[2]);
                        }
                        if ((((pInts[2] + pInts[1]) + pInts[3]) < buffer.Length) && (pInts[3] > 0))
                        {
                            this._clientCertPublicKey = new byte[pInts[3]];
                            Array.Copy(buffer, pInts[1] + pInts[2], this._clientCertPublicKey, 0, pInts[3]);
                        }
                    }
                }
                if ((num > 0) && (pDates[0] != 0L))
                {
                    this._clientCertValidFrom = DateTime.FromFileTime(pDates[0]);
                }
                else
                {
                    this._clientCertValidFrom = DateTime.Now;
                }
                if ((num > 0) && (pDates[1] != 0L))
                {
                    this._clientCertValidUntil = DateTime.FromFileTime(pDates[1]);
                }
                else
                {
                    this._clientCertValidUntil = DateTime.Now;
                }
            }
        }

        private void FlushCachedResponse(bool isFinal)
        {
            if (this._ecb != IntPtr.Zero)
            {
                bool async = false;
                int minimumLength = 0;
                IntPtr[] bodyFragments = null;
                int[] bodyFragmentLengths = null;
                long num2 = 0L;
                try
                {
                    if (this._cachedResponseBodyLength > 0)
                    {
                        minimumLength = this._cachedResponseBodyBytes.Count;
                        bodyFragments = RecyclableArrayHelper.GetIntPtrArray(minimumLength);
                        bodyFragmentLengths = RecyclableArrayHelper.GetIntegerArray(minimumLength);
                        for (int i = 0; i < minimumLength; i++)
                        {
                            MemoryBytes bytes = (MemoryBytes) this._cachedResponseBodyBytes[i];
                            bodyFragments[i] = bytes.LockMemory();
                            if (!isFinal || !bytes.IsBufferFromUnmanagedPool)
                            {
                                this._requiresAsyncFlushCallback = true;
                            }
                            if (bytes.UseTransmitFile)
                            {
                                bodyFragmentLengths[i] = -bytes.Size;
                                this._ignoreMinAsyncSize = true;
                                num2 += bytes.FileSize;
                            }
                            else
                            {
                                bodyFragmentLengths[i] = bytes.Size;
                                num2 += bytes.Size;
                            }
                        }
                    }
                    int doneWithSession = isFinal ? 1 : 0;
                    int finalStatus = isFinal ? ((this._cachedResponseKeepConnected != 0) ? 2 : 1) : 0;
                    this._cachedResponseBodyBytesIoLockCount = 2;
                    this._endOfRequestCallbackLockCount++;
                    if (isFinal)
                    {
                        PerfCounters.DecrementCounter(AppPerfCounter.REQUESTS_EXECUTING);
                    }
                    int delta = (int) num2;
                    if (delta > 0)
                    {
                        PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_OUT, delta);
                    }
                    try
                    {
                        this.FlushCore(this._cachedResponseStatus, this._cachedResponseHeaders, this._cachedResponseKeepConnected, this._cachedResponseBodyLength, minimumLength, bodyFragments, bodyFragmentLengths, doneWithSession, finalStatus, out async);
                    }
                    finally
                    {
                        if (isFinal)
                        {
                            this.Close();
                            this._ecb = IntPtr.Zero;
                        }
                    }
                }
                finally
                {
                    if (!async)
                    {
                        this._cachedResponseBodyBytesIoLockCount--;
                        this._endOfRequestCallbackLockCount--;
                    }
                    this.UnlockCachedResponseBytesOnceAfterIoComplete();
                    RecyclableArrayHelper.ReuseIntPtrArray(bodyFragments);
                    RecyclableArrayHelper.ReuseIntegerArray(bodyFragmentLengths);
                }
            }
        }

        internal abstract void FlushCore(byte[] status, byte[] header, int keepConnected, int totalBodySize, int numBodyFragments, IntPtr[] bodyFragments, int[] bodyFragmentLengths, int doneWithSession, int finalStatus, out bool async);
        public override void FlushResponse(bool finalFlush)
        {
            if (!this._headersSent)
            {
                this.SendHeaders();
            }
            this.FlushCachedResponse(finalFlush);
        }

        internal abstract int GetAdditionalPostedContentCore(byte[] bytes, int offset, int bufferSize);
        public override string GetAppPath()
        {
            return this._appPath;
        }

        public override string GetAppPathTranslated()
        {
            return this._appPathTranslated;
        }

        internal abstract int GetBasicsCore(byte[] buffer, int size, int[] contentInfo);
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

        internal abstract int GetClientCertificateCore(byte[] buffer, int[] pInts, long[] pDates);
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

        public override string GetFilePath()
        {
            return this._filePath;
        }

        public override string GetFilePathTranslated()
        {
            return this._pathTranslated;
        }

        public override string GetHttpVerbName()
        {
            return this._method;
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
                        return this._contentTotalLength.ToString();

                    case 12:
                        if (this._contentType != 1)
                        {
                            break;
                        }
                        return "application/x-www-form-urlencoded";
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
            return int.Parse(this.GetServerVariable("SERVER_PORT"));
        }

        internal override string GetLocalPortAsString()
        {
            return this.GetServerVariable("SERVER_PORT");
        }

        public override string GetPathInfo()
        {
            return this._pathInfo;
        }

        public override byte[] GetPreloadedEntityBody()
        {
            if (!this._preloadedContentRead)
            {
                if (this._contentAvailLength > 0)
                {
                    this._preloadedContent = new byte[this._contentAvailLength];
                    if (this.GetPreloadedPostedContentCore(this._preloadedContent, 0, this._contentAvailLength) < 0)
                    {
                        throw new HttpException(System.Web.SR.GetString("Cannot_read_posted_data"));
                    }
                }
                this._preloadedContentRead = true;
            }
            return this._preloadedContent;
        }

        public override int GetPreloadedEntityBody(byte[] buffer, int offset)
        {
            if (this._contentAvailLength == 0)
            {
                return 0;
            }
            if ((buffer.Length - offset) < this._contentAvailLength)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            int num = this.GetPreloadedPostedContentCore(buffer, offset, this._contentAvailLength);
            if (num < 0)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_read_posted_data"));
            }
            return num;
        }

        public override int GetPreloadedEntityBodyLength()
        {
            return this._contentAvailLength;
        }

        internal abstract int GetPreloadedPostedContentCore(byte[] bytes, int offset, int numBytesToRead);
        public override string GetQueryString()
        {
            if (this._queryStringLength == 0)
            {
                return string.Empty;
            }
            int capacity = this._queryStringLength + 2;
            StringBuilder buffer = new StringBuilder(capacity);
            if (this.GetQueryStringCore(0, buffer, capacity) != 1)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_get_query_string"));
            }
            return buffer.ToString();
        }

        internal abstract int GetQueryStringCore(int encode, StringBuilder buffer, int size);
        public override byte[] GetQueryStringRawBytes()
        {
            if (this._queryStringLength == 0)
            {
                return null;
            }
            byte[] buffer = new byte[this._queryStringLength];
            if (this.GetQueryStringRawBytesCore(buffer, this._queryStringLength) != 1)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_get_query_string_bytes"));
            }
            return buffer;
        }

        internal abstract int GetQueryStringRawBytesCore(byte[] buffer, int size);
        public override string GetRawUrl()
        {
            string queryString = this.GetQueryString();
            if (!string.IsNullOrEmpty(queryString))
            {
                return (this._path + "?" + queryString);
            }
            return this._path;
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
            return 0;
        }

        public override string GetServerName()
        {
            return this.GetServerVariable("SERVER_NAME");
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

        public override IntPtr GetUserToken()
        {
            return this.GetUserTokenCore();
        }

        internal abstract IntPtr GetUserTokenCore();
        public override IntPtr GetVirtualPathToken()
        {
            return this.GetVirtualPathTokenCore();
        }

        internal abstract IntPtr GetVirtualPathTokenCore();
        public override bool HeadersSent()
        {
            return this._headersSent;
        }

        internal void Initialize()
        {
            this.ReadRequestBasics();
            if (((this._appPathTranslated != null) && (this._appPathTranslated.Length > 2)) && !StringUtil.StringEndsWith(this._appPathTranslated, '\\'))
            {
                this._appPathTranslated = this._appPathTranslated + @"\";
            }
            PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, this._contentTotalLength);
        }

        public override bool IsClientConnected()
        {
            return (this.IsClientConnectedCore() != 0);
        }

        internal abstract int IsClientConnectedCore();
        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return (this._contentAvailLength == this._contentTotalLength);
        }

        public override bool IsSecure()
        {
            string serverVariable = this.GetServerVariable("HTTPS");
            return ((serverVariable != null) && serverVariable.Equals("on"));
        }

        public override string MapPath(string path)
        {
            return HostingEnvironment.MapPathInternal(path);
        }

        internal abstract int MapUrlToPathCore(string url, byte[] buffer, int size);
        internal virtual MemoryBytes PackageFile(string filename, long offset64, long length64, bool isImpersonating)
        {
            int offset = Convert.ToInt32(offset64);
            Convert.ToInt32(length64);
            FileStream stream = null;
            MemoryBytes bytes = null;
            try
            {
                stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                int count = ((int) stream.Length) - offset;
                byte[] buffer = new byte[count];
                int size = stream.Read(buffer, offset, count);
                bytes = new MemoryBytes(buffer, size);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return bytes;
        }

        private string[] ReadBasics(int[] contentInfo)
        {
            RecyclableByteBuffer buffer = new RecyclableByteBuffer();
            int num = this.GetBasicsCore(buffer.Buffer, buffer.Buffer.Length, contentInfo);
            while (num < 0)
            {
                buffer.Resize(-num);
                num = this.GetBasicsCore(buffer.Buffer, buffer.Buffer.Length, contentInfo);
            }
            if (num == 0)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_retrieve_request_data"));
            }
            string[] strArray = buffer.GetDecodedTabSeparatedStrings(Encoding.Default, 6, 0);
            buffer.Dispose();
            return strArray;
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            return this.ReadEntityBody(buffer, 0, size);
        }

        public override int ReadEntityBody(byte[] buffer, int offset, int size)
        {
            if ((buffer.Length - offset) < size)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            int num = this.GetAdditionalPostedContentCore(buffer, offset, size);
            if (num < 0)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_read_posted_data"));
            }
            return num;
        }

        internal virtual void ReadRequestBasics()
        {
            int[] contentInfo = new int[4];
            string[] strArray = this.ReadBasics(contentInfo);
            if ((strArray == null) || (strArray.Length != 6))
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_retrieve_request_data"));
            }
            this._contentType = contentInfo[0];
            this._contentTotalLength = contentInfo[1];
            this._contentAvailLength = contentInfo[2];
            this._queryStringLength = contentInfo[3];
            this._method = strArray[0];
            this._filePath = strArray[1];
            this._pathInfo = strArray[2];
            this._path = (this._pathInfo.Length > 0) ? (this._filePath + this._pathInfo) : this._filePath;
            this._pathTranslated = strArray[3];
            this._appPath = strArray[4];
            this._appPathTranslated = strArray[5];
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

        private void ResetCachedResponse()
        {
            this._cachedResponseStatus = null;
            this._cachedResponseHeaders = null;
            this._cachedResponseBodyLength = 0;
            this._cachedResponseBodyBytes = null;
            this._requiresAsyncFlushCallback = false;
            this._ignoreMinAsyncSize = false;
        }

        public override void SendCalculatedContentLength(int contentLength)
        {
            this.SendCalculatedContentLength((long) contentLength);
        }

        public override void SendCalculatedContentLength(long contentLength)
        {
            if (!this._headersSent)
            {
                this._headers.Append("Content-Length: ");
                this._headers.Append(contentLength.ToString(CultureInfo.InvariantCulture));
                this._headers.Append("\r\n");
                this._contentLengthSent = true;
            }
        }

        internal virtual void SendEmptyResponse()
        {
        }

        private void SendHeaders()
        {
            if (!this._headersSent && this._statusSet)
            {
                this._headers.Append("\r\n");
                this.AddHeadersToCachedResponse(this._status.GetEncodedBytesBuffer(), this._headers.GetEncodedBytesBuffer(this._headerEncoding), (this._contentLengthSent || this._chunked) ? 1 : 0);
                this._headersSent = true;
            }
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            if (this._headersSent)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_append_header_after_headers_sent"));
            }
            if (index == 0x1b)
            {
                this.DisableKernelCache();
            }
            this._headers.Append(HttpWorkerRequest.GetKnownResponseHeaderName(index));
            this._headers.Append(": ");
            this._headers.Append(value);
            this._headers.Append("\r\n");
            if (index == 11)
            {
                this._contentLengthSent = true;
            }
            else if (((index == 6) && (value != null)) && value.Equals("chunked"))
            {
                this._chunked = true;
            }
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            if (!this._headersSent)
            {
                this.SendHeaders();
            }
            if (length != 0L)
            {
                FileStream f = null;
                try
                {
                    f = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
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

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            if (!this._headersSent)
            {
                this.SendHeaders();
            }
            if (length != 0L)
            {
                FileStream f = null;
                try
                {
                    f = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
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

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            if (!this._headersSent)
            {
                this.SendHeaders();
            }
            if (length > 0)
            {
                this.AddBodyToCachedResponse(new MemoryBytes(data, length));
            }
        }

        public override void SendResponseFromMemory(IntPtr data, int length)
        {
            this.SendResponseFromMemory(data, length, false);
        }

        internal override void SendResponseFromMemory(IntPtr data, int length, bool isBufferFromUnmanagedPool)
        {
            if (!this._headersSent)
            {
                this.SendHeaders();
            }
            if (length > 0)
            {
                this.AddBodyToCachedResponse(new MemoryBytes(data, length, isBufferFromUnmanagedPool ? BufferType.UnmanagedPool : BufferType.Managed));
            }
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            this._status.Append(statusCode.ToString());
            this._status.Append(" ");
            this._status.Append(statusDescription);
            this._statusSet = true;
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            if (this._headersSent)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_append_header_after_headers_sent"));
            }
            if (StringUtil.EqualsIgnoreCase(name, "Set-Cookie"))
            {
                this.DisableKernelCache();
            }
            this._headers.Append(name);
            this._headers.Append(": ");
            this._headers.Append(value);
            this._headers.Append("\r\n");
        }

        public override void SetEndOfSendNotification(HttpWorkerRequest.EndOfSendNotification callback, object extraData)
        {
            if (this._endOfRequestCallback != null)
            {
                throw new InvalidOperationException();
            }
            this._endOfRequestCallback = callback;
            this._endOfRequestCallbackArg = extraData;
            this._endOfRequestCallbackLockCount = 1;
        }

        internal override void SetHeaderEncoding(Encoding encoding)
        {
            this._headerEncoding = encoding;
        }

        internal override void TransmitFile(string filename, long offset, long length, bool isImpersonating)
        {
            if (!this._headersSent)
            {
                this.SendHeaders();
            }
            if (length != 0L)
            {
                this.AddBodyToCachedResponse(this.PackageFile(filename, offset, length, isImpersonating));
            }
        }

        internal void UnlockCachedResponseBytesOnceAfterIoComplete()
        {
            if (Interlocked.Decrement(ref this._cachedResponseBodyBytesIoLockCount) == 0)
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
        }

        internal IntPtr Ecb
        {
            get
            {
                return this._ecb;
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
    }
}

