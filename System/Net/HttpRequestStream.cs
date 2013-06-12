namespace System.Net
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    internal class HttpRequestStream : Stream
    {
        private bool m_Closed;
        private int m_DataChunkIndex;
        private uint m_DataChunkOffset;
        private HttpListenerContext m_HttpContext;
        private const int MaxReadSize = 0x20000;

        internal HttpRequestStream(HttpListenerContext httpContext)
        {
            this.m_HttpContext = httpContext;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override unsafe IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.HttpListener, this, "BeginRead", "");
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
            if ((size == 0) || this.m_Closed)
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.HttpListener, this, "BeginRead", "");
                }
                HttpRequestStreamAsyncResult result = new HttpRequestStreamAsyncResult(this, state, callback);
                result.InvokeCallback(0);
                return result;
            }
            HttpRequestStreamAsyncResult result2 = null;
            uint num = 0;
            if (this.m_DataChunkIndex != -1)
            {
                num = UnsafeNclNativeMethods.HttpApi.GetChunks(this.m_HttpContext.Request.RequestBuffer, this.m_HttpContext.Request.OriginalBlobAddress, ref this.m_DataChunkIndex, ref this.m_DataChunkOffset, buffer, offset, size);
                if ((this.m_DataChunkIndex != -1) && (num == size))
                {
                    result2 = new HttpRequestStreamAsyncResult(this, state, callback, buffer, offset, (uint) size, 0);
                    result2.InvokeCallback(num);
                }
            }
            if ((this.m_DataChunkIndex == -1) && (num < size))
            {
                uint num2 = 0;
                offset += (int) num;
                size -= (int) num;
                if (size > 0x20000)
                {
                    size = 0x20000;
                }
                result2 = new HttpRequestStreamAsyncResult(this, state, callback, buffer, offset, (uint) size, num);
                try
                {
                    byte[] buffer2 = buffer;
                    if (buffer2 != null)
                    {
                        int length = buffer2.Length;
                    }
                    this.m_HttpContext.EnsureBoundHandle();
                    num2 = UnsafeNclNativeMethods.HttpApi.HttpReceiveRequestEntityBody(this.m_HttpContext.RequestQueueHandle, this.m_HttpContext.RequestId, 1, result2.m_pPinnedBuffer, (uint) size, null, result2.m_pOverlapped);
                }
                catch (Exception exception)
                {
                    if (Logging.On)
                    {
                        Logging.Exception(Logging.HttpListener, this, "BeginRead", exception);
                    }
                    result2.InternalCleanup();
                    throw;
                }
                switch (num2)
                {
                    case 0:
                    case 0x3e5:
                        goto Label_0217;
                }
                if (num2 == 0x26)
                {
                    result2.m_pOverlapped.InternalLow = IntPtr.Zero;
                }
                result2.InternalCleanup();
                if (num2 != 0x26)
                {
                    Exception e = new HttpListenerException((int) num2);
                    if (Logging.On)
                    {
                        Logging.Exception(Logging.HttpListener, this, "BeginRead", e);
                    }
                    result2.InternalCleanup();
                    throw e;
                }
                result2 = new HttpRequestStreamAsyncResult(this, state, callback, num);
                result2.InvokeCallback(0);
            }
        Label_0217:
            if (Logging.On)
            {
                Logging.Exit(Logging.HttpListener, this, "BeginRead", "");
            }
            return result2;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            throw new InvalidOperationException(SR.GetString("net_readonlystream"));
        }

        protected override void Dispose(bool disposing)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.HttpListener, this, "Dispose", "");
            }
            try
            {
                this.m_Closed = true;
            }
            finally
            {
                base.Dispose(disposing);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.HttpListener, this, "Dispose", "");
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.HttpListener, this, "EndRead", "");
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            HttpRequestStreamAsyncResult result = asyncResult as HttpRequestStreamAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndRead" }));
            }
            result.EndCalled = true;
            object obj2 = result.InternalWaitForCompletion();
            Exception e = obj2 as Exception;
            if (e != null)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.HttpListener, this, "EndRead", e);
                }
                throw e;
            }
            uint dataRead = (uint) obj2;
            this.UpdateAfterRead((uint) result.ErrorCode, dataRead);
            if (Logging.On)
            {
                Logging.Exit(Logging.HttpListener, this, "EndRead", "");
            }
            return (int) (dataRead + result.m_dataAlreadyRead);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new InvalidOperationException(SR.GetString("net_readonlystream"));
        }

        public override void Flush()
        {
        }

        public override unsafe int Read([In, Out] byte[] buffer, int offset, int size)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.HttpListener, this, "Read", "");
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
            if ((size == 0) || this.m_Closed)
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.HttpListener, this, "Read", "dataRead:0");
                }
                return 0;
            }
            uint dataRead = 0;
            if (this.m_DataChunkIndex != -1)
            {
                dataRead = UnsafeNclNativeMethods.HttpApi.GetChunks(this.m_HttpContext.Request.RequestBuffer, this.m_HttpContext.Request.OriginalBlobAddress, ref this.m_DataChunkIndex, ref this.m_DataChunkOffset, buffer, offset, size);
            }
            if ((this.m_DataChunkIndex == -1) && (dataRead < size))
            {
                uint statusCode = 0;
                uint pBytesReturned = 0;
                offset += (int) dataRead;
                size -= (int) dataRead;
                if (size > 0x20000)
                {
                    size = 0x20000;
                }
                fixed (byte* numRef = buffer)
                {
                    statusCode = UnsafeNclNativeMethods.HttpApi.HttpReceiveRequestEntityBody(this.m_HttpContext.RequestQueueHandle, this.m_HttpContext.RequestId, 1, (void*) (numRef + offset), (uint) size, &pBytesReturned, null);
                    dataRead += pBytesReturned;
                }
                switch (statusCode)
                {
                    case 0:
                    case 0x26:
                        this.UpdateAfterRead(statusCode, dataRead);
                        break;

                    default:
                    {
                        Exception e = new HttpListenerException((int) statusCode);
                        if (Logging.On)
                        {
                            Logging.Exception(Logging.HttpListener, this, "Read", e);
                        }
                        throw e;
                    }
                }
            }
            if (Logging.On)
            {
                Logging.Dump(Logging.HttpListener, this, "Read", buffer, offset, (int) dataRead);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.HttpListener, this, "Read", "dataRead:" + dataRead);
            }
            return (int) dataRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        private void UpdateAfterRead(uint statusCode, uint dataRead)
        {
            if ((statusCode == 0x26) || (dataRead == 0))
            {
                this.Close();
            }
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            throw new InvalidOperationException(SR.GetString("net_readonlystream"));
        }

        public override bool CanRead
        {
            get
            {
                return true;
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
                return false;
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

        private class HttpRequestStreamAsyncResult : LazyAsyncResult
        {
            internal uint m_dataAlreadyRead;
            internal unsafe NativeOverlapped* m_pOverlapped;
            internal unsafe void* m_pPinnedBuffer;
            private static readonly IOCompletionCallback s_IOCallback = new IOCompletionCallback(HttpRequestStream.HttpRequestStreamAsyncResult.Callback);

            internal HttpRequestStreamAsyncResult(object asyncObject, object userState, AsyncCallback callback) : base(asyncObject, userState, callback)
            {
            }

            internal HttpRequestStreamAsyncResult(object asyncObject, object userState, AsyncCallback callback, uint dataAlreadyRead) : base(asyncObject, userState, callback)
            {
                this.m_dataAlreadyRead = dataAlreadyRead;
            }

            internal unsafe HttpRequestStreamAsyncResult(object asyncObject, object userState, AsyncCallback callback, byte[] buffer, int offset, uint size, uint dataAlreadyRead) : base(asyncObject, userState, callback)
            {
                this.m_dataAlreadyRead = dataAlreadyRead;
                this.m_pOverlapped = new Overlapped { AsyncResult = this }.Pack(s_IOCallback, buffer);
                this.m_pPinnedBuffer = (void*) Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
            }

            private static unsafe void Callback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
            {
                HttpRequestStream.HttpRequestStreamAsyncResult asyncResult = Overlapped.Unpack(nativeOverlapped).AsyncResult as HttpRequestStream.HttpRequestStreamAsyncResult;
                object result = null;
                try
                {
                    if ((errorCode != 0) && (errorCode != 0x26))
                    {
                        asyncResult.ErrorCode = (int) errorCode;
                        result = new HttpListenerException((int) errorCode);
                    }
                    else
                    {
                        result = numBytes;
                        if (Logging.On)
                        {
                            Logging.Dump(Logging.HttpListener, asyncResult, "Callback", (IntPtr) asyncResult.m_pPinnedBuffer, (int) numBytes);
                        }
                    }
                }
                catch (Exception exception)
                {
                    result = exception;
                }
                asyncResult.InvokeCallback(result);
            }

            protected override unsafe void Cleanup()
            {
                base.Cleanup();
                if (this.m_pOverlapped != null)
                {
                    Overlapped.Free(this.m_pOverlapped);
                }
            }
        }
    }
}

