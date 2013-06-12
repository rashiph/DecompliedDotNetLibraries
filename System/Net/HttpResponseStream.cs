namespace System.Net
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal class HttpResponseStream : Stream
    {
        private bool m_Closed;
        private HttpListenerContext m_HttpContext;
        private long m_LeftToWrite = -9223372036854775808L;

        internal HttpResponseStream(HttpListenerContext httpContext)
        {
            this.m_HttpContext = httpContext;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            throw new InvalidOperationException(SR.GetString("net_writeonlystream"));
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override unsafe IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            uint num;
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
            UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS flags = this.ComputeLeftToWrite();
            if (this.m_Closed || ((size == 0) && (this.m_LeftToWrite != 0L)))
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.HttpListener, this, "BeginWrite", "");
                }
                HttpResponseStreamAsyncResult result = new HttpResponseStreamAsyncResult(this, state, callback);
                result.InvokeCallback(0);
                return result;
            }
            if ((this.m_LeftToWrite >= 0L) && (size > this.m_LeftToWrite))
            {
                throw new ProtocolViolationException(SR.GetString("net_entitytoobig"));
            }
            flags |= (this.m_LeftToWrite == size) ? UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE : UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_SEND_RESPONSE_FLAG_MORE_DATA;
            bool sentHeaders = this.m_HttpContext.Response.SentHeaders;
            HttpResponseStreamAsyncResult asyncResult = new HttpResponseStreamAsyncResult(this, state, callback, buffer, offset, size, this.m_HttpContext.Response.BoundaryType == BoundaryType.Chunked, sentHeaders);
            try
            {
                if (!sentHeaders)
                {
                    num = this.m_HttpContext.Response.SendHeaders(null, asyncResult, flags);
                }
                else
                {
                    this.m_HttpContext.EnsureBoundHandle();
                    num = UnsafeNclNativeMethods.HttpApi.HttpSendResponseEntityBody(this.m_HttpContext.RequestQueueHandle, this.m_HttpContext.RequestId, (uint) flags, asyncResult.dataChunkCount, asyncResult.pDataChunks, null, SafeLocalFree.Zero, 0, asyncResult.m_pOverlapped, null);
                }
            }
            catch (Exception exception)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.HttpListener, this, "BeginWrite", exception);
                }
                asyncResult.InternalCleanup();
                this.m_HttpContext.Abort();
                throw;
            }
            switch (num)
            {
                case 0:
                case 0x3e5:
                    break;

                default:
                    asyncResult.InternalCleanup();
                    if (!this.m_HttpContext.Listener.IgnoreWriteExceptions || !sentHeaders)
                    {
                        Exception e = new HttpListenerException((int) num);
                        if (Logging.On)
                        {
                            Logging.Exception(Logging.HttpListener, this, "BeginWrite", e);
                        }
                        this.m_HttpContext.Abort();
                        throw e;
                    }
                    break;
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.HttpListener, this, "BeginWrite", "");
            }
            return asyncResult;
        }

        internal UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS ComputeLeftToWrite()
        {
            UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS nONE = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE;
            if (!this.m_HttpContext.Response.ComputedHeaders)
            {
                nONE = this.m_HttpContext.Response.ComputeHeaders();
            }
            if (this.m_LeftToWrite == -9223372036854775808L)
            {
                UnsafeNclNativeMethods.HttpApi.HTTP_VERB knownMethod = this.m_HttpContext.GetKnownMethod();
                this.m_LeftToWrite = (knownMethod != UnsafeNclNativeMethods.HttpApi.HTTP_VERB.HttpVerbHEAD) ? this.m_HttpContext.Response.ContentLength64 : 0L;
            }
            return nONE;
        }

        protected override unsafe void Dispose(bool disposing)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.HttpListener, this, "Close", "");
            }
            try
            {
                if (!disposing)
                {
                    goto Label_023F;
                }
                if (this.m_Closed)
                {
                    if (Logging.On)
                    {
                        Logging.Exit(Logging.HttpListener, this, "Close", "");
                    }
                    return;
                }
                this.m_Closed = true;
                UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS flags = this.ComputeLeftToWrite();
                if (this.m_LeftToWrite > 0L)
                {
                    throw new InvalidOperationException(SR.GetString("net_io_notenoughbyteswritten"));
                }
                bool sentHeaders = this.m_HttpContext.Response.SentHeaders;
                if (sentHeaders && (this.m_LeftToWrite == 0L))
                {
                    if (Logging.On)
                    {
                        Logging.Exit(Logging.HttpListener, this, "Close", "");
                    }
                    return;
                }
                uint num = 0;
                if (((this.m_HttpContext.Response.BoundaryType == BoundaryType.Chunked) || (this.m_HttpContext.Response.BoundaryType == BoundaryType.None)) && (string.Compare(this.m_HttpContext.Request.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase) != 0))
                {
                    if (this.m_HttpContext.Response.BoundaryType == BoundaryType.None)
                    {
                        flags |= UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_INITIALIZE_SERVER;
                    }
                    try
                    {
                        byte[] buffer;
                        if (((buffer = NclConstants.ChunkTerminator) == null) || (buffer.Length == 0))
                        {
                            ptrRef = null;
                            goto Label_0132;
                        }
                        fixed (IntPtr* ptrRef = buffer)
                        {
                            UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK* http_data_chunkPtr;
                        Label_0132:
                            http_data_chunkPtr = null;
                            if (this.m_HttpContext.Response.BoundaryType == BoundaryType.Chunked)
                            {
                                UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK http_data_chunk = new UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK {
                                    DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory,
                                    pBuffer = (byte*) ptrRef,
                                    BufferLength = (uint) NclConstants.ChunkTerminator.Length
                                };
                                http_data_chunkPtr = &http_data_chunk;
                            }
                            if (!sentHeaders)
                            {
                                num = this.m_HttpContext.Response.SendHeaders(http_data_chunkPtr, null, flags);
                            }
                            else
                            {
                                num = UnsafeNclNativeMethods.HttpApi.HttpSendResponseEntityBody(this.m_HttpContext.RequestQueueHandle, this.m_HttpContext.RequestId, (uint) flags, (http_data_chunkPtr != null) ? ((ushort) 1) : ((ushort) 0), http_data_chunkPtr, null, SafeLocalFree.Zero, 0, null, null);
                                if (this.m_HttpContext.Listener.IgnoreWriteExceptions)
                                {
                                    num = 0;
                                }
                            }
                            goto Label_01F6;
                        }
                    }
                    finally
                    {
                        ptrRef = null;
                    }
                }
                if (!sentHeaders)
                {
                    num = this.m_HttpContext.Response.SendHeaders(null, null, flags);
                }
            Label_01F6:
                switch (num)
                {
                    case 0:
                    case 0x26:
                        break;

                    default:
                    {
                        Exception e = new HttpListenerException((int) num);
                        if (Logging.On)
                        {
                            Logging.Exception(Logging.HttpListener, this, "Close", e);
                        }
                        this.m_HttpContext.Abort();
                        throw e;
                    }
                }
                this.m_LeftToWrite = 0L;
            }
            finally
            {
                base.Dispose(disposing);
            }
        Label_023F:
            if (Logging.On)
            {
                Logging.Exit(Logging.HttpListener, this, "Dispose", "");
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new InvalidOperationException(SR.GetString("net_writeonlystream"));
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.HttpListener, this, "EndWrite", "");
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            HttpResponseStreamAsyncResult result = asyncResult as HttpResponseStreamAsyncResult;
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
            Exception e = obj2 as Exception;
            if (e != null)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.HttpListener, this, "EndWrite", e);
                }
                this.m_HttpContext.Abort();
                throw e;
            }
            this.UpdateAfterWrite((uint) obj2);
            if (Logging.On)
            {
                Logging.Exit(Logging.HttpListener, this, "EndWrite", "");
            }
        }

        public override void Flush()
        {
        }

        public override int Read([In, Out] byte[] buffer, int offset, int size)
        {
            throw new InvalidOperationException(SR.GetString("net_writeonlystream"));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        private void UpdateAfterWrite(uint dataWritten)
        {
            if (this.m_LeftToWrite > 0L)
            {
                this.m_LeftToWrite -= dataWritten;
            }
            if (this.m_LeftToWrite == 0L)
            {
                this.m_Closed = true;
            }
        }

        public override unsafe void Write(byte[] buffer, int offset, int size)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.HttpListener, this, "Write", "");
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
            UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS flags = this.ComputeLeftToWrite();
            if (this.m_Closed || ((size == 0) && (this.m_LeftToWrite != 0L)))
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.HttpListener, this, "Write", "");
                }
            }
            else
            {
                uint num;
                if ((this.m_LeftToWrite >= 0L) && (size > this.m_LeftToWrite))
                {
                    throw new ProtocolViolationException(SR.GetString("net_entitytoobig"));
                }
                uint dataWritten = (uint) size;
                SafeLocalFree free = null;
                IntPtr zero = IntPtr.Zero;
                bool sentHeaders = this.m_HttpContext.Response.SentHeaders;
                try
                {
                    if (size == 0)
                    {
                        num = this.m_HttpContext.Response.SendHeaders(null, null, flags);
                    }
                    else
                    {
                        try
                        {
                            byte[] buffer2;
                            if (((buffer2 = buffer) == null) || (buffer2.Length == 0))
                            {
                                numRef = null;
                                goto Label_0109;
                            }
                            fixed (byte* numRef = buffer2)
                            {
                                byte* numPtr;
                            Label_0109:
                                numPtr = numRef;
                                if (this.m_HttpContext.Response.BoundaryType == BoundaryType.Chunked)
                                {
                                    string str = size.ToString("x", CultureInfo.InvariantCulture);
                                    dataWritten += (uint) (str.Length + 4);
                                    free = SafeLocalFree.LocalAlloc((int) dataWritten);
                                    zero = free.DangerousGetHandle();
                                    for (int i = 0; i < str.Length; i++)
                                    {
                                        Marshal.WriteByte(zero, i, (byte) str[i]);
                                    }
                                    Marshal.WriteInt16(zero, str.Length, (short) 0xa0d);
                                    Marshal.Copy(buffer, offset, IntPtrHelper.Add(zero, str.Length + 2), size);
                                    Marshal.WriteInt16(zero, ((int) dataWritten) - 2, (short) 0xa0d);
                                    numPtr = (byte*) zero;
                                    offset = 0;
                                }
                                UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK pDataChunk = new UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK {
                                    DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory,
                                    pBuffer = numPtr + offset,
                                    BufferLength = dataWritten
                                };
                                flags |= (this.m_LeftToWrite == size) ? UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE : UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_SEND_RESPONSE_FLAG_MORE_DATA;
                                if (!sentHeaders)
                                {
                                    num = this.m_HttpContext.Response.SendHeaders(&pDataChunk, null, flags);
                                }
                                else
                                {
                                    num = UnsafeNclNativeMethods.HttpApi.HttpSendResponseEntityBody(this.m_HttpContext.RequestQueueHandle, this.m_HttpContext.RequestId, (uint) flags, 1, &pDataChunk, null, SafeLocalFree.Zero, 0, null, null);
                                    if (this.m_HttpContext.Listener.IgnoreWriteExceptions)
                                    {
                                        num = 0;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            numRef = null;
                        }
                    }
                }
                finally
                {
                    if (free != null)
                    {
                        free.Close();
                    }
                }
                switch (num)
                {
                    case 0:
                    case 0x26:
                        this.UpdateAfterWrite(dataWritten);
                        if (Logging.On)
                        {
                            Logging.Dump(Logging.HttpListener, this, "Write", buffer, offset, (int) dataWritten);
                        }
                        if (Logging.On)
                        {
                            Logging.Exit(Logging.HttpListener, this, "Write", "");
                        }
                        return;
                }
                Exception e = new HttpListenerException((int) num);
                if (Logging.On)
                {
                    Logging.Exception(Logging.HttpListener, this, "Write", e);
                }
                this.m_HttpContext.Abort();
                throw e;
            }
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
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
    }
}

