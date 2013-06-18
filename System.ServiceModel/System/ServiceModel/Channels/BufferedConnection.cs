namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class BufferedConnection : DelegatingConnection
    {
        private long flushTimeout;
        private IOThreadTimer flushTimer;
        private const int maxFlushSkew = 100;
        private TimeSpan pendingTimeout;
        private Exception pendingWriteException;
        private int pendingWriteSize;
        private byte[] writeBuffer;
        private int writeBufferSize;

        public BufferedConnection(IConnection connection, TimeSpan flushTimeout, int writeBufferSize) : base(connection)
        {
            this.flushTimeout = Ticks.FromTimeSpan(flushTimeout);
            this.writeBufferSize = writeBufferSize;
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThreadTrace.Trace("BC:BeginWrite");
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.Flush(helper.RemainingTime());
            return base.BeginWrite(buffer, offset, size, immediate, helper.RemainingTime(), callback, state);
        }

        private void CancelFlushTimer()
        {
            if (this.flushTimer != null)
            {
                this.flushTimer.Cancel();
                this.pendingTimeout = TimeSpan.Zero;
            }
        }

        public override void Close(TimeSpan timeout, bool asyncAndLinger)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.Flush(helper.RemainingTime());
            base.Close(helper.RemainingTime(), asyncAndLinger);
        }

        public override void EndWrite(IAsyncResult result)
        {
            ThreadTrace.Trace("BC:EndWrite");
            base.EndWrite(result);
        }

        private void Flush(TimeSpan timeout)
        {
            this.ThrowPendingWriteException();
            lock (this.ThisLock)
            {
                this.FlushCore(timeout);
            }
        }

        private void FlushCore(TimeSpan timeout)
        {
            if (this.pendingWriteSize > 0)
            {
                ThreadTrace.Trace("BC:Flush");
                base.Connection.Write(this.writeBuffer, 0, this.pendingWriteSize, false, timeout);
                this.pendingWriteSize = 0;
            }
        }

        private void OnFlushTimer(object state)
        {
            ThreadTrace.Trace("BC:Flush timer");
            lock (this.ThisLock)
            {
                try
                {
                    this.FlushCore(this.pendingTimeout);
                    this.pendingTimeout = TimeSpan.Zero;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.pendingWriteException = exception;
                    this.CancelFlushTimer();
                }
            }
        }

        private void SetFlushTimer()
        {
            if (this.flushTimer == null)
            {
                int maxSkewInMilliseconds = Ticks.ToMilliseconds(Math.Min(this.flushTimeout / 10L, Ticks.FromMilliseconds(100)));
                this.flushTimer = new IOThreadTimer(new Action<object>(this.OnFlushTimer), null, true, maxSkewInMilliseconds);
            }
            this.flushTimer.Set(Ticks.ToTimeSpan(this.flushTimeout));
        }

        public override void Shutdown(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.Flush(helper.RemainingTime());
            base.Shutdown(helper.RemainingTime());
        }

        private void ThrowPendingWriteException()
        {
            if (this.pendingWriteException != null)
            {
                lock (this.ThisLock)
                {
                    if (this.pendingWriteException != null)
                    {
                        Exception pendingWriteException = this.pendingWriteException;
                        this.pendingWriteException = null;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(pendingWriteException);
                    }
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
        {
            if (size <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("size", size, System.ServiceModel.SR.GetString("ValueMustBePositive")));
            }
            this.ThrowPendingWriteException();
            if (immediate || (this.flushTimeout == 0L))
            {
                ThreadTrace.Trace("BC:Write now");
                this.WriteNow(buffer, offset, size, timeout);
            }
            else
            {
                ThreadTrace.Trace("BC:Write later");
                this.WriteLater(buffer, offset, size, timeout);
            }
            ThreadTrace.Trace("BC:Write done");
        }

        public override void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager)
        {
            if (size <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("size", size, System.ServiceModel.SR.GetString("ValueMustBePositive")));
            }
            this.ThrowPendingWriteException();
            if (immediate || (this.flushTimeout == 0L))
            {
                ThreadTrace.Trace("BC:Write now");
                this.WriteNow(buffer, offset, size, timeout, bufferManager);
            }
            else
            {
                ThreadTrace.Trace("BC:Write later");
                this.WriteLater(buffer, offset, size, timeout);
                bufferManager.ReturnBuffer(buffer);
            }
            ThreadTrace.Trace("BC:Write done");
        }

        private void WriteLater(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            lock (this.ThisLock)
            {
                bool flag = this.pendingWriteSize == 0;
                TimeoutHelper helper = new TimeoutHelper(timeout);
                while (size > 0)
                {
                    if ((size >= this.writeBufferSize) && (this.pendingWriteSize == 0))
                    {
                        base.Connection.Write(buffer, offset, size, false, helper.RemainingTime());
                        size = 0;
                    }
                    else
                    {
                        if (this.writeBuffer == null)
                        {
                            this.writeBuffer = DiagnosticUtility.Utility.AllocateByteArray(this.writeBufferSize);
                        }
                        int num = this.writeBufferSize - this.pendingWriteSize;
                        int count = size;
                        if (count > num)
                        {
                            count = num;
                        }
                        Buffer.BlockCopy(buffer, offset, this.writeBuffer, this.pendingWriteSize, count);
                        this.pendingWriteSize += count;
                        if (this.pendingWriteSize == this.writeBufferSize)
                        {
                            this.FlushCore(helper.RemainingTime());
                            flag = true;
                        }
                        size -= count;
                        offset += count;
                    }
                }
                if (this.pendingWriteSize > 0)
                {
                    if (flag)
                    {
                        this.SetFlushTimer();
                        this.pendingTimeout = TimeoutHelper.Add(this.pendingTimeout, helper.RemainingTime());
                    }
                }
                else
                {
                    this.CancelFlushTimer();
                }
            }
        }

        private void WriteNow(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            this.WriteNow(buffer, offset, size, timeout, null);
        }

        private void WriteNow(byte[] buffer, int offset, int size, TimeSpan timeout, BufferManager bufferManager)
        {
            lock (this.ThisLock)
            {
                if (this.pendingWriteSize > 0)
                {
                    int num = this.writeBufferSize - this.pendingWriteSize;
                    this.CancelFlushTimer();
                    if (size <= num)
                    {
                        Buffer.BlockCopy(buffer, offset, this.writeBuffer, this.pendingWriteSize, size);
                        if (bufferManager != null)
                        {
                            bufferManager.ReturnBuffer(buffer);
                        }
                        this.pendingWriteSize += size;
                        this.FlushCore(timeout);
                        goto Label_00BF;
                    }
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    this.FlushCore(helper.RemainingTime());
                    timeout = helper.RemainingTime();
                }
                if (bufferManager == null)
                {
                    base.Connection.Write(buffer, offset, size, true, timeout);
                }
                else
                {
                    base.Connection.Write(buffer, offset, size, true, timeout, bufferManager);
                }
            Label_00BF:;
            }
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }
    }
}

