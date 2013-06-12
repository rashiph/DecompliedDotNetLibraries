namespace System.Net
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.Threading;

    internal class ConnectStream : Stream, ICloseEx, IRequestLifetimeTracker
    {
        private const int AlreadyAborted = 0xbde31;
        private const long c_MaxDrainBytes = 0x10000L;
        private ScatterGatherBuffers m_BufferedData;
        private bool m_BufferOnly;
        private int m_BytesAlreadyTransferred;
        private long m_BytesLeftToWrite;
        private int m_CallNesting;
        private bool m_Chunked;
        private bool m_ChunkedNeedCRLFRead;
        private bool m_ChunkEofRecvd;
        private int m_ChunkSize;
        private System.Net.Connection m_Connection;
        private int m_DoneCalled;
        private bool m_Draining;
        private Exception m_ErrorException;
        private bool m_ErrorResponseStatus;
        private HttpWriteMode m_HttpWriteMode;
        private bool m_IgnoreSocketErrors;
        private byte[] m_ReadBuffer;
        private int m_ReadBufferSize;
        private long m_ReadBytes;
        private static readonly AsyncCallback m_ReadCallbackDelegate = new AsyncCallback(ConnectStream.ReadCallback);
        private static readonly WaitCallback m_ReadChunkedCallbackDelegate = new WaitCallback(ConnectStream.ReadChunkedCallback);
        private int m_ReadOffset;
        private int m_ReadTimeout;
        private HttpWebRequest m_Request;
        private RequestLifetimeSetter m_RequestLifetimeSetter;
        private int m_ShutDown;
        private bool m_SuppressWrite;
        private byte[] m_TempBuffer;
        private static readonly AsyncCallback m_WriteCallbackDelegate = new AsyncCallback(ConnectStream.WriteCallback);
        private static readonly AsyncCallback m_WriteHeadersCallback = new AsyncCallback(ConnectStream.WriteHeadersCallback);
        private int m_WriteTimeout;
        internal static byte[] s_DrainingBuffer = new byte[0x1000];
        private static readonly object ZeroLengthRead = new object();

        public ConnectStream(System.Net.Connection connection, HttpWebRequest request)
        {
            this.m_Connection = connection;
            this.m_ReadTimeout = this.m_WriteTimeout = -1;
            this.m_Request = request;
            this.m_HttpWriteMode = request.HttpWriteMode;
            this.m_BytesLeftToWrite = (this.m_HttpWriteMode == HttpWriteMode.ContentLength) ? request.ContentLength : -1L;
            if (request.HttpWriteMode == HttpWriteMode.Buffer)
            {
                this.m_BufferOnly = true;
                this.EnableWriteBuffering();
            }
        }

        public ConnectStream(System.Net.Connection connection, byte[] buffer, int offset, int bufferCount, long readCount, bool chunked, HttpWebRequest request)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, this, "ConnectStream", SR.GetString("net_log_buffered_n_bytes", new object[] { readCount }));
            }
            this.m_ReadBuffer = buffer;
            this.m_ReadOffset = offset;
            this.m_ReadBufferSize = bufferCount;
            this.m_ReadBytes = readCount;
            this.m_ReadTimeout = this.m_WriteTimeout = -1;
            this.m_Chunked = chunked;
            this.m_Connection = connection;
            this.m_TempBuffer = new byte[2];
            this.m_Request = request;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "BeginRead", "");
            }
            if (this.WriteStream)
            {
                throw new NotSupportedException(SR.GetString("net_writeonlystream"));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (this.ErrorInStream)
            {
                throw this.m_ErrorException;
            }
            if (this.IsClosed)
            {
                throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
            }
            if (this.m_Request.Aborted)
            {
                throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
            }
            if (Interlocked.CompareExchange(ref this.m_CallNesting, 1, 0) != 0)
            {
                throw new NotSupportedException(SR.GetString("net_no_concurrent_io_allowed"));
            }
            IAsyncResult retObject = this.BeginReadWithoutValidation(buffer, offset, size, callback, state);
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "BeginRead", retObject);
            }
            return retObject;
        }

        private IAsyncResult BeginReadWithoutValidation(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            IAsyncResult result5;
            int num = 0;
            if (this.m_Chunked)
            {
                if (!this.m_ChunkEofRecvd)
                {
                    if (this.m_ChunkSize == 0)
                    {
                        NestedSingleAsyncResult result = new NestedSingleAsyncResult(this, state, callback, buffer, offset, size);
                        ThreadPool.QueueUserWorkItem(m_ReadChunkedCallbackDelegate, result);
                        return result;
                    }
                    num = Math.Min(size, this.m_ChunkSize);
                }
            }
            else if (this.m_ReadBytes != -1L)
            {
                num = (int) Math.Min(this.m_ReadBytes, (long) size);
            }
            else
            {
                num = size;
            }
            if ((num == 0) || this.Eof)
            {
                return new NestedSingleAsyncResult(this, state, callback, ZeroLengthRead);
            }
            try
            {
                int num2 = 0;
                if (this.m_ReadBufferSize > 0)
                {
                    num2 = this.FillFromBufferedData(buffer, ref offset, ref num);
                    if (num == 0)
                    {
                        return new NestedSingleAsyncResult(this, state, callback, num2);
                    }
                }
                if (this.ErrorInStream)
                {
                    throw this.m_ErrorException;
                }
                this.m_BytesAlreadyTransferred = num2;
                IAsyncResult result4 = this.m_Connection.BeginRead(buffer, offset, num, callback, state);
                if (result4 == null)
                {
                    this.m_BytesAlreadyTransferred = 0;
                    this.m_ErrorException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                    throw this.m_ErrorException;
                }
                result5 = result4;
            }
            catch (Exception exception)
            {
                this.IOError(exception);
                throw;
            }
            return result5;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "BeginWrite", "");
            }
            if (!this.WriteStream)
            {
                throw new NotSupportedException(SR.GetString("net_readonlystream"));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            IAsyncResult retObject = this.InternalWrite(true, buffer, offset, size, callback, state);
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "BeginWrite", retObject);
            }
            return retObject;
        }

        internal void CallDone()
        {
            this.CallDone(null);
        }

        private void CallDone(ConnectionReturnResult returnResult)
        {
            if (Interlocked.Increment(ref this.m_DoneCalled) == 1)
            {
                if (!this.WriteStream)
                {
                    if (returnResult == null)
                    {
                        this.m_Connection.ReadStartNextRequest(this.m_Request, ref returnResult);
                    }
                    else
                    {
                        ConnectionReturnResult.SetResponses(returnResult);
                    }
                }
                else
                {
                    this.m_Request.WriteCallDone(this, returnResult);
                }
            }
        }

        internal void CloseInternal(bool internalCall)
        {
            ((ICloseEx) this).CloseEx(internalCall ? CloseExState.Silent : CloseExState.Normal);
        }

        private void CloseInternal(bool internalCall, bool aborting)
        {
            bool flag = !aborting;
            Exception exception = null;
            if (aborting)
            {
                if (Interlocked.Exchange(ref this.m_ShutDown, 0xbde31) >= 0xbde31)
                {
                    return;
                }
            }
            else
            {
                if (Interlocked.Increment(ref this.m_ShutDown) > 1)
                {
                    return;
                }
                RequestLifetimeSetter.Report(this.m_RequestLifetimeSetter);
            }
            int num = (((this.IsPostStream && internalCall) && (!this.IgnoreSocketErrors && !this.BufferOnly)) && (flag && !NclUtilities.HasShutdownStarted)) ? 2 : 3;
            if (Interlocked.Exchange(ref this.m_CallNesting, num) == 1)
            {
                if (num == 2)
                {
                    return;
                }
                flag &= !NclUtilities.HasShutdownStarted;
            }
            if ((this.IgnoreSocketErrors && this.IsPostStream) && !internalCall)
            {
                this.m_BytesLeftToWrite = 0L;
            }
            if (!this.IgnoreSocketErrors && flag)
            {
                if (!this.WriteStream)
                {
                    flag = this.DrainSocket();
                }
                else
                {
                    try
                    {
                        if (!this.ErrorInStream)
                        {
                            if (!this.WriteChunked)
                            {
                                if (this.BytesLeftToWrite > 0L)
                                {
                                    throw new IOException(SR.GetString("net_io_notenoughbyteswritten"));
                                }
                                if (this.BufferOnly)
                                {
                                    this.m_BytesLeftToWrite = this.BufferedData.Length;
                                    this.m_Request.SwitchToContentLength();
                                    this.SafeSetSocketTimeout(SocketShutdown.Send);
                                    this.m_Request.NeedEndSubmitRequest();
                                    return;
                                }
                            }
                            else
                            {
                                try
                                {
                                    if (!this.m_IgnoreSocketErrors)
                                    {
                                        this.m_IgnoreSocketErrors = true;
                                        this.SafeSetSocketTimeout(SocketShutdown.Send);
                                        this.m_Connection.Write(NclConstants.ChunkTerminator, 0, NclConstants.ChunkTerminator.Length);
                                    }
                                }
                                catch
                                {
                                }
                                this.m_BytesLeftToWrite = 0L;
                            }
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                    catch (Exception exception2)
                    {
                        flag = false;
                        if (NclUtilities.IsFatal(exception2))
                        {
                            this.m_ErrorException = exception2;
                            throw;
                        }
                        exception = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), exception2, WebExceptionStatus.RequestCanceled, null);
                    }
                }
            }
            if (!flag && (this.m_DoneCalled == 0))
            {
                if (!aborting && (Interlocked.Exchange(ref this.m_ShutDown, 0xbde31) >= 0xbde31))
                {
                    return;
                }
                this.m_ErrorException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                this.m_Connection.AbortSocket(true);
                if (this.WriteStream)
                {
                    HttpWebRequest request = this.m_Request;
                    if (request != null)
                    {
                        request.Abort();
                    }
                }
                if (exception != null)
                {
                    this.CallDone();
                    if (!internalCall)
                    {
                        throw exception;
                    }
                }
            }
            this.CallDone();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (Logging.On)
                    {
                        Logging.Enter(Logging.Web, this, "Close", "");
                    }
                    ((ICloseEx) this).CloseEx(CloseExState.Normal);
                    if (Logging.On)
                    {
                        Logging.Exit(Logging.Web, this, "Close", "");
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private bool DrainSocket()
        {
            int num2;
            if (this.IgnoreSocketErrors)
            {
                return true;
            }
            long readBytes = this.m_ReadBytes;
            if (!this.m_Chunked)
            {
                if (this.m_ReadBufferSize != 0)
                {
                    this.m_ReadOffset += this.m_ReadBufferSize;
                    if (this.m_ReadBytes != -1L)
                    {
                        this.m_ReadBytes -= this.m_ReadBufferSize;
                        if (this.m_ReadBytes < 0L)
                        {
                            this.m_ReadBytes = 0L;
                            return false;
                        }
                    }
                    this.m_ReadBufferSize = 0;
                    this.m_ReadBuffer = null;
                }
                if (readBytes == -1L)
                {
                    return true;
                }
            }
            if (this.Eof)
            {
                return true;
            }
            if (this.m_ReadBytes > 0x10000L)
            {
                this.m_Connection.AbortSocket(false);
                return true;
            }
            this.m_Draining = true;
            try
            {
                do
                {
                    num2 = this.ReadWithoutValidation(s_DrainingBuffer, 0, s_DrainingBuffer.Length, false);
                }
                while (num2 > 0);
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception))
                {
                    throw;
                }
                num2 = -1;
            }
            return (num2 > 0);
        }

        internal void EnableWriteBuffering()
        {
            if (this.BufferedData == null)
            {
                if (this.WriteChunked)
                {
                    this.BufferedData = new ScatterGatherBuffers();
                }
                else
                {
                    this.BufferedData = new ScatterGatherBuffers(this.BytesLeftToWrite);
                }
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int num;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "EndRead", "");
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            bool zeroLengthRead = false;
            if (asyncResult.GetType() == typeof(NestedSingleAsyncResult))
            {
                NestedSingleAsyncResult result = (NestedSingleAsyncResult) asyncResult;
                if (result.AsyncObject != this)
                {
                    throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
                }
                if (result.EndCalled)
                {
                    throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndRead" }));
                }
                result.EndCalled = true;
                if (this.ErrorInStream)
                {
                    throw this.m_ErrorException;
                }
                object obj2 = result.InternalWaitForCompletion();
                Exception exception = obj2 as Exception;
                if (exception != null)
                {
                    this.IOError(exception, false);
                    num = -1;
                }
                else if (obj2 == null)
                {
                    num = 0;
                }
                else if (obj2 == ZeroLengthRead)
                {
                    num = 0;
                    zeroLengthRead = true;
                }
                else
                {
                    try
                    {
                        num = (int) obj2;
                    }
                    catch (InvalidCastException)
                    {
                        num = -1;
                    }
                }
            }
            else
            {
                try
                {
                    num = this.m_Connection.EndRead(asyncResult);
                }
                catch (Exception exception2)
                {
                    if (NclUtilities.IsFatal(exception2))
                    {
                        throw;
                    }
                    this.IOError(exception2, false);
                    num = -1;
                }
            }
            num = this.EndReadWithoutValidation(num, zeroLengthRead);
            Interlocked.CompareExchange(ref this.m_CallNesting, 0, 1);
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "EndRead", num);
            }
            if (this.m_ErrorException != null)
            {
                throw this.m_ErrorException;
            }
            return num;
        }

        private int EndReadWithoutValidation(int bytesTransferred, bool zeroLengthRead)
        {
            int bytesAlreadyTransferred = this.m_BytesAlreadyTransferred;
            this.m_BytesAlreadyTransferred = 0;
            if (this.m_Chunked)
            {
                if (bytesTransferred < 0)
                {
                    this.IOError(null, false);
                    bytesTransferred = 0;
                }
                if ((bytesTransferred == 0) && (this.m_ChunkSize > 0))
                {
                    WebException exception = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
                    this.IOError(exception, true);
                    throw exception;
                }
                bytesTransferred += bytesAlreadyTransferred;
                this.m_ChunkSize -= bytesTransferred;
                return bytesTransferred;
            }
            bool flag = false;
            if (bytesTransferred <= 0)
            {
                if ((this.m_ReadBytes != -1L) && ((bytesTransferred < 0) || !zeroLengthRead))
                {
                    this.IOError(null, false);
                }
                else
                {
                    flag = true;
                    bytesTransferred = 0;
                }
            }
            bytesTransferred += bytesAlreadyTransferred;
            if (this.m_ReadBytes != -1L)
            {
                this.m_ReadBytes -= bytesTransferred;
            }
            if ((this.m_ReadBytes == 0L) || flag)
            {
                this.m_ReadBytes = 0L;
                this.CallDone();
            }
            return bytesTransferred;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "EndWrite", "");
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            LazyAsyncResult result = asyncResult as LazyAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndWrite" }));
            }
            result.EndCalled = true;
            object obj2 = result.InternalWaitForCompletion();
            if (this.ErrorInStream)
            {
                throw this.m_ErrorException;
            }
            Exception exception = obj2 as Exception;
            if (exception != null)
            {
                if ((exception is IOException) && this.m_Request.Aborted)
                {
                    throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                }
                this.IOError(exception);
                throw exception;
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "EndWrite", "");
            }
        }

        internal void ErrorResponseNotify(bool isKeepAlive)
        {
            this.m_ErrorResponseStatus = true;
            this.m_IgnoreSocketErrors |= !isKeepAlive;
        }

        private int ExchangeCallNesting(int value, int comparand)
        {
            return Interlocked.CompareExchange(ref this.m_CallNesting, value, comparand);
        }

        internal void FatalResponseNotify()
        {
            if (this.m_ErrorException == null)
            {
                Interlocked.CompareExchange<Exception>(ref this.m_ErrorException, new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") })), null);
            }
            this.m_ErrorResponseStatus = false;
        }

        private int FillFromBufferedData(byte[] buffer, ref int offset, ref int size)
        {
            if (this.m_ReadBufferSize == 0)
            {
                return 0;
            }
            int count = Math.Min(size, this.m_ReadBufferSize);
            Buffer.BlockCopy(this.m_ReadBuffer, this.m_ReadOffset, buffer, offset, count);
            this.m_ReadOffset += count;
            this.m_ReadBufferSize -= count;
            if (this.m_ReadBufferSize == 0)
            {
                this.m_ReadBuffer = null;
            }
            size -= count;
            offset += count;
            return count;
        }

        public override void Flush()
        {
        }

        internal ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            ChannelBinding channelBinding = null;
            TlsStream networkStream = this.m_Connection.NetworkStream as TlsStream;
            if (networkStream != null)
            {
                channelBinding = networkStream.GetChannelBinding(kind);
            }
            return channelBinding;
        }

        internal static byte[] GetChunkHeader(int size, out int offset)
        {
            uint num = 0xf0000000;
            byte[] buffer = new byte[10];
            offset = -1;
            int index = 0;
            while (index < 8)
            {
                if ((offset != -1) || ((size & num) != 0L))
                {
                    uint num3 = (uint) (size >> 0x1c);
                    if (num3 < 10)
                    {
                        buffer[index] = (byte) (num3 + 0x30);
                    }
                    else
                    {
                        buffer[index] = (byte) ((num3 - 10) + 0x41);
                    }
                    if (offset == -1)
                    {
                        offset = index;
                    }
                }
                index++;
                size = size << 4;
            }
            buffer[8] = 13;
            buffer[9] = 10;
            return buffer;
        }

        private int InternalRead(byte[] buffer, int offset, int size)
        {
            int num = this.FillFromBufferedData(buffer, ref offset, ref size);
            if (num > 0)
            {
                return num;
            }
            if (this.ErrorInStream)
            {
                throw this.m_ErrorException;
            }
            return this.m_Connection.Read(buffer, offset, size);
        }

        private IAsyncResult InternalWrite(bool async, byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            if (this.ErrorInStream)
            {
                throw this.m_ErrorException;
            }
            if (this.IsClosed && !this.IgnoreSocketErrors)
            {
                throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
            }
            if (this.m_Request.Aborted && !this.IgnoreSocketErrors)
            {
                throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
            }
            int num = Interlocked.CompareExchange(ref this.m_CallNesting, 1, 0);
            if ((num != 0) && (num != 2))
            {
                throw new NotSupportedException(SR.GetString("net_no_concurrent_io_allowed"));
            }
            if (((this.BufferedData != null) && (size != 0)) && (((this.m_Request.ContentLength != 0L) || !this.IsPostStream) || !this.m_Request.NtlmKeepAlive))
            {
                this.BufferedData.Write(buffer, offset, size);
            }
            LazyAsyncResult result = null;
            bool flag = false;
            try
            {
                if (((size == 0) || this.BufferOnly) || (this.m_SuppressWrite || this.IgnoreSocketErrors))
                {
                    if ((this.m_SuppressWrite && (this.m_BytesLeftToWrite > 0L)) && (size > 0))
                    {
                        this.m_BytesLeftToWrite -= size;
                    }
                    if (async)
                    {
                        result = new LazyAsyncResult(this, state, callback);
                        flag = true;
                    }
                    return result;
                }
                if (this.WriteChunked)
                {
                    BufferOffsetSize[] sizeArray;
                    int num2 = 0;
                    byte[] chunkHeader = GetChunkHeader(size, out num2);
                    if (this.m_ErrorResponseStatus)
                    {
                        this.m_IgnoreSocketErrors = true;
                        sizeArray = new BufferOffsetSize[] { new BufferOffsetSize(NclConstants.ChunkTerminator, 0, NclConstants.ChunkTerminator.Length, false) };
                    }
                    else
                    {
                        sizeArray = new BufferOffsetSize[] { new BufferOffsetSize(chunkHeader, num2, chunkHeader.Length - num2, false), new BufferOffsetSize(buffer, offset, size, false), new BufferOffsetSize(NclConstants.CRLF, 0, NclConstants.CRLF.Length, false) };
                    }
                    result = async ? new NestedMultipleAsyncResult(this, state, callback, sizeArray) : null;
                    try
                    {
                        if (async)
                        {
                            this.m_Connection.BeginMultipleWrite(sizeArray, m_WriteCallbackDelegate, result);
                            return result;
                        }
                        this.SafeSetSocketTimeout(SocketShutdown.Send);
                        this.m_Connection.MultipleWrite(sizeArray);
                    }
                    catch (Exception exception)
                    {
                        if (this.IgnoreSocketErrors && !NclUtilities.IsFatal(exception))
                        {
                            if (async)
                            {
                                flag = true;
                            }
                            return result;
                        }
                        if (this.m_Request.Aborted && ((exception is IOException) || (exception is ObjectDisposedException)))
                        {
                            throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                        }
                        num = 3;
                        if (NclUtilities.IsFatal(exception))
                        {
                            this.m_ErrorResponseStatus = false;
                            this.IOError(exception);
                            throw;
                        }
                        if (this.m_ErrorResponseStatus)
                        {
                            this.m_IgnoreSocketErrors = true;
                            if (async)
                            {
                                flag = true;
                            }
                            return result;
                        }
                        this.IOError(exception);
                        throw;
                    }
                    return result;
                }
                result = async ? new NestedSingleAsyncResult(this, state, callback, buffer, offset, size) : null;
                if (this.BytesLeftToWrite != -1L)
                {
                    if (this.BytesLeftToWrite < size)
                    {
                        throw new ProtocolViolationException(SR.GetString("net_entitytoobig"));
                    }
                    if (!async)
                    {
                        this.m_BytesLeftToWrite -= size;
                    }
                }
                try
                {
                    if (async)
                    {
                        if ((this.m_Request.ContentLength == 0L) && this.IsPostStream)
                        {
                            this.m_BytesLeftToWrite -= size;
                            flag = true;
                        }
                        else
                        {
                            this.m_BytesAlreadyTransferred = size;
                            this.m_Connection.BeginWrite(buffer, offset, size, m_WriteCallbackDelegate, result);
                        }
                    }
                    else
                    {
                        this.SafeSetSocketTimeout(SocketShutdown.Send);
                        if (((this.m_Request.ContentLength != 0L) || !this.IsPostStream) || !this.m_Request.NtlmKeepAlive)
                        {
                            this.m_Connection.Write(buffer, offset, size);
                        }
                    }
                }
                catch (Exception exception2)
                {
                    if (this.IgnoreSocketErrors && !NclUtilities.IsFatal(exception2))
                    {
                        if (async)
                        {
                            flag = true;
                        }
                        return result;
                    }
                    if (this.m_Request.Aborted && ((exception2 is IOException) || (exception2 is ObjectDisposedException)))
                    {
                        throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                    }
                    num = 3;
                    if (NclUtilities.IsFatal(exception2))
                    {
                        this.m_ErrorResponseStatus = false;
                        this.IOError(exception2);
                        throw;
                    }
                    if (!this.m_ErrorResponseStatus)
                    {
                        this.IOError(exception2);
                        throw;
                    }
                    this.m_IgnoreSocketErrors = true;
                    if (async)
                    {
                        flag = true;
                    }
                }
                result2 = result;
            }
            finally
            {
                if ((!async || (num == 3)) || flag)
                {
                    if (Interlocked.CompareExchange(ref this.m_CallNesting, (num == 3) ? 3 : 0, 1) == 2)
                    {
                        this.ResumeInternalClose(result);
                    }
                    else if (flag && (result != null))
                    {
                        result.InvokeCallback();
                    }
                }
            }
            return result2;
        }

        private void IOError(Exception exception)
        {
            this.IOError(exception, true);
        }

        private void IOError(Exception exception, bool willThrow)
        {
            if (this.m_ErrorException == null)
            {
                if (exception == null)
                {
                    string str;
                    if (!this.WriteStream)
                    {
                        str = SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") });
                    }
                    else
                    {
                        str = SR.GetString("net_io_writefailure", new object[] { SR.GetString("net_io_connectionclosed") });
                    }
                    Interlocked.CompareExchange<Exception>(ref this.m_ErrorException, new IOException(str), null);
                }
                else
                {
                    willThrow &= Interlocked.CompareExchange<Exception>(ref this.m_ErrorException, exception, null) != null;
                }
            }
            this.m_ChunkEofRecvd = true;
            ConnectionReturnResult returnResult = null;
            if (this.WriteStream)
            {
                this.m_Connection.HandleConnectStreamException(true, false, WebExceptionStatus.SendFailure, ref returnResult, this.m_ErrorException);
            }
            else
            {
                this.m_Connection.HandleConnectStreamException(false, true, WebExceptionStatus.ReceiveFailure, ref returnResult, this.m_ErrorException);
            }
            this.CallDone(returnResult);
            if (willThrow)
            {
                throw this.m_ErrorException;
            }
        }

        internal void PollAndRead(bool userRetrievedStream)
        {
            this.m_Connection.PollAndRead(this.m_Request, userRetrievedStream);
        }

        private int ProcessReadChunkedSize(StreamChunkBytes ReadByteBuffer)
        {
            int num;
            if (ChunkParse.GetChunkSize(ReadByteBuffer, out num) <= 0)
            {
                throw new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            if (ChunkParse.SkipPastCRLF(ReadByteBuffer) <= 0)
            {
                throw new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            return num;
        }

        private void ProcessWriteCallback(IAsyncResult asyncResult, LazyAsyncResult userResult)
        {
            Exception exception = null;
            try
            {
                NestedSingleAsyncResult result = userResult as NestedSingleAsyncResult;
                if (result != null)
                {
                    try
                    {
                        this.m_Connection.EndWrite(asyncResult);
                        if (this.BytesLeftToWrite != -1L)
                        {
                            this.m_BytesLeftToWrite -= this.m_BytesAlreadyTransferred;
                            this.m_BytesAlreadyTransferred = 0;
                        }
                        if (Logging.On)
                        {
                            Logging.Dump(Logging.Web, this, "WriteCallback", result.Buffer, result.Offset, result.Size);
                        }
                    }
                    catch (Exception exception2)
                    {
                        exception = exception2;
                        if (NclUtilities.IsFatal(exception2))
                        {
                            this.m_ErrorResponseStatus = false;
                            this.IOError(exception2);
                            throw;
                        }
                        if (this.m_ErrorResponseStatus)
                        {
                            this.m_IgnoreSocketErrors = true;
                            exception = null;
                        }
                    }
                }
                else
                {
                    NestedMultipleAsyncResult result2 = (NestedMultipleAsyncResult) userResult;
                    try
                    {
                        this.m_Connection.EndMultipleWrite(asyncResult);
                        if (Logging.On)
                        {
                            foreach (BufferOffsetSize size in result2.Buffers)
                            {
                                Logging.Dump(Logging.Web, result2, "WriteCallback", size.Buffer, size.Offset, size.Size);
                            }
                        }
                    }
                    catch (Exception exception3)
                    {
                        exception = exception3;
                        if (NclUtilities.IsFatal(exception3))
                        {
                            this.m_ErrorResponseStatus = false;
                            this.IOError(exception3);
                            throw;
                        }
                        if (this.m_ErrorResponseStatus)
                        {
                            this.m_IgnoreSocketErrors = true;
                            exception = null;
                        }
                    }
                }
            }
            finally
            {
                if (2 == this.ExchangeCallNesting((exception == null) ? 0 : 3, 1))
                {
                    if ((exception != null) && (this.m_ErrorException == null))
                    {
                        Interlocked.CompareExchange<Exception>(ref this.m_ErrorException, exception, null);
                    }
                    this.ResumeInternalClose(userResult);
                }
                else
                {
                    userResult.InvokeCallback(exception);
                }
            }
        }

        internal void ProcessWriteCallDone(ConnectionReturnResult returnResult)
        {
            try
            {
                if (returnResult == null)
                {
                    this.m_Connection.WriteStartNextRequest(this.m_Request, ref returnResult);
                    if ((!this.m_Request.Async && (this.m_Request.ConnectionReaderAsyncResult.InternalWaitForCompletion() == null)) && !this.m_Request.SawInitialResponse)
                    {
                        this.m_Connection.SyncRead(this.m_Request, true, false);
                    }
                    this.m_Request.SawInitialResponse = false;
                }
                ConnectionReturnResult.SetResponses(returnResult);
            }
            finally
            {
                if (this.IsPostStream || this.m_Request.Async)
                {
                    this.m_Request.CheckWriteSideResponseProcessing();
                }
            }
        }

        public override int Read([In, Out] byte[] buffer, int offset, int size)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "Read", "");
            }
            if (this.WriteStream)
            {
                throw new NotSupportedException(SR.GetString("net_writeonlystream"));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (this.ErrorInStream)
            {
                throw this.m_ErrorException;
            }
            if (this.IsClosed)
            {
                throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
            }
            if (this.m_Request.Aborted)
            {
                throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
            }
            if (Interlocked.CompareExchange(ref this.m_CallNesting, 1, 0) != 0)
            {
                throw new NotSupportedException(SR.GetString("net_no_concurrent_io_allowed"));
            }
            int length = -1;
            try
            {
                this.SafeSetSocketTimeout(SocketShutdown.Receive);
            }
            catch (Exception exception)
            {
                this.IOError(exception);
                throw;
            }
            try
            {
                length = this.ReadWithoutValidation(buffer, offset, size);
            }
            catch (Exception exception2)
            {
                Win32Exception innerException = exception2.InnerException as Win32Exception;
                if ((innerException != null) && (innerException.NativeErrorCode == 0x274c))
                {
                    exception2 = new WebException(SR.GetString("net_timeout"), WebExceptionStatus.Timeout);
                }
                throw exception2;
            }
            Interlocked.CompareExchange(ref this.m_CallNesting, 0, 1);
            if (Logging.On && (length > 0))
            {
                Logging.Dump(Logging.Web, this, "Read", buffer, offset, length);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "Read", length);
            }
            return length;
        }

        private static void ReadCallback(IAsyncResult asyncResult)
        {
            NestedSingleAsyncResult asyncState = (NestedSingleAsyncResult) asyncResult.AsyncState;
            ConnectStream asyncObject = (ConnectStream) asyncState.AsyncObject;
            object result = null;
            try
            {
                int num = asyncObject.m_Connection.EndRead(asyncResult);
                if (Logging.On)
                {
                    Logging.Dump(Logging.Web, asyncObject, "ReadCallback", asyncState.Buffer, asyncState.Offset, Math.Min(asyncState.Size, num));
                }
                result = num;
            }
            catch (Exception exception)
            {
                result = exception;
            }
            asyncState.InvokeCallback(result);
        }

        private static void ReadChunkedCallback(object state)
        {
            NestedSingleAsyncResult castedAsyncResult = state as NestedSingleAsyncResult;
            ConnectStream asyncObject = castedAsyncResult.AsyncObject as ConnectStream;
            object result = null;
            try
            {
                result = ReadChunkedCallbackWorker(castedAsyncResult, asyncObject);
            }
            catch (Exception exception)
            {
                result = exception;
            }
            if (result != null)
            {
                castedAsyncResult.InvokeCallback(result);
            }
        }

        private static object ReadChunkedCallbackWorker(NestedSingleAsyncResult castedAsyncResult, ConnectStream thisConnectStream)
        {
            if (!thisConnectStream.m_Draining && thisConnectStream.IsClosed)
            {
                return new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
            }
            if (thisConnectStream.m_ErrorException != null)
            {
                return thisConnectStream.m_ErrorException;
            }
            if (thisConnectStream.m_ChunkedNeedCRLFRead)
            {
                thisConnectStream.ReadCRLF(thisConnectStream.m_TempBuffer);
                thisConnectStream.m_ChunkedNeedCRLFRead = false;
            }
            StreamChunkBytes readByteBuffer = new StreamChunkBytes(thisConnectStream);
            thisConnectStream.m_ChunkSize = thisConnectStream.ProcessReadChunkedSize(readByteBuffer);
            if (thisConnectStream.m_ChunkSize != 0)
            {
                thisConnectStream.m_ChunkedNeedCRLFRead = true;
                int size = Math.Min(castedAsyncResult.Size, thisConnectStream.m_ChunkSize);
                int num2 = 0;
                if (thisConnectStream.m_ReadBufferSize > 0)
                {
                    num2 = thisConnectStream.FillFromBufferedData(castedAsyncResult.Buffer, ref castedAsyncResult.Offset, ref size);
                    if (size == 0)
                    {
                        return num2;
                    }
                }
                if (thisConnectStream.ErrorInStream)
                {
                    return thisConnectStream.m_ErrorException;
                }
                thisConnectStream.m_BytesAlreadyTransferred = num2;
                if (thisConnectStream.m_Connection.BeginRead(castedAsyncResult.Buffer, castedAsyncResult.Offset, size, m_ReadCallbackDelegate, castedAsyncResult) == null)
                {
                    thisConnectStream.m_BytesAlreadyTransferred = 0;
                    thisConnectStream.m_ErrorException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                    return thisConnectStream.m_ErrorException;
                }
                return null;
            }
            thisConnectStream.ReadCRLF(thisConnectStream.m_TempBuffer);
            thisConnectStream.RemoveTrailers(readByteBuffer);
            thisConnectStream.m_ReadBytes = 0L;
            thisConnectStream.m_ChunkEofRecvd = true;
            thisConnectStream.CallDone();
            return 0;
        }

        private int ReadChunkedSync(byte[] buffer, int offset, int size)
        {
            if (!this.m_Draining && this.IsClosed)
            {
                Exception exception = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
                throw exception;
            }
            if (this.m_ErrorException != null)
            {
                throw this.m_ErrorException;
            }
            if (this.m_ChunkedNeedCRLFRead)
            {
                this.ReadCRLF(this.m_TempBuffer);
                this.m_ChunkedNeedCRLFRead = false;
            }
            StreamChunkBytes readByteBuffer = new StreamChunkBytes(this);
            this.m_ChunkSize = this.ProcessReadChunkedSize(readByteBuffer);
            if (this.m_ChunkSize != 0)
            {
                this.m_ChunkedNeedCRLFRead = true;
                return this.InternalRead(buffer, offset, Math.Min(size, this.m_ChunkSize));
            }
            this.ReadCRLF(this.m_TempBuffer);
            this.RemoveTrailers(readByteBuffer);
            this.m_ReadBytes = 0L;
            this.m_ChunkEofRecvd = true;
            this.CallDone();
            return 0;
        }

        private int ReadCRLF(byte[] buffer)
        {
            int offset = 0;
            int length = NclConstants.CRLF.Length;
            int num3 = this.FillFromBufferedData(buffer, ref offset, ref length);
            if ((num3 >= 0) && (num3 != NclConstants.CRLF.Length))
            {
                do
                {
                    int num4 = this.m_Connection.Read(buffer, offset, length);
                    if (num4 <= 0)
                    {
                        throw new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") }));
                    }
                    length -= num4;
                    offset += num4;
                }
                while (length > 0);
            }
            return num3;
        }

        internal int ReadSingleByte()
        {
            if (this.ErrorInStream)
            {
                return -1;
            }
            if (this.m_ReadBufferSize != 0)
            {
                this.m_ReadBufferSize--;
                return this.m_ReadBuffer[this.m_ReadOffset++];
            }
            if (this.m_Connection.Read(this.m_TempBuffer, 0, 1) <= 0)
            {
                return -1;
            }
            return this.m_TempBuffer[0];
        }

        private int ReadWithoutValidation(byte[] buffer, int offset, int size)
        {
            return this.ReadWithoutValidation(buffer, offset, size, true);
        }

        private int ReadWithoutValidation([In, Out] byte[] buffer, int offset, int size, bool abortOnError)
        {
            int num = 0;
            if (this.m_Chunked)
            {
                if (!this.m_ChunkEofRecvd)
                {
                    if (this.m_ChunkSize == 0)
                    {
                        try
                        {
                            num = this.ReadChunkedSync(buffer, offset, size);
                            this.m_ChunkSize -= num;
                        }
                        catch (Exception exception)
                        {
                            if (abortOnError)
                            {
                                this.IOError(exception);
                            }
                            throw;
                        }
                        return num;
                    }
                    num = Math.Min(size, this.m_ChunkSize);
                }
            }
            else if (this.m_ReadBytes != -1L)
            {
                num = (int) Math.Min(this.m_ReadBytes, (long) size);
            }
            else
            {
                num = size;
            }
            if ((num == 0) || this.Eof)
            {
                return 0;
            }
            try
            {
                num = this.InternalRead(buffer, offset, num);
            }
            catch (Exception exception2)
            {
                if (abortOnError)
                {
                    this.IOError(exception2);
                }
                throw;
            }
            int num2 = num;
            if (this.m_Chunked && (this.m_ChunkSize > 0))
            {
                if (num2 == 0)
                {
                    WebException exception3 = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
                    this.IOError(exception3, true);
                    throw exception3;
                }
                this.m_ChunkSize -= num2;
                return num2;
            }
            bool flag = false;
            if (num2 <= 0)
            {
                num2 = 0;
                if (this.m_ReadBytes != -1L)
                {
                    if (!abortOnError)
                    {
                        throw this.m_ErrorException;
                    }
                    this.IOError(null, false);
                }
                else
                {
                    flag = true;
                }
            }
            if (this.m_ReadBytes != -1L)
            {
                this.m_ReadBytes -= num2;
                if (this.m_ReadBytes < 0L)
                {
                    throw new InternalException();
                }
            }
            if ((this.m_ReadBytes == 0L) || flag)
            {
                this.m_ReadBytes = 0L;
                this.CallDone();
            }
            return num2;
        }

        private void RemoveTrailers(StreamChunkBytes ReadByteBuffer)
        {
            while ((this.m_TempBuffer[0] != 13) && (this.m_TempBuffer[1] != 10))
            {
                if (ChunkParse.SkipPastCRLF(ReadByteBuffer) <= 0)
                {
                    throw new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") }));
                }
                this.ReadCRLF(this.m_TempBuffer);
            }
        }

        internal void ResubmitWrite(ConnectStream oldStream, bool suppressWrite)
        {
            try
            {
                Interlocked.CompareExchange(ref this.m_CallNesting, 4, 0);
                ScatterGatherBuffers bufferedData = oldStream.BufferedData;
                this.SafeSetSocketTimeout(SocketShutdown.Send);
                if (!this.WriteChunked)
                {
                    if (!suppressWrite)
                    {
                        this.m_Connection.Write(bufferedData);
                    }
                }
                else
                {
                    this.m_HttpWriteMode = HttpWriteMode.ContentLength;
                    if (bufferedData.Length == 0)
                    {
                        this.m_Connection.Write(NclConstants.ChunkTerminator, 0, NclConstants.ChunkTerminator.Length);
                    }
                    else
                    {
                        int offset = 0;
                        byte[] chunkHeader = GetChunkHeader(bufferedData.Length, out offset);
                        BufferOffsetSize[] buffers = bufferedData.GetBuffers();
                        BufferOffsetSize[] sizeArray2 = new BufferOffsetSize[buffers.Length + 3];
                        sizeArray2[0] = new BufferOffsetSize(chunkHeader, offset, chunkHeader.Length - offset, false);
                        int num2 = 0;
                        foreach (BufferOffsetSize size in buffers)
                        {
                            sizeArray2[++num2] = size;
                        }
                        sizeArray2[++num2] = new BufferOffsetSize(NclConstants.CRLF, 0, NclConstants.CRLF.Length, false);
                        sizeArray2[++num2] = new BufferOffsetSize(NclConstants.ChunkTerminator, 0, NclConstants.ChunkTerminator.Length, false);
                        SplitWritesState state = new SplitWritesState(sizeArray2);
                        for (BufferOffsetSize[] sizeArray3 = state.GetNextBuffers(); sizeArray3 != null; sizeArray3 = state.GetNextBuffers())
                        {
                            this.m_Connection.MultipleWrite(sizeArray3);
                        }
                    }
                }
                if (Logging.On && (bufferedData.GetBuffers() != null))
                {
                    foreach (BufferOffsetSize size2 in bufferedData.GetBuffers())
                    {
                        if (size2 == null)
                        {
                            Logging.Dump(Logging.Web, this, "ResubmitWrite", null, 0, 0);
                        }
                        else
                        {
                            Logging.Dump(Logging.Web, this, "ResubmitWrite", size2.Buffer, size2.Offset, size2.Size);
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
                WebException exception2 = new WebException(NetRes.GetWebStatusString("net_connclosed", WebExceptionStatus.SendFailure), WebExceptionStatus.SendFailure, WebExceptionInternalStatus.RequestFatal, exception);
                this.IOError(exception2, false);
            }
            finally
            {
                Interlocked.CompareExchange(ref this.m_CallNesting, 0, 4);
            }
            this.m_BytesLeftToWrite = 0L;
            this.CallDone();
        }

        private void ResumeClose_Part2(LazyAsyncResult userResult)
        {
            try
            {
                try
                {
                    if (this.ErrorInStream)
                    {
                        this.m_Connection.AbortSocket(true);
                    }
                }
                finally
                {
                    this.CallDone();
                }
            }
            catch
            {
            }
            finally
            {
                if (userResult != null)
                {
                    userResult.InvokeCallback();
                }
            }
        }

        private void ResumeClose_Part2_Wrapper(IAsyncResult ar)
        {
            try
            {
                this.m_Connection.EndWrite(ar);
            }
            catch (Exception)
            {
            }
            this.ResumeClose_Part2((LazyAsyncResult) ar.AsyncState);
        }

        private void ResumeInternalClose(LazyAsyncResult userResult)
        {
            if ((this.WriteChunked && !this.ErrorInStream) && !this.m_IgnoreSocketErrors)
            {
                this.m_IgnoreSocketErrors = true;
                try
                {
                    if (userResult == null)
                    {
                        this.SafeSetSocketTimeout(SocketShutdown.Send);
                        this.m_Connection.Write(NclConstants.ChunkTerminator, 0, NclConstants.ChunkTerminator.Length);
                    }
                    else
                    {
                        this.m_Connection.BeginWrite(NclConstants.ChunkTerminator, 0, NclConstants.ChunkTerminator.Length, new AsyncCallback(this.ResumeClose_Part2_Wrapper), userResult);
                        return;
                    }
                }
                catch (Exception)
                {
                }
            }
            this.ResumeClose_Part2(userResult);
        }

        private void SafeSetSocketTimeout(SocketShutdown mode)
        {
            if (!this.Eof)
            {
                int readTimeout;
                if (mode == SocketShutdown.Receive)
                {
                    readTimeout = this.ReadTimeout;
                }
                else
                {
                    readTimeout = this.WriteTimeout;
                }
                System.Net.Connection connection = this.m_Connection;
                if (connection != null)
                {
                    NetworkStream networkStream = connection.NetworkStream;
                    if (networkStream != null)
                    {
                        networkStream.SetSocketTimeoutOption(mode, readTimeout, false);
                    }
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        internal void SwitchToContentLength()
        {
            this.m_HttpWriteMode = HttpWriteMode.ContentLength;
        }

        void ICloseEx.CloseEx(CloseExState closeState)
        {
            this.CloseInternal((closeState & CloseExState.Silent) != CloseExState.Normal, (closeState & CloseExState.Abort) != CloseExState.Normal);
            GC.SuppressFinalize(this);
        }

        void IRequestLifetimeTracker.TrackRequestLifetime(long requestStartTimestamp)
        {
            this.m_RequestLifetimeSetter = new RequestLifetimeSetter(requestStartTimestamp);
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "Write", "");
            }
            if (!this.WriteStream)
            {
                throw new NotSupportedException(SR.GetString("net_readonlystream"));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            this.InternalWrite(false, buffer, offset, size, null, null);
            if (Logging.On)
            {
                Logging.Dump(Logging.Web, this, "Write", buffer, offset, size);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "Write", "");
            }
        }

        private static void WriteCallback(IAsyncResult asyncResult)
        {
            LazyAsyncResult asyncState = (LazyAsyncResult) asyncResult.AsyncState;
            ((ConnectStream) asyncState.AsyncObject).ProcessWriteCallback(asyncResult, asyncState);
        }

        internal void WriteHeaders(bool async)
        {
            WebExceptionStatus sendFailure = WebExceptionStatus.SendFailure;
            if (!this.ErrorInStream)
            {
                byte[] writeBuffer = this.m_Request.WriteBuffer;
                try
                {
                    Interlocked.CompareExchange(ref this.m_CallNesting, 4, 0);
                    if (async)
                    {
                        WriteHeadersCallbackState state = new WriteHeadersCallbackState(this.m_Request, this);
                        IAsyncResult asyncResult = this.m_Connection.UnsafeBeginWrite(writeBuffer, 0, writeBuffer.Length, m_WriteHeadersCallback, state);
                        if (asyncResult.CompletedSynchronously)
                        {
                            this.m_Connection.EndWrite(asyncResult);
                            this.m_Connection.CheckStartReceive(this.m_Request);
                            sendFailure = WebExceptionStatus.Success;
                        }
                        else
                        {
                            sendFailure = WebExceptionStatus.Pending;
                        }
                    }
                    else
                    {
                        this.SafeSetSocketTimeout(SocketShutdown.Send);
                        this.m_Connection.Write(writeBuffer, 0, writeBuffer.Length);
                        this.m_Connection.CheckStartReceive(this.m_Request);
                        sendFailure = WebExceptionStatus.Success;
                    }
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_sending_headers", new object[] { this.m_Request.Headers.ToString(true) }));
                    }
                }
                catch (Exception exception)
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                    if ((exception is IOException) || (exception is ObjectDisposedException))
                    {
                        if (!this.m_Connection.AtLeastOneResponseReceived && !this.m_Request.BodyStarted)
                        {
                            exception = new WebException(NetRes.GetWebStatusString("net_connclosed", sendFailure), sendFailure, WebExceptionInternalStatus.Recoverable, exception);
                        }
                        else
                        {
                            exception = new WebException(NetRes.GetWebStatusString("net_connclosed", sendFailure), sendFailure, this.m_Connection.AtLeastOneResponseReceived ? WebExceptionInternalStatus.Isolated : WebExceptionInternalStatus.RequestFatal, exception);
                        }
                    }
                    this.IOError(exception, false);
                }
                finally
                {
                    if (sendFailure != WebExceptionStatus.Pending)
                    {
                        Interlocked.CompareExchange(ref this.m_CallNesting, 0, 4);
                    }
                }
            }
            if (sendFailure != WebExceptionStatus.Pending)
            {
                this.m_Request.WriteHeadersCallback(sendFailure, this, async);
            }
        }

        private static void WriteHeadersCallback(IAsyncResult ar)
        {
            if (!ar.CompletedSynchronously)
            {
                WriteHeadersCallbackState asyncState = (WriteHeadersCallbackState) ar.AsyncState;
                ConnectStream stream = asyncState.stream;
                HttpWebRequest request = asyncState.request;
                WebExceptionStatus sendFailure = WebExceptionStatus.SendFailure;
                byte[] writeBuffer = request.WriteBuffer;
                try
                {
                    stream.m_Connection.EndWrite(ar);
                    stream.m_Connection.CheckStartReceive(request);
                    if (stream.m_Connection.m_InnerException != null)
                    {
                        throw stream.m_Connection.m_InnerException;
                    }
                    sendFailure = WebExceptionStatus.Success;
                }
                catch (Exception exception)
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                    if ((exception is IOException) || (exception is ObjectDisposedException))
                    {
                        if (!stream.m_Connection.AtLeastOneResponseReceived && !request.BodyStarted)
                        {
                            exception = new WebException(NetRes.GetWebStatusString("net_connclosed", sendFailure), sendFailure, WebExceptionInternalStatus.Recoverable, exception);
                        }
                        else
                        {
                            exception = new WebException(NetRes.GetWebStatusString("net_connclosed", sendFailure), sendFailure, stream.m_Connection.AtLeastOneResponseReceived ? WebExceptionInternalStatus.Isolated : WebExceptionInternalStatus.RequestFatal, exception);
                        }
                    }
                    stream.IOError(exception, false);
                }
                stream.ExchangeCallNesting(0, 4);
                request.WriteHeadersCallback(sendFailure, stream, true);
            }
        }

        internal ScatterGatherBuffers BufferedData
        {
            get
            {
                return this.m_BufferedData;
            }
            set
            {
                this.m_BufferedData = value;
            }
        }

        internal bool BufferOnly
        {
            get
            {
                return this.m_BufferOnly;
            }
        }

        internal long BytesLeftToWrite
        {
            get
            {
                return this.m_BytesLeftToWrite;
            }
        }

        public override bool CanRead
        {
            get
            {
                return (!this.WriteStream && !this.IsClosed);
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return (this.WriteStream && !this.IsClosed);
            }
        }

        internal System.Net.Connection Connection
        {
            get
            {
                return this.m_Connection;
            }
        }

        private bool Eof
        {
            get
            {
                if (this.ErrorInStream)
                {
                    return true;
                }
                if (this.m_Chunked)
                {
                    return this.m_ChunkEofRecvd;
                }
                if (this.m_ReadBytes == 0L)
                {
                    return true;
                }
                if (this.m_ReadBytes != -1L)
                {
                    return false;
                }
                return ((this.m_DoneCalled > 0) && (this.m_ReadBufferSize <= 0));
            }
        }

        internal bool ErrorInStream
        {
            get
            {
                return (this.m_ErrorException != null);
            }
        }

        internal bool IgnoreSocketErrors
        {
            get
            {
                return this.m_IgnoreSocketErrors;
            }
        }

        internal bool IsClosed
        {
            get
            {
                return (this.m_ShutDown != 0);
            }
        }

        internal bool IsPostStream
        {
            get
            {
                return (this.m_HttpWriteMode != HttpWriteMode.None);
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException(SR.GetString("net_noseek"));
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException(SR.GetString("net_noseek"));
            }
            set
            {
                throw new NotSupportedException(SR.GetString("net_noseek"));
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return this.m_ReadTimeout;
            }
            set
            {
                if ((value <= 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_gt_zero"));
                }
                this.m_ReadTimeout = value;
            }
        }

        internal bool SuppressWrite
        {
            set
            {
                this.m_SuppressWrite = value;
            }
        }

        private bool WriteChunked
        {
            get
            {
                return (this.m_HttpWriteMode == HttpWriteMode.Chunked);
            }
        }

        private bool WriteStream
        {
            get
            {
                return (this.m_HttpWriteMode != HttpWriteMode.Unknown);
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return this.m_WriteTimeout;
            }
            set
            {
                if ((value <= 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_gt_zero"));
                }
                this.m_WriteTimeout = value;
            }
        }

        private static class Nesting
        {
            public const int Closed = 2;
            public const int Idle = 0;
            public const int InError = 3;
            public const int InternalIO = 4;
            public const int IoInProgress = 1;
        }
    }
}

