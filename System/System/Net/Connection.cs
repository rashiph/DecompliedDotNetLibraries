namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net.Configuration;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    internal class Connection : PooledStream
    {
        private const int AfterCarriageReturn = 5;
        private const int AfterStatusCode = 4;
        private const string BeforeVersionNumberBytes = "HTTP/";
        private const int BeforeVersionNumbers = 0;
        private const long c_InvalidContentLength = -2L;
        private const int CRLFSize = 2;
        private HttpAbortDelegate m_AbortDelegate;
        private bool m_AtLeastOneResponseReceived;
        private int m_BytesRead;
        private int m_BytesScanned;
        private bool m_CanPipeline;
        private System.Net.ConnectionGroup m_ConnectionGroup;
        private UnlockConnectionDelegate m_ConnectionUnlock;
        private HttpWebRequest m_CurrentRequest;
        private WebExceptionStatus m_Error;
        private bool m_Free;
        private bool m_Idle;
        private DateTime m_IdleSinceUtc;
        internal int m_IISVersion;
        internal Exception m_InnerException;
        private bool m_IsPipelinePaused;
        private bool m_KeepAlive;
        private IAsyncResult m_LastAsyncResult;
        private HttpWebRequest m_LockedRequest;
        private int m_MaximumResponseHeadersLength;
        private long m_MaximumUnauthorizedUploadLength;
        private bool m_NonKeepAliveRequestPipelined;
        private WebParseError m_ParseError;
        private bool m_Pipelining;
        private static readonly WaitCallback m_PostReceiveDelegate = new WaitCallback(Connection.PostReceiveWrapper);
        private byte[] m_ReadBuffer;
        private static readonly AsyncCallback m_ReadCallback = new AsyncCallback(Connection.ReadCallbackWrapper);
        private bool m_ReadDone;
        private ReadState m_ReadState;
        private TimerThread.Timer m_RecycleTimer;
        private bool m_RemovedFromConnectionList;
        private int m_ReservedCount;
        private CoreResponseData m_ResponseData;
        private StatusLineValues m_StatusLineValues;
        private int m_StatusState;
        private int m_TotalResponseHeadersLength;
        private static readonly AsyncCallback m_TunnelCallback = new AsyncCallback(Connection.TunnelThroughProxyWrapper);
        private List<WaitListItem> m_WaitList;
        private bool m_WriteDone;
        private ArrayList m_WriteList;
        private const int MajorVersionNumber = 1;
        private const int MinorVersionNumber = 2;
        private static int s_MaxPipelinedCount = 10;
        private static int s_MinPipelinedCount = 5;
        private static byte[] s_NullBuffer = new byte[0];
        private static readonly string[] s_ShortcutStatusDescriptions = new string[] { "OK", "Continue", "Unauthorized" };
        private const int StatusCodeNumber = 3;
        [ThreadStatic]
        private static int t_SyncReadNesting;

        internal Connection(System.Net.ConnectionGroup connectionGroup) : base(null)
        {
            this.m_IISVersion = -1;
            this.m_Free = true;
            this.m_Idle = true;
            this.m_KeepAlive = true;
            this.m_MaximumUnauthorizedUploadLength = SettingsSectionInternal.Section.MaximumUnauthorizedUploadLength;
            if (this.m_MaximumUnauthorizedUploadLength > 0L)
            {
                this.m_MaximumUnauthorizedUploadLength *= 0x400L;
            }
            this.m_ResponseData = new CoreResponseData();
            this.m_ConnectionGroup = connectionGroup;
            this.m_ReadBuffer = new byte[0x1000];
            this.m_ReadState = ReadState.Start;
            this.m_WaitList = new List<WaitListItem>();
            this.m_WriteList = new ArrayList();
            this.m_AbortDelegate = new HttpAbortDelegate(this.AbortOrDisassociate);
            this.m_ConnectionUnlock = new UnlockConnectionDelegate(this.UnlockRequest);
            this.m_StatusLineValues = new StatusLineValues();
            this.m_RecycleTimer = this.ConnectionGroup.ServicePoint.ConnectionLeaseTimerQueue.CreateTimer();
            this.ConnectionGroup.Associate(this);
            this.m_ReadDone = true;
            this.m_WriteDone = true;
            this.m_Error = WebExceptionStatus.Success;
        }

        internal bool AbortOrDisassociate(HttpWebRequest request, WebException webException)
        {
            Predicate<WaitListItem> match = null;
            ConnectionReturnResult returnResult = null;
            lock (this)
            {
                int index = this.m_WriteList.IndexOf(request);
                if (index == -1)
                {
                    WaitListItem item = null;
                    if (this.m_WaitList.Count > 0)
                    {
                        if (match == null)
                        {
                            match = o => object.ReferenceEquals(o.Request, request);
                        }
                        item = this.m_WaitList.Find(match);
                    }
                    if (item != null)
                    {
                        NetworkingPerfCounters.Instance.IncrementAverage(NetworkingPerfCounterName.HttpWebRequestAvgQueueTime, item.QueueStartTime);
                        this.m_WaitList.Remove(item);
                    }
                    return true;
                }
                if (index != 0)
                {
                    this.m_WriteList.RemoveAt(index);
                    this.m_KeepAlive = false;
                    return true;
                }
                this.m_KeepAlive = false;
                if ((webException != null) && (this.m_InnerException == null))
                {
                    this.m_InnerException = webException;
                    this.m_Error = webException.Status;
                }
                else
                {
                    this.m_Error = WebExceptionStatus.RequestCanceled;
                }
                this.PrepareCloseConnectionSocket(ref returnResult);
                base.Close(0);
            }
            ConnectionReturnResult.SetResponses(returnResult);
            return false;
        }

        internal void AbortSocket(bool isAbortState)
        {
            if (isAbortState)
            {
                this.UnlockRequest();
                this.CheckIdle();
            }
            else
            {
                this.m_Error = WebExceptionStatus.KeepAliveFailure;
            }
            lock (this)
            {
                base.Close(0);
            }
        }

        private void CheckIdle()
        {
            if (!this.m_Idle && (this.BusyCount == 0))
            {
                this.m_Idle = true;
                this.ServicePoint.DecrementConnection();
                if (this.ConnectionGroup != null)
                {
                    this.ConnectionGroup.ConnectionGoneIdle();
                }
                this.m_IdleSinceUtc = DateTime.UtcNow;
            }
        }

        private HttpWebRequest CheckNextRequest()
        {
            if (this.m_WaitList.Count == 0)
            {
                this.m_Free = this.m_KeepAlive;
                return null;
            }
            if (!base.CanBePooled)
            {
                return null;
            }
            WaitListItem item = this.m_WaitList[0];
            HttpWebRequest request = item.Request;
            if (this.m_IsPipelinePaused)
            {
                this.m_IsPipelinePaused = this.m_WriteList.Count > s_MinPipelinedCount;
            }
            if (((!request.Pipelined || request.HasEntityBody) || ((!this.m_CanPipeline || !this.m_Pipelining) || this.m_IsPipelinePaused)) && (this.m_WriteList.Count != 0))
            {
                request = null;
            }
            if (request != null)
            {
                NetworkingPerfCounters.Instance.IncrementAverage(NetworkingPerfCounterName.HttpWebRequestAvgQueueTime, item.QueueStartTime);
                this.m_WaitList.RemoveAt(0);
                this.CheckIdle();
            }
            return request;
        }

        private void CheckNonIdle()
        {
            if (this.m_Idle && (this.BusyCount != 0))
            {
                this.m_Idle = false;
                this.ServicePoint.IncrementConnection();
            }
        }

        internal void CheckStartReceive(HttpWebRequest request)
        {
            lock (this)
            {
                request.HeadersCompleted = true;
                if ((this.m_WriteList.Count == 0) || (!this.m_ReadDone || (this.m_WriteList[0] != request)))
                {
                    return;
                }
                this.m_ReadDone = false;
                this.m_CurrentRequest = (HttpWebRequest) this.m_WriteList[0];
            }
            if (!request.Async)
            {
                request.ConnectionReaderAsyncResult.InvokeCallback();
            }
            else if (this.m_BytesScanned < this.m_BytesRead)
            {
                this.ReadComplete(0, WebExceptionStatus.Success);
            }
            else if (Thread.CurrentThread.IsThreadPoolThread)
            {
                this.PostReceive();
            }
            else
            {
                ThreadPool.UnsafeQueueUserWorkItem(m_PostReceiveDelegate, this);
            }
        }

        private void ClearReaderState()
        {
            this.m_BytesRead = 0;
            this.m_BytesScanned = 0;
        }

        internal void CloseOnIdle()
        {
            lock (this)
            {
                this.m_KeepAlive = false;
                this.m_RemovedFromConnectionList = true;
                if (!this.m_Idle)
                {
                    this.CheckIdle();
                }
                if (this.m_Idle)
                {
                    this.AbortSocket(false);
                    GC.SuppressFinalize(this);
                }
            }
        }

        private void CompleteConnection(bool async, HttpWebRequest request)
        {
            WebExceptionStatus connectFailure = WebExceptionStatus.ConnectFailure;
            if (request.Async)
            {
                request.OpenWriteSideResponseWindow();
            }
            try
            {
                try
                {
                    if (request.Address.Scheme == Uri.UriSchemeHttps)
                    {
                        TlsStream stream = new TlsStream(request.GetRemoteResourceUri().Host, base.NetworkStream, request.ClientCertificates, this.ServicePoint, request, request.Async ? request.GetConnectingContext().ContextCopy : null);
                        base.NetworkStream = stream;
                    }
                }
                finally
                {
                    this.m_ReadState = ReadState.Start;
                    this.ClearReaderState();
                    request.SetRequestSubmitDone(new ConnectStream(this, request));
                    connectFailure = WebExceptionStatus.Success;
                }
            }
            catch (Exception exception)
            {
                if (this.m_InnerException == null)
                {
                    this.m_InnerException = exception;
                }
                WebException exception2 = exception as WebException;
                if (exception2 != null)
                {
                    connectFailure = exception2.Status;
                }
            }
            if (connectFailure != WebExceptionStatus.Success)
            {
                ConnectionReturnResult returnResult = null;
                this.HandleError(false, false, connectFailure, ref returnResult);
                ConnectionReturnResult.SetResponses(returnResult);
            }
        }

        private void CompleteConnectionWrapper(object request, object state)
        {
            Exception exception = state as Exception;
            if (exception != null)
            {
                ConnectionReturnResult returnResult = null;
                if (this.m_InnerException == null)
                {
                    this.m_InnerException = exception;
                }
                this.HandleError(false, false, WebExceptionStatus.ConnectFailure, ref returnResult);
                ConnectionReturnResult.SetResponses(returnResult);
            }
            this.CompleteConnection(true, (HttpWebRequest) request);
        }

        private void CompleteStartConnection(bool async, HttpWebRequest httpWebRequest)
        {
            WebExceptionStatus connectFailure = WebExceptionStatus.ConnectFailure;
            this.m_InnerException = null;
            bool flag = true;
            try
            {
                if ((httpWebRequest.Address.Scheme == Uri.UriSchemeHttps) && this.ServicePoint.InternalProxyServicePoint)
                {
                    if (!this.TunnelThroughProxy(this.ServicePoint.InternalAddress, httpWebRequest, async))
                    {
                        connectFailure = WebExceptionStatus.ConnectFailure;
                        flag = false;
                    }
                    if (async && flag)
                    {
                        return;
                    }
                }
                else
                {
                    TimerThread.Timer requestTimer = httpWebRequest.RequestTimer;
                    if (!base.Activate(httpWebRequest, async, (requestTimer != null) ? requestTimer.TimeRemaining : 0, new GeneralAsyncDelegate(this.CompleteConnectionWrapper)))
                    {
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                if (this.m_InnerException == null)
                {
                    this.m_InnerException = exception;
                }
                if (exception is WebException)
                {
                    connectFailure = ((WebException) exception).Status;
                }
                flag = false;
            }
            if (flag)
            {
                this.CompleteConnection(async, httpWebRequest);
            }
            else
            {
                ConnectionReturnResult returnResult = null;
                this.HandleError(false, false, connectFailure, ref returnResult);
                ConnectionReturnResult.SetResponses(returnResult);
            }
        }

        private void CompleteStartRequest(bool onSubmitThread, HttpWebRequest request, TriState needReConnect)
        {
            if (needReConnect == TriState.True)
            {
                try
                {
                    if (request.Async)
                    {
                        this.CompleteStartConnection(true, request);
                    }
                    else if (onSubmitThread)
                    {
                        this.CompleteStartConnection(false, request);
                    }
                }
                catch (Exception exception)
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                }
                if (!request.Async)
                {
                    request.ConnectionAsyncResult.InvokeCallback(new AsyncTriState(needReConnect));
                }
            }
            else
            {
                if (request.Async)
                {
                    request.OpenWriteSideResponseWindow();
                }
                ConnectStream submitStream = new ConnectStream(this, request);
                if (request.Async || onSubmitThread)
                {
                    request.SetRequestSubmitDone(submitStream);
                }
                else
                {
                    request.ConnectionAsyncResult.InvokeCallback(submitStream);
                }
            }
        }

        [Conditional("DEBUG")]
        internal void Debug(int requestHash)
        {
        }

        [Conditional("TRAVE")]
        private void DebugDumpListEntry(int currentPos, HttpWebRequest req, string listType)
        {
        }

        [Conditional("TRAVE")]
        private void DebugDumpWaitListEntries()
        {
            for (int i = 0; i < this.m_WaitList.Count; i++)
            {
            }
        }

        [Conditional("TRAVE")]
        private void DebugDumpWriteListEntries()
        {
            for (int i = 0; i < this.m_WriteList.Count; i++)
            {
            }
        }

        private static int FindChunkEntitySize(byte[] buffer, int offset, int size)
        {
            BufferChunkBytes source = new BufferChunkBytes();
            int num2 = offset;
            int num = offset + size;
            source.Buffer = buffer;
            while (offset < num)
            {
                int num4;
                source.Offset = offset;
                source.Count = size;
                int chunkSize = ChunkParse.GetChunkSize(source, out num4);
                switch (chunkSize)
                {
                    case -1:
                        return -1;

                    case 0:
                        return 0;
                }
                offset += chunkSize;
                size -= chunkSize;
                if (num4 != 0)
                {
                    source.Offset = offset;
                    source.Count = size;
                    chunkSize = ChunkParse.SkipPastCRLF(source);
                    if (chunkSize <= 0)
                    {
                        return chunkSize;
                    }
                    offset += chunkSize;
                    size -= chunkSize;
                    offset += num4 + 2;
                    size -= num4 + 2;
                }
                else
                {
                    if (size >= 2)
                    {
                        offset += 2;
                        size -= 2;
                        while (((size >= 2) && (buffer[offset] != 13)) && (buffer[offset + 1] != 10))
                        {
                            source.Offset = offset;
                            source.Count = size;
                            chunkSize = ChunkParse.SkipPastCRLF(source);
                            if (chunkSize <= 0)
                            {
                                return chunkSize;
                            }
                            offset += chunkSize;
                            size -= chunkSize;
                        }
                        if (size >= 2)
                        {
                            return ((offset + 2) - num2);
                        }
                    }
                    return -1;
                }
            }
            return -1;
        }

        internal void HandleConnectStreamException(bool writeDone, bool readDone, WebExceptionStatus webExceptionStatus, ref ConnectionReturnResult returnResult, Exception e)
        {
            if (this.m_InnerException == null)
            {
                this.m_InnerException = e;
                if (!(e is WebException) && (base.NetworkStream is TlsStream))
                {
                    webExceptionStatus = ((TlsStream) base.NetworkStream).ExceptionStatus;
                }
                else if (e is ObjectDisposedException)
                {
                    webExceptionStatus = WebExceptionStatus.RequestCanceled;
                }
            }
            this.HandleError(writeDone, readDone, webExceptionStatus, ref returnResult);
        }

        private void HandleError(bool writeDone, bool readDone, WebExceptionStatus webExceptionStatus, ref ConnectionReturnResult returnResult)
        {
            lock (this)
            {
                if (writeDone)
                {
                    this.m_WriteDone = true;
                }
                if (readDone)
                {
                    this.m_ReadDone = true;
                }
                if (webExceptionStatus == WebExceptionStatus.Success)
                {
                    throw new InternalException();
                }
                this.m_Error = webExceptionStatus;
                this.PrepareCloseConnectionSocket(ref returnResult);
                base.Close(0);
            }
        }

        private void HandleErrorWithReadDone(WebExceptionStatus webExceptionStatus, ref ConnectionReturnResult returnResult)
        {
            this.HandleError(false, true, webExceptionStatus, ref returnResult);
        }

        private void InitializeParseStatusLine()
        {
            this.m_StatusState = 0;
            this.m_StatusLineValues.MajorVersion = 0;
            this.m_StatusLineValues.MinorVersion = 0;
            this.m_StatusLineValues.StatusCode = 0;
            this.m_StatusLineValues.StatusDescription = null;
        }

        private void InternalWriteStartNextRequest(HttpWebRequest request, ref bool calledCloseConnection, ref TriState startRequestResult, ref HttpWebRequest nextRequest, ref ConnectionReturnResult returnResult)
        {
            lock (this)
            {
                this.m_WriteDone = true;
                if ((!this.m_KeepAlive || (this.m_Error != WebExceptionStatus.Success)) || !base.CanBePooled)
                {
                    if (this.m_ReadDone)
                    {
                        if (this.m_Error == WebExceptionStatus.Success)
                        {
                            this.m_Error = WebExceptionStatus.KeepAliveFailure;
                        }
                        this.PrepareCloseConnectionSocket(ref returnResult);
                        calledCloseConnection = true;
                        this.Close();
                    }
                    else if (this.m_Error != WebExceptionStatus.Success)
                    {
                    }
                }
                else
                {
                    if (this.m_Pipelining || this.m_ReadDone)
                    {
                        nextRequest = this.CheckNextRequest();
                    }
                    if (nextRequest != null)
                    {
                        startRequestResult = this.StartRequest(nextRequest, false);
                    }
                }
            }
        }

        internal void MarkAsReserved()
        {
            Interlocked.Increment(ref this.m_ReservedCount);
        }

        private DataParseStatus ParseResponseData(ref ConnectionReturnResult returnResult, out bool requestDone, out CoreResponseData continueResponseData)
        {
            DataParseStatus status2;
            DataParseStatus needMoreData = DataParseStatus.NeedMoreData;
            requestDone = false;
            continueResponseData = null;
            switch (this.m_ReadState)
            {
                case ReadState.Start:
                    break;

                case ReadState.StatusLine:
                    goto Label_00F6;

                case ReadState.Headers:
                    goto Label_0299;

                case ReadState.Data:
                    goto Label_050A;

                default:
                    goto Label_0515;
            }
        Label_002C:
            if (this.m_CurrentRequest == null)
            {
                lock (this)
                {
                    if ((this.m_WriteList.Count == 0) || ((this.m_CurrentRequest = this.m_WriteList[0] as HttpWebRequest) == null))
                    {
                        this.m_ParseError.Section = WebParseErrorSection.Generic;
                        this.m_ParseError.Code = WebParseErrorCode.Generic;
                        needMoreData = DataParseStatus.Invalid;
                        goto Label_0515;
                    }
                }
            }
            this.m_KeepAlive &= this.m_CurrentRequest.KeepAlive || this.m_CurrentRequest.NtlmKeepAlive;
            this.m_MaximumResponseHeadersLength = this.m_CurrentRequest.MaximumResponseHeadersLength * 0x400;
            this.m_ResponseData = new CoreResponseData();
            this.m_ReadState = ReadState.StatusLine;
            this.m_TotalResponseHeadersLength = 0;
            this.InitializeParseStatusLine();
        Label_00F6:
            if (SettingsSectionInternal.Section.UseUnsafeHeaderParsing)
            {
                int[] numArray2 = new int[4];
                numArray2[1] = this.m_StatusLineValues.MajorVersion;
                numArray2[2] = this.m_StatusLineValues.MinorVersion;
                numArray2[3] = this.m_StatusLineValues.StatusCode;
                int[] statusLineInts = numArray2;
                if (this.m_StatusLineValues.StatusDescription == null)
                {
                    this.m_StatusLineValues.StatusDescription = "";
                }
                status2 = this.ParseStatusLine(this.m_ReadBuffer, this.m_BytesRead, ref this.m_BytesScanned, ref statusLineInts, ref this.m_StatusLineValues.StatusDescription, ref this.m_StatusState, ref this.m_ParseError);
                this.m_StatusLineValues.MajorVersion = statusLineInts[1];
                this.m_StatusLineValues.MinorVersion = statusLineInts[2];
                this.m_StatusLineValues.StatusCode = statusLineInts[3];
            }
            else
            {
                status2 = ParseStatusLineStrict(this.m_ReadBuffer, this.m_BytesRead, ref this.m_BytesScanned, ref this.m_StatusState, this.m_StatusLineValues, this.m_MaximumResponseHeadersLength, ref this.m_TotalResponseHeadersLength, ref this.m_ParseError);
            }
            if (status2 == DataParseStatus.Done)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_received_status_line", new object[] { this.m_StatusLineValues.MajorVersion + "." + this.m_StatusLineValues.MinorVersion, this.m_StatusLineValues.StatusCode, this.m_StatusLineValues.StatusDescription }));
                }
                this.SetStatusLineParsed();
                this.m_ReadState = ReadState.Headers;
                this.m_ResponseData.m_ResponseHeaders = new WebHeaderCollection(WebHeaderCollectionType.HttpWebResponse);
            }
            else
            {
                if (status2 != DataParseStatus.NeedMoreData)
                {
                    needMoreData = status2;
                }
                goto Label_0515;
            }
        Label_0299:
            if (this.m_BytesScanned >= this.m_BytesRead)
            {
                goto Label_0515;
            }
            if (SettingsSectionInternal.Section.UseUnsafeHeaderParsing)
            {
                status2 = this.m_ResponseData.m_ResponseHeaders.ParseHeaders(this.m_ReadBuffer, this.m_BytesRead, ref this.m_BytesScanned, ref this.m_TotalResponseHeadersLength, this.m_MaximumResponseHeadersLength, ref this.m_ParseError);
            }
            else
            {
                status2 = this.m_ResponseData.m_ResponseHeaders.ParseHeadersStrict(this.m_ReadBuffer, this.m_BytesRead, ref this.m_BytesScanned, ref this.m_TotalResponseHeadersLength, this.m_MaximumResponseHeadersLength, ref this.m_ParseError);
            }
            if ((status2 == DataParseStatus.Invalid) || (status2 == DataParseStatus.DataTooBig))
            {
                needMoreData = status2;
                goto Label_0515;
            }
            if (status2 != DataParseStatus.Done)
            {
                goto Label_0515;
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_received_headers", new object[] { this.m_ResponseData.m_ResponseHeaders.ToString(true) }));
            }
            if (this.m_IISVersion == -1)
            {
                int num;
                string server = this.m_ResponseData.m_ResponseHeaders.Server;
                if (((server != null) && server.ToLower(CultureInfo.InvariantCulture).Contains("microsoft-iis")) && ((server.IndexOf("/")++ > 0) && (num < server.Length)))
                {
                    this.m_IISVersion = server[num++] - '0';
                    while ((num < server.Length) && char.IsDigit(server[num]))
                    {
                        this.m_IISVersion = ((this.m_IISVersion * 10) + server[num++]) - 0x30;
                    }
                }
                if ((this.m_IISVersion == -1) && (this.m_ResponseData.m_StatusCode != HttpStatusCode.Continue))
                {
                    this.m_IISVersion = 0;
                }
            }
            if ((this.m_ResponseData.m_StatusCode == HttpStatusCode.Continue) || (this.m_ResponseData.m_StatusCode == HttpStatusCode.BadRequest))
            {
                if (this.m_ResponseData.m_StatusCode == HttpStatusCode.BadRequest)
                {
                    if (((this.ServicePoint.HttpBehaviour == HttpBehaviour.HTTP11) && (this.m_CurrentRequest.HttpWriteMode == HttpWriteMode.Chunked)) && ((this.m_ResponseData.m_ResponseHeaders.Via != null) && (string.Compare(this.m_ResponseData.m_StatusDescription, "Bad Request ( The HTTP request includes a non-supported header. Contact the Server administrator.  )", StringComparison.OrdinalIgnoreCase) == 0)))
                    {
                        this.ServicePoint.HttpBehaviour = HttpBehaviour.HTTP11PartiallyCompliant;
                    }
                }
                else
                {
                    this.m_CurrentRequest.Saw100Continue = true;
                    if (!this.ServicePoint.Understands100Continue)
                    {
                        this.ServicePoint.Understands100Continue = true;
                    }
                    continueResponseData = this.m_ResponseData;
                    goto Label_002C;
                }
            }
            this.m_ReadState = ReadState.Data;
        Label_050A:
            requestDone = true;
            needMoreData = this.ParseStreamData(ref returnResult);
        Label_0515:
            if (this.m_BytesScanned == this.m_BytesRead)
            {
                this.ClearReaderState();
            }
            return needMoreData;
        }

        private DataParseStatus ParseStatusLine(byte[] statusLine, int statusLineLength, ref int bytesParsed, ref int[] statusLineInts, ref string statusDescription, ref int statusState, ref WebParseError parseError)
        {
            DataParseStatus done = DataParseStatus.Done;
            int byteIndex = -1;
            int num2 = 0;
            while (((bytesParsed < statusLineLength) && (statusLine[bytesParsed] != 13)) && (statusLine[bytesParsed] != 10))
            {
                switch (statusState)
                {
                    case 0:
                        if (statusLine[bytesParsed] != 0x2f)
                        {
                            break;
                        }
                        statusState++;
                        goto Label_00DA;

                    case 1:
                        if (statusLine[bytesParsed] != 0x2e)
                        {
                            goto Label_0069;
                        }
                        statusState++;
                        goto Label_00DA;

                    case 2:
                        goto Label_0069;

                    case 3:
                        goto Label_007A;

                    case 4:
                        if (statusLine[bytesParsed] != 0x20)
                        {
                            num2 = bytesParsed;
                            if (byteIndex == -1)
                            {
                                byteIndex = bytesParsed;
                            }
                        }
                        goto Label_00DA;

                    default:
                        goto Label_00DA;
                }
                if (statusLine[bytesParsed] == 0x20)
                {
                    statusState = 3;
                }
                goto Label_00DA;
            Label_0069:
                if (statusLine[bytesParsed] == 0x20)
                {
                    statusState++;
                    goto Label_00DA;
                }
            Label_007A:
                if (char.IsDigit((char) statusLine[bytesParsed]))
                {
                    int num3 = statusLine[bytesParsed] - 0x30;
                    statusLineInts[statusState] = (statusLineInts[statusState] * 10) + num3;
                }
                else if (statusLineInts[3] > 0)
                {
                    statusState++;
                }
                else if (!char.IsWhiteSpace((char) statusLine[bytesParsed]))
                {
                    statusLineInts[statusState] = -1;
                }
            Label_00DA:
                bytesParsed++;
                if ((this.m_MaximumResponseHeadersLength >= 0) && (++this.m_TotalResponseHeadersLength >= this.m_MaximumResponseHeadersLength))
                {
                    done = DataParseStatus.DataTooBig;
                    goto Label_01CA;
                }
            }
            if (byteIndex != -1)
            {
                statusDescription = statusDescription + WebHeaderCollection.HeaderEncoding.GetString(statusLine, byteIndex, (num2 - byteIndex) + 1);
            }
            if (bytesParsed != statusLineLength)
            {
                while ((bytesParsed < statusLineLength) && ((statusLine[bytesParsed] == 13) || (statusLine[bytesParsed] == 0x20)))
                {
                    bytesParsed++;
                    if ((this.m_MaximumResponseHeadersLength >= 0) && (++this.m_TotalResponseHeadersLength >= this.m_MaximumResponseHeadersLength))
                    {
                        done = DataParseStatus.DataTooBig;
                        goto Label_01CA;
                    }
                }
                if (bytesParsed == statusLineLength)
                {
                    done = DataParseStatus.NeedMoreData;
                }
                else if (statusLine[bytesParsed] == 10)
                {
                    bytesParsed++;
                    if ((this.m_MaximumResponseHeadersLength >= 0) && (++this.m_TotalResponseHeadersLength >= this.m_MaximumResponseHeadersLength))
                    {
                        done = DataParseStatus.DataTooBig;
                    }
                    else
                    {
                        done = DataParseStatus.Done;
                    }
                }
            }
            else
            {
                return DataParseStatus.NeedMoreData;
            }
        Label_01CA:
            if (((done == DataParseStatus.Done) && (statusState != 4)) && ((statusState != 3) || (statusLineInts[3] <= 0)))
            {
                done = DataParseStatus.Invalid;
            }
            if (done == DataParseStatus.Invalid)
            {
                parseError.Section = WebParseErrorSection.ResponseStatusLine;
                parseError.Code = WebParseErrorCode.Generic;
            }
            return done;
        }

        private static unsafe DataParseStatus ParseStatusLineStrict(byte[] statusLine, int statusLineLength, ref int bytesParsed, ref int statusState, StatusLineValues statusLineValues, int maximumHeaderLength, ref int totalBytesParsed, ref WebParseError parseError)
        {
            int num = bytesParsed;
            DataParseStatus dataTooBig = DataParseStatus.DataTooBig;
            int num2 = (maximumHeaderLength <= 0) ? 0x7fffffff : ((maximumHeaderLength - totalBytesParsed) + bytesParsed);
            if (statusLineLength < num2)
            {
                dataTooBig = DataParseStatus.NeedMoreData;
                num2 = statusLineLength;
            }
            if (bytesParsed < num2)
            {
                try
                {
                    fixed (byte* numRef = statusLine)
                    {
                        switch (statusState)
                        {
                            case 0:
                                goto Label_00A2;

                            case 1:
                                goto Label_0115;

                            case 2:
                                goto Label_0190;

                            case 3:
                                goto Label_01FB;

                            case 4:
                                goto Label_029B;

                            case 5:
                                goto Label_0423;

                            default:
                                goto Label_043F;
                        }
                    Label_006D:
                        if (((byte) "HTTP/"[(totalBytesParsed - num) + bytesParsed]) != numRef[bytesParsed])
                        {
                            dataTooBig = DataParseStatus.Invalid;
                            goto Label_043F;
                        }
                        if (++bytesParsed == num2)
                        {
                            goto Label_043F;
                        }
                    Label_00A2:
                        if (((totalBytesParsed - num) + bytesParsed) < "HTTP/".Length)
                        {
                            goto Label_006D;
                        }
                        if (numRef[bytesParsed] == 0x2e)
                        {
                            dataTooBig = DataParseStatus.Invalid;
                            goto Label_043F;
                        }
                        statusState = 1;
                    Label_0115:
                        while (numRef[bytesParsed] != 0x2e)
                        {
                            if ((numRef[bytesParsed] < 0x30) || (numRef[bytesParsed] > 0x39))
                            {
                                dataTooBig = DataParseStatus.Invalid;
                                goto Label_043F;
                            }
                            statusLineValues.MajorVersion = ((statusLineValues.MajorVersion * 10) + numRef[bytesParsed]) - 0x30;
                            if (++bytesParsed == num2)
                            {
                                goto Label_043F;
                            }
                        }
                        if ((bytesParsed + 1) == num2)
                        {
                            goto Label_043F;
                        }
                        bytesParsed++;
                        if (numRef[bytesParsed] == 0x20)
                        {
                            dataTooBig = DataParseStatus.Invalid;
                            goto Label_043F;
                        }
                        statusState = 2;
                    Label_0190:
                        while (numRef[bytesParsed] != 0x20)
                        {
                            if ((numRef[bytesParsed] < 0x30) || (numRef[bytesParsed] > 0x39))
                            {
                                dataTooBig = DataParseStatus.Invalid;
                                goto Label_043F;
                            }
                            statusLineValues.MinorVersion = ((statusLineValues.MinorVersion * 10) + numRef[bytesParsed]) - 0x30;
                            if (++bytesParsed == num2)
                            {
                                goto Label_043F;
                            }
                        }
                        statusState = 3;
                        statusLineValues.StatusCode = 1;
                        if (++bytesParsed != num2)
                        {
                            goto Label_01FB;
                        }
                        goto Label_043F;
                    Label_01B8:
                        if (statusLineValues.StatusCode >= 0x3e8)
                        {
                            dataTooBig = DataParseStatus.Invalid;
                            goto Label_043F;
                        }
                        statusLineValues.StatusCode = ((statusLineValues.StatusCode * 10) + numRef[bytesParsed]) - 0x30;
                        if (++bytesParsed == num2)
                        {
                            goto Label_043F;
                        }
                    Label_01FB:
                        if ((numRef[bytesParsed] >= 0x30) && (numRef[bytesParsed] <= 0x39))
                        {
                            goto Label_01B8;
                        }
                        if ((numRef[bytesParsed] != 0x20) || (statusLineValues.StatusCode < 0x3e8))
                        {
                            if ((numRef[bytesParsed] == 13) && (statusLineValues.StatusCode >= 0x3e8))
                            {
                                statusLineValues.StatusCode -= 0x3e8;
                                statusState = 5;
                                if (++bytesParsed != num2)
                                {
                                    goto Label_0423;
                                }
                            }
                            else
                            {
                                dataTooBig = DataParseStatus.Invalid;
                            }
                            goto Label_043F;
                        }
                        statusLineValues.StatusCode -= 0x3e8;
                        statusState = 4;
                        if (++bytesParsed == num2)
                        {
                            goto Label_043F;
                        }
                    Label_029B:
                        if (statusLineValues.StatusDescription == null)
                        {
                            foreach (string str in s_ShortcutStatusDescriptions)
                            {
                                if ((bytesParsed >= (num2 - str.Length)) || (numRef[bytesParsed] != ((byte) str[0])))
                                {
                                    continue;
                                }
                                byte* numPtr = (numRef + bytesParsed) + 1;
                                int num3 = 1;
                                while (num3 < str.Length)
                                {
                                    numPtr++;
                                    if (numPtr[0] != ((byte) str[num3]))
                                    {
                                        break;
                                    }
                                    num3++;
                                }
                                if (num3 == str.Length)
                                {
                                    statusLineValues.StatusDescription = str;
                                    bytesParsed += str.Length;
                                }
                                break;
                            }
                        }
                        int num4 = bytesParsed;
                        while (numRef[bytesParsed] != 13)
                        {
                            if ((numRef[bytesParsed] < 0x20) || (numRef[bytesParsed] == 0x7f))
                            {
                                dataTooBig = DataParseStatus.Invalid;
                                goto Label_043F;
                            }
                            if (++bytesParsed == num2)
                            {
                                string str2 = WebHeaderCollection.HeaderEncoding.GetString(numRef + num4, bytesParsed - num4);
                                if (statusLineValues.StatusDescription == null)
                                {
                                    statusLineValues.StatusDescription = str2;
                                }
                                else
                                {
                                    statusLineValues.StatusDescription = statusLineValues.StatusDescription + str2;
                                }
                                goto Label_043F;
                            }
                        }
                        if (bytesParsed > num4)
                        {
                            string str3 = WebHeaderCollection.HeaderEncoding.GetString(numRef + num4, bytesParsed - num4);
                            if (statusLineValues.StatusDescription == null)
                            {
                                statusLineValues.StatusDescription = str3;
                            }
                            else
                            {
                                statusLineValues.StatusDescription = statusLineValues.StatusDescription + str3;
                            }
                        }
                        else if (statusLineValues.StatusDescription == null)
                        {
                            statusLineValues.StatusDescription = "";
                        }
                        statusState = 5;
                        if (++bytesParsed == num2)
                        {
                            goto Label_043F;
                        }
                    Label_0423:
                        if (numRef[bytesParsed] != 10)
                        {
                            dataTooBig = DataParseStatus.Invalid;
                        }
                        else
                        {
                            dataTooBig = DataParseStatus.Done;
                            bytesParsed++;
                        }
                    }
                }
                finally
                {
                    numRef = null;
                }
            }
        Label_043F:
            totalBytesParsed += bytesParsed - num;
            if (dataTooBig == DataParseStatus.Invalid)
            {
                parseError.Section = WebParseErrorSection.ResponseStatusLine;
                parseError.Code = WebParseErrorCode.Generic;
            }
            return dataTooBig;
        }

        private DataParseStatus ParseStreamData(ref ConnectionReturnResult returnResult)
        {
            bool flag2;
            int num3;
            DataParseStatus continueParsing;
            if (this.m_CurrentRequest == null)
            {
                this.m_ParseError.Section = WebParseErrorSection.Generic;
                this.m_ParseError.Code = WebParseErrorCode.UnexpectedServerResponse;
                return DataParseStatus.Invalid;
            }
            bool fHaveChunked = false;
            long num = this.ProcessHeaderData(ref fHaveChunked, this.m_CurrentRequest, out flag2);
            if (num == -2L)
            {
                this.m_ParseError.Section = WebParseErrorSection.ResponseHeader;
                this.m_ParseError.Code = WebParseErrorCode.InvalidContentLength;
                return DataParseStatus.Invalid;
            }
            int size = this.m_BytesRead - this.m_BytesScanned;
            if (this.m_ResponseData.m_StatusCode > ((HttpStatusCode) 0x12b))
            {
                this.m_CurrentRequest.ErrorStatusCodeNotify(this, this.m_KeepAlive, false);
            }
            if (flag2)
            {
                num3 = 0;
                fHaveChunked = false;
            }
            else if (fHaveChunked)
            {
                num3 = FindChunkEntitySize(this.m_ReadBuffer, this.m_BytesScanned, size);
                if (num3 == 0)
                {
                    this.m_ParseError.Section = WebParseErrorSection.ResponseBody;
                    this.m_ParseError.Code = WebParseErrorCode.InvalidChunkFormat;
                    return DataParseStatus.Invalid;
                }
            }
            else if (num > 0x7fffffffL)
            {
                num3 = -1;
            }
            else
            {
                num3 = (int) num;
            }
            if ((num3 != -1) && (num3 <= size))
            {
                this.m_ResponseData.m_ConnectStream = new ConnectStream(this, this.m_ReadBuffer, this.m_BytesScanned, num3, flag2 ? 0L : num, fHaveChunked, this.m_CurrentRequest);
                continueParsing = DataParseStatus.ContinueParsing;
                this.m_BytesScanned += num3;
            }
            else
            {
                this.m_ResponseData.m_ConnectStream = new ConnectStream(this, this.m_ReadBuffer, this.m_BytesScanned, size, flag2 ? 0L : num, fHaveChunked, this.m_CurrentRequest);
                continueParsing = DataParseStatus.Done;
                this.ClearReaderState();
            }
            this.m_ResponseData.m_ContentLength = num;
            ConnectionReturnResult.Add(ref returnResult, this.m_CurrentRequest, this.m_ResponseData.Clone());
            return continueParsing;
        }

        internal void PollAndRead(HttpWebRequest request, bool userRetrievedStream)
        {
            request.SawInitialResponse = false;
            if ((request.ConnectionReaderAsyncResult.InternalPeekCompleted && (request.ConnectionReaderAsyncResult.Result == null)) && base.CanBePooled)
            {
                this.SyncRead(request, userRetrievedStream, true);
            }
        }

        private void PostReceive()
        {
            try
            {
                if ((this.m_LastAsyncResult != null) && !this.m_LastAsyncResult.IsCompleted)
                {
                    throw new InternalException();
                }
                this.m_LastAsyncResult = this.UnsafeBeginRead(this.m_ReadBuffer, this.m_BytesRead, this.m_ReadBuffer.Length - this.m_BytesRead, m_ReadCallback, this);
                if (this.m_LastAsyncResult.CompletedSynchronously)
                {
                    this.ReadCallback(this.m_LastAsyncResult);
                }
            }
            catch (Exception)
            {
                HttpWebRequest currentRequest = this.m_CurrentRequest;
                if (currentRequest != null)
                {
                    currentRequest.ErrorStatusCodeNotify(this, false, true);
                }
                ConnectionReturnResult returnResult = null;
                this.HandleErrorWithReadDone(WebExceptionStatus.ReceiveFailure, ref returnResult);
                ConnectionReturnResult.SetResponses(returnResult);
            }
        }

        private static void PostReceiveWrapper(object state)
        {
            (state as Connection).PostReceive();
        }

        private void PrepareCloseConnectionSocket(ref ConnectionReturnResult returnResult)
        {
            this.m_IdleSinceUtc = DateTime.MinValue;
            base.CanBePooled = false;
            if ((this.m_WriteList.Count != 0) || (this.m_WaitList.Count != 0))
            {
                HttpWebRequest lockedRequest = this.LockedRequest;
                if (lockedRequest != null)
                {
                    bool flag = false;
                    foreach (HttpWebRequest request2 in this.m_WriteList)
                    {
                        if (request2 == lockedRequest)
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        foreach (WaitListItem item in this.m_WaitList)
                        {
                            if (item.Request == lockedRequest)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (flag)
                    {
                        this.UnlockRequest();
                    }
                }
                HttpWebRequest[] requests = null;
                if (this.m_WaitList.Count != 0)
                {
                    requests = new HttpWebRequest[this.m_WaitList.Count];
                    for (int i = 0; i < this.m_WaitList.Count; i++)
                    {
                        requests[i] = this.m_WaitList[i].Request;
                    }
                    ConnectionReturnResult.AddExceptionRange(ref returnResult, requests, ExceptionHelper.IsolatedException);
                }
                if (this.m_WriteList.Count != 0)
                {
                    Exception innerException = this.m_InnerException;
                    if (!(innerException is WebException) && !(innerException is SecurityException))
                    {
                        if (this.m_Error == WebExceptionStatus.ServerProtocolViolation)
                        {
                            string webStatusString = NetRes.GetWebStatusString(this.m_Error);
                            string str2 = "";
                            if (this.m_ParseError.Section != WebParseErrorSection.Generic)
                            {
                                str2 = str2 + " Section=" + this.m_ParseError.Section.ToString();
                            }
                            if (this.m_ParseError.Code != WebParseErrorCode.Generic)
                            {
                                str2 = str2 + " Detail=" + SR.GetString("net_WebResponseParseError_" + this.m_ParseError.Code.ToString());
                            }
                            if (str2.Length != 0)
                            {
                                webStatusString = webStatusString + "." + str2;
                            }
                            innerException = new WebException(webStatusString, innerException, this.m_Error, null, WebExceptionInternalStatus.RequestFatal);
                        }
                        else if (this.m_Error == WebExceptionStatus.SecureChannelFailure)
                        {
                            innerException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.SecureChannelFailure), WebExceptionStatus.SecureChannelFailure);
                        }
                        else if (this.m_Error == WebExceptionStatus.Timeout)
                        {
                            innerException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
                        }
                        else if (this.m_Error == WebExceptionStatus.RequestCanceled)
                        {
                            innerException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled, WebExceptionInternalStatus.RequestFatal, innerException);
                        }
                        else if ((this.m_Error == WebExceptionStatus.MessageLengthLimitExceeded) || (this.m_Error == WebExceptionStatus.TrustFailure))
                        {
                            innerException = new WebException(NetRes.GetWebStatusString("net_connclosed", this.m_Error), this.m_Error, WebExceptionInternalStatus.RequestFatal, innerException);
                        }
                        else
                        {
                            if (this.m_Error == WebExceptionStatus.Success)
                            {
                                throw new InternalException();
                            }
                            bool flag2 = false;
                            bool flag3 = false;
                            if (this.m_WriteList.Count != 1)
                            {
                                flag2 = true;
                            }
                            else if (this.m_Error == WebExceptionStatus.KeepAliveFailure)
                            {
                                HttpWebRequest request3 = (HttpWebRequest) this.m_WriteList[0];
                                if (!request3.BodyStarted)
                                {
                                    flag3 = true;
                                }
                            }
                            else
                            {
                                flag2 = !this.AtLeastOneResponseReceived && !((HttpWebRequest) this.m_WriteList[0]).BodyStarted;
                            }
                            innerException = new WebException(NetRes.GetWebStatusString("net_connclosed", this.m_Error), this.m_Error, flag3 ? WebExceptionInternalStatus.Isolated : (flag2 ? WebExceptionInternalStatus.Recoverable : WebExceptionInternalStatus.RequestFatal), innerException);
                        }
                    }
                    WebException exception = new WebException(NetRes.GetWebStatusString("net_connclosed", WebExceptionStatus.PipelineFailure), WebExceptionStatus.PipelineFailure, WebExceptionInternalStatus.Recoverable, innerException);
                    requests = new HttpWebRequest[this.m_WriteList.Count];
                    this.m_WriteList.CopyTo(requests, 0);
                    ConnectionReturnResult.AddExceptionRange(ref returnResult, requests, exception, innerException);
                }
                this.m_WriteList.Clear();
                foreach (WaitListItem item2 in this.m_WaitList)
                {
                    NetworkingPerfCounters.Instance.IncrementAverage(NetworkingPerfCounterName.HttpWebRequestAvgQueueTime, item2.QueueStartTime);
                }
                this.m_WaitList.Clear();
            }
            this.CheckIdle();
            if (this.m_Idle)
            {
                GC.SuppressFinalize(this);
            }
            if (!this.m_RemovedFromConnectionList && (this.ConnectionGroup != null))
            {
                this.m_RemovedFromConnectionList = true;
                this.ConnectionGroup.Disassociate(this);
            }
        }

        private long ProcessHeaderData(ref bool fHaveChunked, HttpWebRequest request, out bool dummyResponseStream)
        {
            long result = -1L;
            fHaveChunked = false;
            string str = this.m_ResponseData.m_ResponseHeaders["Transfer-Encoding"];
            if (str != null)
            {
                str = str.ToLower(CultureInfo.InvariantCulture);
                fHaveChunked = str.IndexOf("chunked") != -1;
            }
            if (!fHaveChunked)
            {
                string contentLength = this.m_ResponseData.m_ResponseHeaders.ContentLength;
                if (contentLength != null)
                {
                    int index = contentLength.IndexOf(':');
                    if (index != -1)
                    {
                        contentLength = contentLength.Substring(index + 1);
                    }
                    if (!long.TryParse(contentLength, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out result))
                    {
                        result = -1L;
                        index = contentLength.LastIndexOf(',');
                        if ((index != -1) && !long.TryParse(contentLength.Substring(index + 1), NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out result))
                        {
                            result = -1L;
                        }
                    }
                    if (result < 0L)
                    {
                        result = -2L;
                    }
                }
            }
            dummyResponseStream = ((!request.CanGetResponseStream || (this.m_ResponseData.m_StatusCode < HttpStatusCode.OK)) || (this.m_ResponseData.m_StatusCode == HttpStatusCode.NoContent)) || ((this.m_ResponseData.m_StatusCode == HttpStatusCode.NotModified) && (result < 0L));
            if (this.m_KeepAlive)
            {
                bool flag2 = false;
                if ((!dummyResponseStream && (result < 0L)) && !fHaveChunked)
                {
                    flag2 = true;
                }
                else if ((this.m_ResponseData.m_StatusCode == HttpStatusCode.Forbidden) && (base.NetworkStream is TlsStream))
                {
                    flag2 = true;
                }
                else if (((this.m_ResponseData.m_StatusCode > ((HttpStatusCode) 0x12b)) && ((request.CurrentMethod == KnownHttpVerb.Post) || (request.CurrentMethod == KnownHttpVerb.Put))) && (((this.m_MaximumUnauthorizedUploadLength >= 0L) && (request.ContentLength > this.m_MaximumUnauthorizedUploadLength)) && ((request.CurrentAuthenticationState == null) || (request.CurrentAuthenticationState.Module == null))))
                {
                    flag2 = true;
                }
                else
                {
                    bool flag3 = false;
                    bool flag4 = false;
                    string str3 = this.m_ResponseData.m_ResponseHeaders["Connection"];
                    if ((str3 == null) && (this.ServicePoint.InternalProxyServicePoint || request.IsTunnelRequest))
                    {
                        str3 = this.m_ResponseData.m_ResponseHeaders["Proxy-Connection"];
                    }
                    if (str3 != null)
                    {
                        str3 = str3.ToLower(CultureInfo.InvariantCulture);
                        if (str3.IndexOf("keep-alive") != -1)
                        {
                            flag4 = true;
                        }
                        else if (str3.IndexOf("close") != -1)
                        {
                            flag3 = true;
                        }
                    }
                    if ((flag3 && (this.ServicePoint.HttpBehaviour == HttpBehaviour.HTTP11)) || (!flag4 && (this.ServicePoint.HttpBehaviour <= HttpBehaviour.HTTP10)))
                    {
                        flag2 = true;
                    }
                }
                if (flag2)
                {
                    lock (this)
                    {
                        this.m_KeepAlive = false;
                        this.m_Free = false;
                    }
                }
            }
            return result;
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            int bytesRead = -1;
            WebExceptionStatus receiveFailure = WebExceptionStatus.ReceiveFailure;
            try
            {
                bytesRead = this.EndRead(asyncResult);
                if (bytesRead == 0)
                {
                    bytesRead = -1;
                }
                receiveFailure = WebExceptionStatus.Success;
            }
            catch (Exception exception)
            {
                HttpWebRequest currentRequest = this.m_CurrentRequest;
                if (currentRequest != null)
                {
                    currentRequest.ErrorStatusCodeNotify(this, false, true);
                }
                if (this.m_InnerException == null)
                {
                    this.m_InnerException = exception;
                }
                if (exception.GetType() == typeof(ObjectDisposedException))
                {
                    receiveFailure = WebExceptionStatus.RequestCanceled;
                }
                if (base.NetworkStream is TlsStream)
                {
                    receiveFailure = ((TlsStream) base.NetworkStream).ExceptionStatus;
                }
                else
                {
                    receiveFailure = WebExceptionStatus.ReceiveFailure;
                }
            }
            this.ReadComplete(bytesRead, receiveFailure);
        }

        private static void ReadCallbackWrapper(IAsyncResult asyncResult)
        {
            if (!asyncResult.CompletedSynchronously)
            {
                ((Connection) asyncResult.AsyncState).ReadCallback(asyncResult);
            }
        }

        private bool ReadComplete(int bytesRead, WebExceptionStatus errorStatus)
        {
            bool requestDone = true;
            CoreResponseData continueResponseData = null;
            ConnectionReturnResult returnResult = null;
            HttpWebRequest request = null;
            try
            {
                if (bytesRead < 0)
                {
                    if ((this.m_ReadState == ReadState.Start) && this.m_AtLeastOneResponseReceived)
                    {
                        if ((errorStatus == WebExceptionStatus.Success) || (errorStatus == WebExceptionStatus.ReceiveFailure))
                        {
                            errorStatus = WebExceptionStatus.KeepAliveFailure;
                        }
                    }
                    else if (errorStatus == WebExceptionStatus.Success)
                    {
                        errorStatus = WebExceptionStatus.ConnectionClosed;
                    }
                    HttpWebRequest currentRequest = this.m_CurrentRequest;
                    if (currentRequest != null)
                    {
                        currentRequest.ErrorStatusCodeNotify(this, false, true);
                    }
                    this.HandleErrorWithReadDone(errorStatus, ref returnResult);
                }
                else
                {
                    bytesRead += this.m_BytesRead;
                    if (bytesRead > this.m_ReadBuffer.Length)
                    {
                        throw new InternalException();
                    }
                    this.m_BytesRead = bytesRead;
                    DataParseStatus status = this.ParseResponseData(ref returnResult, out requestDone, out continueResponseData);
                    request = this.m_CurrentRequest;
                    if (status != DataParseStatus.NeedMoreData)
                    {
                        this.m_CurrentRequest = null;
                    }
                    if ((status == DataParseStatus.Invalid) || (status == DataParseStatus.DataTooBig))
                    {
                        if (request != null)
                        {
                            request.ErrorStatusCodeNotify(this, false, false);
                        }
                        if (status == DataParseStatus.Invalid)
                        {
                            this.HandleErrorWithReadDone(WebExceptionStatus.ServerProtocolViolation, ref returnResult);
                        }
                        else
                        {
                            this.HandleErrorWithReadDone(WebExceptionStatus.MessageLengthLimitExceeded, ref returnResult);
                        }
                    }
                    else if ((status != DataParseStatus.Done) && (status == DataParseStatus.NeedMoreData))
                    {
                        int count = this.m_BytesRead - this.m_BytesScanned;
                        if (count != 0)
                        {
                            if ((this.m_BytesScanned == 0) && (this.m_BytesRead == this.m_ReadBuffer.Length))
                            {
                                byte[] dst = new byte[this.m_ReadBuffer.Length * 2];
                                Buffer.BlockCopy(this.m_ReadBuffer, 0, dst, 0, this.m_BytesRead);
                                this.m_ReadBuffer = dst;
                            }
                            else
                            {
                                Buffer.BlockCopy(this.m_ReadBuffer, this.m_BytesScanned, this.m_ReadBuffer, 0, count);
                            }
                        }
                        this.m_BytesRead = count;
                        this.m_BytesScanned = 0;
                        if ((request != null) && request.Async)
                        {
                            if (Thread.CurrentThread.IsThreadPoolThread)
                            {
                                this.PostReceive();
                            }
                            else
                            {
                                ThreadPool.UnsafeQueueUserWorkItem(m_PostReceiveDelegate, this);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception))
                {
                    throw;
                }
                requestDone = true;
                if (this.m_InnerException == null)
                {
                    this.m_InnerException = exception;
                }
                HttpWebRequest request3 = this.m_CurrentRequest;
                if (request3 != null)
                {
                    request3.ErrorStatusCodeNotify(this, false, true);
                }
                this.HandleErrorWithReadDone(WebExceptionStatus.ReceiveFailure, ref returnResult);
            }
            try
            {
                if ((continueResponseData == null) && ((returnResult == null) || !returnResult.IsNotEmpty))
                {
                    return requestDone;
                }
                if (request != null)
                {
                    request.SetRequestContinue(continueResponseData);
                }
            }
            finally
            {
                ConnectionReturnResult.SetResponses(returnResult);
            }
            return requestDone;
        }

        internal void ReadStartNextRequest(WebRequest currentRequest, ref ConnectionReturnResult returnResult)
        {
            HttpWebRequest request = null;
            TriState unspecified = TriState.Unspecified;
            bool flag = false;
            bool flag2 = false;
            Interlocked.Decrement(ref this.m_ReservedCount);
            try
            {
                lock (this)
                {
                    if ((this.m_WriteList.Count > 0) && (currentRequest == this.m_WriteList[0]))
                    {
                        this.m_ReadState = ReadState.Start;
                        this.m_WriteList.RemoveAt(0);
                        this.m_ResponseData.m_ConnectStream = null;
                    }
                    else
                    {
                        flag2 = true;
                    }
                    if (!flag2)
                    {
                        if (this.m_ReadDone)
                        {
                            throw new InternalException();
                        }
                        if ((!this.m_KeepAlive || (this.m_Error != WebExceptionStatus.Success)) || !base.CanBePooled)
                        {
                            this.m_ReadDone = true;
                            if (this.m_WriteDone)
                            {
                                if (this.m_Error == WebExceptionStatus.Success)
                                {
                                    this.m_Error = WebExceptionStatus.KeepAliveFailure;
                                }
                                this.PrepareCloseConnectionSocket(ref returnResult);
                                flag = true;
                                this.Close();
                            }
                        }
                        else
                        {
                            this.m_AtLeastOneResponseReceived = true;
                            if (this.m_WriteList.Count != 0)
                            {
                                request = this.m_WriteList[0] as HttpWebRequest;
                                if (!request.HeadersCompleted)
                                {
                                    request = null;
                                    this.m_ReadDone = true;
                                }
                            }
                            else
                            {
                                this.m_ReadDone = true;
                                if (this.m_WriteDone)
                                {
                                    request = this.CheckNextRequest();
                                    if (request != null)
                                    {
                                        if (request.HeadersCompleted)
                                        {
                                            throw new InternalException();
                                        }
                                        unspecified = this.StartRequest(request, false);
                                    }
                                    else
                                    {
                                        this.m_Free = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                this.CheckIdle();
                if (returnResult != null)
                {
                    ConnectionReturnResult.SetResponses(returnResult);
                }
            }
            if (!flag2 && !flag)
            {
                if (unspecified != TriState.Unspecified)
                {
                    this.CompleteStartRequest(false, request, unspecified);
                }
                else if (request != null)
                {
                    if (!request.Async)
                    {
                        request.ConnectionReaderAsyncResult.InvokeCallback();
                    }
                    else if (this.m_BytesScanned < this.m_BytesRead)
                    {
                        this.ReadComplete(0, WebExceptionStatus.Success);
                    }
                    else if (Thread.CurrentThread.IsThreadPoolThread)
                    {
                        this.PostReceive();
                    }
                    else
                    {
                        ThreadPool.UnsafeQueueUserWorkItem(m_PostReceiveDelegate, this);
                    }
                }
            }
        }

        private void SetStatusLineParsed()
        {
            this.m_ResponseData.m_StatusCode = (HttpStatusCode) this.m_StatusLineValues.StatusCode;
            this.m_ResponseData.m_StatusDescription = this.m_StatusLineValues.StatusDescription;
            this.m_ResponseData.m_IsVersionHttp11 = (this.m_StatusLineValues.MajorVersion >= 1) && (this.m_StatusLineValues.MinorVersion >= 1);
            if ((this.ServicePoint.HttpBehaviour == HttpBehaviour.Unknown) || ((this.ServicePoint.HttpBehaviour == HttpBehaviour.HTTP11) && !this.m_ResponseData.m_IsVersionHttp11))
            {
                this.ServicePoint.HttpBehaviour = this.m_ResponseData.m_IsVersionHttp11 ? HttpBehaviour.HTTP11 : HttpBehaviour.HTTP10;
            }
            this.m_CanPipeline = this.ServicePoint.SupportsPipelining;
        }

        private TriState StartRequest(HttpWebRequest request, bool canPollRead)
        {
            if (this.m_WriteList.Count == 0)
            {
                if (((this.ServicePoint.MaxIdleTime != -1) && (this.m_IdleSinceUtc != DateTime.MinValue)) && ((this.m_IdleSinceUtc + TimeSpan.FromMilliseconds((double) this.ServicePoint.MaxIdleTime)) < DateTime.UtcNow))
                {
                    return TriState.Unspecified;
                }
                if (canPollRead && base.PollRead())
                {
                    return TriState.Unspecified;
                }
            }
            TriState @false = TriState.False;
            this.m_IdleSinceUtc = DateTime.MinValue;
            if (!this.m_IsPipelinePaused)
            {
                this.m_IsPipelinePaused = this.m_WriteList.Count >= s_MaxPipelinedCount;
            }
            this.m_Pipelining = (this.m_CanPipeline && request.Pipelined) && !request.HasEntityBody;
            this.m_WriteDone = false;
            this.m_WriteList.Add(request);
            this.CheckNonIdle();
            if (base.IsInitalizing)
            {
                @false = TriState.True;
            }
            return @false;
        }

        internal bool SubmitRequest(HttpWebRequest request, bool forcedsubmit)
        {
            TriState unspecified = TriState.Unspecified;
            ConnectionReturnResult returnResult = null;
            bool flag = false;
            lock (this)
            {
                request.AbortDelegate = this.m_AbortDelegate;
                if (request.Aborted)
                {
                    return true;
                }
                if (!base.CanBePooled)
                {
                    return false;
                }
                if (!forcedsubmit && this.NonKeepAliveRequestPipelined)
                {
                    return false;
                }
                if (this.m_RecycleTimer.Duration != this.ServicePoint.ConnectionLeaseTimerQueue.Duration)
                {
                    this.m_RecycleTimer.Cancel();
                    this.m_RecycleTimer = this.ServicePoint.ConnectionLeaseTimerQueue.CreateTimer();
                }
                if (this.m_RecycleTimer.HasExpired)
                {
                    request.KeepAlive = false;
                }
                if ((this.LockedRequest != null) && (this.LockedRequest != request))
                {
                    return false;
                }
                if (!forcedsubmit && !this.m_NonKeepAliveRequestPipelined)
                {
                    this.m_NonKeepAliveRequestPipelined = !request.KeepAlive && !request.NtlmKeepAlive;
                }
                if (((this.m_Free && this.m_WriteDone) && !forcedsubmit) && ((this.m_WriteList.Count == 0) || (((request.Pipelined && !request.HasEntityBody) && (this.m_CanPipeline && this.m_Pipelining)) && !this.m_IsPipelinePaused)))
                {
                    this.m_Free = false;
                    unspecified = this.StartRequest(request, true);
                    if (unspecified == TriState.Unspecified)
                    {
                        flag = true;
                        this.PrepareCloseConnectionSocket(ref returnResult);
                        base.Close(0);
                    }
                }
                else
                {
                    this.m_WaitList.Add(new WaitListItem(request, NetworkingPerfCounters.GetTimestamp()));
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.HttpWebRequestQueued);
                    this.CheckNonIdle();
                }
            }
            if (flag)
            {
                ConnectionReturnResult.SetResponses(returnResult);
                return false;
            }
            if (Logging.On)
            {
                Logging.Associate(Logging.Web, this, request);
            }
            if (unspecified != TriState.Unspecified)
            {
                this.CompleteStartRequest(true, request, unspecified);
            }
            if (!request.Async)
            {
                object obj2 = request.ConnectionAsyncResult.InternalWaitForCompletion();
                ConnectStream submitStream = obj2 as ConnectStream;
                AsyncTriState state2 = null;
                if (submitStream == null)
                {
                    state2 = obj2 as AsyncTriState;
                }
                if ((unspecified == TriState.Unspecified) && (state2 != null))
                {
                    this.CompleteStartRequest(true, request, state2.Value);
                }
                else if (submitStream != null)
                {
                    request.SetRequestSubmitDone(submitStream);
                }
            }
            return true;
        }

        internal void SyncRead(HttpWebRequest request, bool userRetrievedStream, bool probeRead)
        {
            if (t_SyncReadNesting <= 0)
            {
                bool flag = !probeRead;
                try
                {
                    bool flag2;
                    t_SyncReadNesting++;
                    int num = probeRead ? request.RequestContinueCount : 0;
                    int bytesRead = -1;
                    WebExceptionStatus receiveFailure = WebExceptionStatus.ReceiveFailure;
                    if (this.m_BytesScanned < this.m_BytesRead)
                    {
                        flag = true;
                        bytesRead = 0;
                        receiveFailure = WebExceptionStatus.Success;
                    }
                    do
                    {
                        flag2 = true;
                        try
                        {
                            if (bytesRead != 0)
                            {
                                receiveFailure = WebExceptionStatus.ReceiveFailure;
                                if (!flag)
                                {
                                    flag = base.Poll(0x55730, SelectMode.SelectRead);
                                }
                                if (flag)
                                {
                                    this.ReadTimeout = request.Timeout;
                                    bytesRead = this.Read(this.m_ReadBuffer, this.m_BytesRead, this.m_ReadBuffer.Length - this.m_BytesRead);
                                    receiveFailure = WebExceptionStatus.Success;
                                    if (bytesRead == 0)
                                    {
                                        bytesRead = -1;
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            if (NclUtilities.IsFatal(exception))
                            {
                                throw;
                            }
                            if (this.m_InnerException == null)
                            {
                                this.m_InnerException = exception;
                            }
                            if (exception.GetType() == typeof(ObjectDisposedException))
                            {
                                receiveFailure = WebExceptionStatus.RequestCanceled;
                            }
                            else if (base.NetworkStream is TlsStream)
                            {
                                receiveFailure = ((TlsStream) base.NetworkStream).ExceptionStatus;
                            }
                            else
                            {
                                SocketException innerException = exception.InnerException as SocketException;
                                if (innerException != null)
                                {
                                    if (innerException.ErrorCode == 0x274c)
                                    {
                                        receiveFailure = WebExceptionStatus.Timeout;
                                    }
                                    else
                                    {
                                        receiveFailure = WebExceptionStatus.ReceiveFailure;
                                    }
                                }
                            }
                        }
                        if (flag)
                        {
                            flag2 = this.ReadComplete(bytesRead, receiveFailure);
                        }
                        bytesRead = -1;
                    }
                    while (!flag2 && (userRetrievedStream || (num == request.RequestContinueCount)));
                }
                finally
                {
                    t_SyncReadNesting--;
                }
                if (probeRead)
                {
                    if (flag)
                    {
                        if (!request.Saw100Continue && !userRetrievedStream)
                        {
                            request.SawInitialResponse = true;
                        }
                    }
                    else
                    {
                        request.SetRequestContinue();
                    }
                }
            }
        }

        private bool TunnelThroughProxy(Uri proxy, HttpWebRequest originalRequest, bool async)
        {
            bool flag = false;
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                new WebPermission(NetworkAccess.Connect, proxy).Assert();
                try
                {
                    request = new HttpWebRequest(proxy, originalRequest.Address, originalRequest);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                request.Credentials = (originalRequest.InternalProxy == null) ? null : originalRequest.InternalProxy.Credentials;
                request.InternalProxy = null;
                request.PreAuthenticate = true;
                if (async)
                {
                    TunnelStateObject state = new TunnelStateObject(originalRequest, this);
                    IAsyncResult asyncResult = request.BeginGetResponse(m_TunnelCallback, state);
                    if (!asyncResult.CompletedSynchronously)
                    {
                        return true;
                    }
                    response = (HttpWebResponse) request.EndGetResponse(asyncResult);
                }
                else
                {
                    response = (HttpWebResponse) request.GetResponse();
                }
                ConnectStream responseStream = (ConnectStream) response.GetResponseStream();
                base.NetworkStream = new NetworkStream(responseStream.Connection.NetworkStream, true);
                responseStream.Connection.NetworkStream.ConvertToNotSocketOwner();
                flag = true;
            }
            catch (Exception exception)
            {
                if (this.m_InnerException == null)
                {
                    this.m_InnerException = exception;
                }
            }
            return flag;
        }

        private static void TunnelThroughProxyWrapper(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                bool flag = false;
                WebExceptionStatus connectFailure = WebExceptionStatus.ConnectFailure;
                HttpWebRequest asyncObject = (HttpWebRequest) ((LazyAsyncResult) result).AsyncObject;
                Connection connection = ((TunnelStateObject) result.AsyncState).Connection;
                HttpWebRequest originalRequest = ((TunnelStateObject) result.AsyncState).OriginalRequest;
                try
                {
                    asyncObject.EndGetResponse(result);
                    HttpWebResponse response = (HttpWebResponse) asyncObject.GetResponse();
                    ConnectStream responseStream = (ConnectStream) response.GetResponseStream();
                    connection.NetworkStream = new NetworkStream(responseStream.Connection.NetworkStream, true);
                    responseStream.Connection.NetworkStream.ConvertToNotSocketOwner();
                    flag = true;
                }
                catch (Exception exception)
                {
                    if (connection.m_InnerException == null)
                    {
                        connection.m_InnerException = exception;
                    }
                    if (exception is WebException)
                    {
                        connectFailure = ((WebException) exception).Status;
                    }
                }
                if (!flag)
                {
                    ConnectionReturnResult returnResult = null;
                    connection.HandleError(false, false, connectFailure, ref returnResult);
                    ConnectionReturnResult.SetResponses(returnResult);
                }
                else
                {
                    connection.CompleteConnection(true, originalRequest);
                }
            }
        }

        private void UnlockRequest()
        {
            this.LockedRequest = null;
            if (this.ConnectionGroup != null)
            {
                this.ConnectionGroup.ConnectionGoneIdle();
            }
        }

        internal void Write(ScatterGatherBuffers writeBuffer)
        {
            BufferOffsetSize[] buffers = writeBuffer.GetBuffers();
            if (buffers != null)
            {
                base.MultipleWrite(buffers);
            }
        }

        internal void WriteStartNextRequest(HttpWebRequest request, ref ConnectionReturnResult returnResult)
        {
            TriState unspecified = TriState.Unspecified;
            HttpWebRequest nextRequest = null;
            bool calledCloseConnection = false;
            this.InternalWriteStartNextRequest(request, ref calledCloseConnection, ref unspecified, ref nextRequest, ref returnResult);
            if (!calledCloseConnection && (unspecified != TriState.Unspecified))
            {
                this.CompleteStartRequest(false, nextRequest, unspecified);
            }
        }

        internal bool AtLeastOneResponseReceived
        {
            get
            {
                return this.m_AtLeastOneResponseReceived;
            }
        }

        internal int BusyCount
        {
            get
            {
                return (((this.m_ReadDone ? 0 : 1) + (2 * (this.m_WaitList.Count + this.m_WriteList.Count))) + this.m_ReservedCount);
            }
        }

        private System.Net.ConnectionGroup ConnectionGroup
        {
            get
            {
                return this.m_ConnectionGroup;
            }
        }

        internal int IISVersion
        {
            get
            {
                return this.m_IISVersion;
            }
        }

        internal bool KeepAlive
        {
            get
            {
                return this.m_KeepAlive;
            }
        }

        internal HttpWebRequest LockedRequest
        {
            get
            {
                return this.m_LockedRequest;
            }
            set
            {
                HttpWebRequest lockedRequest = this.m_LockedRequest;
                if (value == lockedRequest)
                {
                    if ((value != null) && (value.UnlockConnectionDelegate != this.m_ConnectionUnlock))
                    {
                        throw new InternalException();
                    }
                }
                else
                {
                    object obj2 = (lockedRequest == null) ? null : lockedRequest.UnlockConnectionDelegate;
                    if ((obj2 != null) && ((value != null) || (this.m_ConnectionUnlock != obj2)))
                    {
                        throw new InternalException();
                    }
                    if (value == null)
                    {
                        this.m_LockedRequest = null;
                        lockedRequest.UnlockConnectionDelegate = null;
                    }
                    else
                    {
                        UnlockConnectionDelegate unlockConnectionDelegate = value.UnlockConnectionDelegate;
                        if (unlockConnectionDelegate != null)
                        {
                            if (unlockConnectionDelegate == this.m_ConnectionUnlock)
                            {
                                throw new InternalException();
                            }
                            unlockConnectionDelegate();
                        }
                        value.UnlockConnectionDelegate = this.m_ConnectionUnlock;
                        this.m_LockedRequest = value;
                    }
                }
            }
        }

        internal bool NonKeepAliveRequestPipelined
        {
            get
            {
                return this.m_NonKeepAliveRequestPipelined;
            }
        }

        internal override System.Net.ServicePoint ServicePoint
        {
            get
            {
                return this.ConnectionGroup.ServicePoint;
            }
        }

        private class AsyncTriState
        {
            public TriState Value;

            public AsyncTriState(TriState newValue)
            {
                this.Value = newValue;
            }
        }

        private class StatusLineValues
        {
            internal int MajorVersion;
            internal int MinorVersion;
            internal int StatusCode;
            internal string StatusDescription;
        }

        private class WaitListItem
        {
            private long queueStartTime;
            private HttpWebRequest request;

            public WaitListItem(HttpWebRequest request, long queueStartTime)
            {
                this.request = request;
                this.queueStartTime = queueStartTime;
            }

            public long QueueStartTime
            {
                get
                {
                    return this.queueStartTime;
                }
            }

            public HttpWebRequest Request
            {
                get
                {
                    return this.request;
                }
            }
        }
    }
}

