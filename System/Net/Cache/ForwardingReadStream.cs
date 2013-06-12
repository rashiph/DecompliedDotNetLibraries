namespace System.Net.Cache
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;

    internal class ForwardingReadStream : BaseWrapperStream, ICloseEx
    {
        private int _Disposed;
        private long m_BytesToSkip;
        private AsyncCallback m_ReadCallback;
        private int m_ReadNesting;
        private bool m_SeenReadEOF;
        private Stream m_ShadowStream;
        private bool m_ShadowStreamIsDead;
        private bool m_ThrowOnWriteError;

        internal ForwardingReadStream(Stream originalStream, Stream shadowStream, long bytesToSkip, bool throwOnWriteError) : base(originalStream)
        {
            if (!shadowStream.CanWrite)
            {
                throw new ArgumentException(SR.GetString("net_cache_shadowstream_not_writable"), "shadowStream");
            }
            this.m_ShadowStream = shadowStream;
            this.m_BytesToSkip = bytesToSkip;
            this.m_ThrowOnWriteError = throwOnWriteError;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            IAsyncResult result4;
            if (Interlocked.Increment(ref this.m_ReadNesting) != 1)
            {
                throw new NotSupportedException(SR.GetString("net_io_invalidnestedcall", new object[] { "BeginRead", "read" }));
            }
            try
            {
                if (this.m_ReadCallback == null)
                {
                    this.m_ReadCallback = new AsyncCallback(this.ReadCallback);
                }
                if (this.m_ShadowStreamIsDead && (this.m_BytesToSkip == 0L))
                {
                    return base.WrappedStream.BeginRead(buffer, offset, count, callback, state);
                }
                InnerAsyncResult result = new InnerAsyncResult(state, callback, buffer, offset, count);
                if (this.m_BytesToSkip != 0L)
                {
                    InnerAsyncResult userState = result;
                    result = new InnerAsyncResult(userState, null, new byte[0x1000], 0, (this.m_BytesToSkip < buffer.Length) ? ((int) this.m_BytesToSkip) : buffer.Length);
                }
                IAsyncResult transportResult = base.WrappedStream.BeginRead(result.Buffer, result.Offset, result.Count, this.m_ReadCallback, result);
                if (transportResult.CompletedSynchronously)
                {
                    this.ReadComplete(transportResult);
                }
                result4 = result;
            }
            catch
            {
                Interlocked.Decrement(ref this.m_ReadNesting);
                throw;
            }
            return result4;
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        protected sealed override void Dispose(bool disposing)
        {
            this.Dispose(disposing, CloseExState.Normal);
        }

        protected virtual void Dispose(bool disposing, CloseExState closeState)
        {
            try
            {
                if (disposing)
                {
                    try
                    {
                        ICloseEx wrappedStream = base.WrappedStream as ICloseEx;
                        if (wrappedStream != null)
                        {
                            wrappedStream.CloseEx(closeState);
                        }
                        else
                        {
                            base.WrappedStream.Close();
                        }
                    }
                    finally
                    {
                        if (!this.m_SeenReadEOF)
                        {
                            closeState |= CloseExState.Abort;
                        }
                        if (this.m_ShadowStream is ICloseEx)
                        {
                            ((ICloseEx) this.m_ShadowStream).CloseEx(closeState);
                        }
                        else
                        {
                            this.m_ShadowStream.Close();
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (Interlocked.Decrement(ref this.m_ReadNesting) != 0)
            {
                Interlocked.Increment(ref this.m_ReadNesting);
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndRead" }));
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            InnerAsyncResult result = asyncResult as InnerAsyncResult;
            if ((result == null) && (base.WrappedStream.EndRead(asyncResult) == 0))
            {
                this.m_SeenReadEOF = true;
            }
            bool flag = false;
            try
            {
                result.InternalWaitForCompletion();
                if (result.Result is Exception)
                {
                    throw ((Exception) result.Result);
                }
                flag = true;
            }
            finally
            {
                if (!flag && !this.m_ShadowStreamIsDead)
                {
                    this.m_ShadowStreamIsDead = true;
                    if (this.m_ShadowStream is ICloseEx)
                    {
                        ((ICloseEx) this.m_ShadowStream).CloseEx(CloseExState.Silent | CloseExState.Abort);
                    }
                    else
                    {
                        this.m_ShadowStream.Close();
                    }
                }
            }
            return (int) result.Result;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num3;
            bool flag = false;
            int num = -1;
            if (Interlocked.Increment(ref this.m_ReadNesting) != 1)
            {
                throw new NotSupportedException(SR.GetString("net_io_invalidnestedcall", new object[] { "Read", "read" }));
            }
            try
            {
                if (this.m_BytesToSkip != 0L)
                {
                    byte[] buffer2 = new byte[0x1000];
                    while (this.m_BytesToSkip != 0L)
                    {
                        int num2 = base.WrappedStream.Read(buffer2, 0, (this.m_BytesToSkip < buffer2.Length) ? ((int) this.m_BytesToSkip) : buffer2.Length);
                        if (num2 == 0)
                        {
                            this.m_SeenReadEOF = true;
                        }
                        this.m_BytesToSkip -= num2;
                        if (!this.m_ShadowStreamIsDead)
                        {
                            this.m_ShadowStream.Write(buffer2, 0, num2);
                        }
                    }
                }
                num = base.WrappedStream.Read(buffer, offset, count);
                if (num == 0)
                {
                    this.m_SeenReadEOF = true;
                }
                if (this.m_ShadowStreamIsDead)
                {
                    return num;
                }
                flag = true;
                this.m_ShadowStream.Write(buffer, offset, num);
                num3 = num;
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!this.m_ShadowStreamIsDead)
                {
                    this.m_ShadowStreamIsDead = true;
                    try
                    {
                        if (this.m_ShadowStream is ICloseEx)
                        {
                            ((ICloseEx) this.m_ShadowStream).CloseEx(CloseExState.Silent | CloseExState.Abort);
                        }
                        else
                        {
                            this.m_ShadowStream.Close();
                        }
                    }
                    catch (Exception)
                    {
                        if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                        {
                            throw;
                        }
                    }
                }
                if (!flag || this.m_ThrowOnWriteError)
                {
                    throw;
                }
                num3 = num;
            }
            finally
            {
                Interlocked.Decrement(ref this.m_ReadNesting);
            }
            return num3;
        }

        private void ReadCallback(IAsyncResult transportResult)
        {
            if (!transportResult.CompletedSynchronously)
            {
                object asyncState = transportResult.AsyncState;
                this.ReadComplete(transportResult);
            }
        }

        private void ReadComplete(IAsyncResult transportResult)
        {
            InnerAsyncResult asyncState;
        Label_0000:
            asyncState = transportResult.AsyncState as InnerAsyncResult;
            try
            {
                if (!asyncState.IsWriteCompletion)
                {
                    asyncState.Count = base.WrappedStream.EndRead(transportResult);
                    if (asyncState.Count == 0)
                    {
                        this.m_SeenReadEOF = true;
                    }
                    if (!this.m_ShadowStreamIsDead)
                    {
                        asyncState.IsWriteCompletion = true;
                        transportResult = this.m_ShadowStream.BeginWrite(asyncState.Buffer, asyncState.Offset, asyncState.Count, this.m_ReadCallback, asyncState);
                        if (!transportResult.CompletedSynchronously)
                        {
                            return;
                        }
                        goto Label_0000;
                    }
                }
                else
                {
                    this.m_ShadowStream.EndWrite(transportResult);
                    asyncState.IsWriteCompletion = false;
                }
            }
            catch (Exception exception)
            {
                if (asyncState.InternalPeekCompleted)
                {
                    throw;
                }
                try
                {
                    this.m_ShadowStreamIsDead = true;
                    if (this.m_ShadowStream is ICloseEx)
                    {
                        ((ICloseEx) this.m_ShadowStream).CloseEx(CloseExState.Silent | CloseExState.Abort);
                    }
                    else
                    {
                        this.m_ShadowStream.Close();
                    }
                }
                catch (Exception)
                {
                }
                if (!asyncState.IsWriteCompletion || this.m_ThrowOnWriteError)
                {
                    if (transportResult.CompletedSynchronously)
                    {
                        throw;
                    }
                    asyncState.InvokeCallback(exception);
                    return;
                }
            }
            try
            {
                if (this.m_BytesToSkip != 0L)
                {
                    this.m_BytesToSkip -= asyncState.Count;
                    asyncState.Count = (this.m_BytesToSkip < asyncState.Buffer.Length) ? ((int) this.m_BytesToSkip) : asyncState.Buffer.Length;
                    if (this.m_BytesToSkip == 0L)
                    {
                        transportResult = asyncState;
                        asyncState = asyncState.AsyncState as InnerAsyncResult;
                    }
                    transportResult = base.WrappedStream.BeginRead(asyncState.Buffer, asyncState.Offset, asyncState.Count, this.m_ReadCallback, asyncState);
                    if (transportResult.CompletedSynchronously)
                    {
                        goto Label_0000;
                    }
                }
                else
                {
                    asyncState.InvokeCallback(asyncState.Count);
                }
            }
            catch (Exception exception2)
            {
                if (asyncState.InternalPeekCompleted)
                {
                    throw;
                }
                try
                {
                    this.m_ShadowStreamIsDead = true;
                    if (this.m_ShadowStream is ICloseEx)
                    {
                        ((ICloseEx) this.m_ShadowStream).CloseEx(CloseExState.Silent | CloseExState.Abort);
                    }
                    else
                    {
                        this.m_ShadowStream.Close();
                    }
                }
                catch (Exception)
                {
                }
                if (transportResult.CompletedSynchronously)
                {
                    throw;
                }
                asyncState.InvokeCallback(exception2);
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

        void ICloseEx.CloseEx(CloseExState closeState)
        {
            if (Interlocked.Increment(ref this._Disposed) == 1)
            {
                if (closeState == CloseExState.Silent)
                {
                    try
                    {
                        int num2;
                        for (int i = 0; (i < ConnectStream.s_DrainingBuffer.Length) && ((num2 = this.Read(ConnectStream.s_DrainingBuffer, 0, ConnectStream.s_DrainingBuffer.Length)) > 0); i += num2)
                        {
                        }
                    }
                    catch (Exception exception)
                    {
                        if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                        {
                            throw;
                        }
                    }
                }
                this.Dispose(true, closeState);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        public override bool CanRead
        {
            get
            {
                return base.WrappedStream.CanRead;
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
                return (base.WrappedStream.CanTimeout && this.m_ShadowStream.CanTimeout);
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
                return (base.WrappedStream.Length - this.m_BytesToSkip);
            }
        }

        public override long Position
        {
            get
            {
                return (base.WrappedStream.Position - this.m_BytesToSkip);
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
                return base.WrappedStream.ReadTimeout;
            }
            set
            {
                base.WrappedStream.ReadTimeout = this.m_ShadowStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return this.m_ShadowStream.WriteTimeout;
            }
            set
            {
                base.WrappedStream.WriteTimeout = this.m_ShadowStream.WriteTimeout = value;
            }
        }

        private class InnerAsyncResult : LazyAsyncResult
        {
            public byte[] Buffer;
            public int Count;
            public bool IsWriteCompletion;
            public int Offset;

            public InnerAsyncResult(object userState, AsyncCallback userCallback, byte[] buffer, int offset, int count) : base(null, userState, userCallback)
            {
                this.Buffer = buffer;
                this.Offset = offset;
                this.Count = count;
            }
        }
    }
}

