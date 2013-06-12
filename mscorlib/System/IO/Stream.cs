namespace System.IO
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public abstract class Stream : MarshalByRefObject, IDisposable
    {
        [NonSerialized]
        private int _asyncActiveCount = 1;
        [NonSerialized]
        private AutoResetEvent _asyncActiveEvent;
        private const int _DefaultBufferSize = 0x1000;
        [NonSerialized]
        private ReadDelegate _readDelegate;
        [NonSerialized]
        private WriteDelegate _writeDelegate;
        public static readonly Stream Null = new NullStream();

        protected Stream()
        {
        }

        private void _CloseAsyncActiveEvent(int asyncActiveCount)
        {
            if ((this._asyncActiveEvent != null) && (asyncActiveCount == 0))
            {
                this._asyncActiveEvent.Close();
                this._asyncActiveEvent = null;
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!this.CanRead)
            {
                __Error.ReadNotSupported();
            }
            Interlocked.Increment(ref this._asyncActiveCount);
            ReadDelegate delegate2 = new ReadDelegate(this.Read);
            if (this._asyncActiveEvent == null)
            {
                lock (this)
                {
                    if (this._asyncActiveEvent == null)
                    {
                        this._asyncActiveEvent = new AutoResetEvent(true);
                    }
                }
            }
            this._asyncActiveEvent.WaitOne();
            this._readDelegate = delegate2;
            return delegate2.BeginInvoke(buffer, offset, count, callback, state);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!this.CanWrite)
            {
                __Error.WriteNotSupported();
            }
            Interlocked.Increment(ref this._asyncActiveCount);
            WriteDelegate delegate2 = new WriteDelegate(this.Write);
            if (this._asyncActiveEvent == null)
            {
                lock (this)
                {
                    if (this._asyncActiveEvent == null)
                    {
                        this._asyncActiveEvent = new AutoResetEvent(true);
                    }
                }
            }
            this._asyncActiveEvent.WaitOne();
            this._writeDelegate = delegate2;
            return delegate2.BeginInvoke(buffer, offset, count, callback, state);
        }

        internal IAsyncResult BlockingBeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            SynchronousAsyncResult ar = new SynchronousAsyncResult(state, false);
            try
            {
                int num = this.Read(buffer, offset, count);
                ar._numRead = num;
            }
            catch (IOException exception)
            {
                ar._exception = exception;
            }
            finally
            {
                ar._isCompleted = true;
                ar._waitHandle.Set();
            }
            if (callback != null)
            {
                callback(ar);
            }
            return ar;
        }

        internal IAsyncResult BlockingBeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            SynchronousAsyncResult ar = new SynchronousAsyncResult(state, true);
            try
            {
                this.Write(buffer, offset, count);
            }
            catch (IOException exception)
            {
                ar._exception = exception;
            }
            finally
            {
                ar._isCompleted = true;
                ar._waitHandle.Set();
            }
            if (callback != null)
            {
                callback(ar);
            }
            return ar;
        }

        internal static int BlockingEndRead(IAsyncResult asyncResult)
        {
            SynchronousAsyncResult result = asyncResult as SynchronousAsyncResult;
            if ((result == null) || result.IsWrite)
            {
                __Error.WrongAsyncResult();
            }
            if (result._EndXxxCalled)
            {
                __Error.EndReadCalledTwice();
            }
            result._EndXxxCalled = true;
            if (result._exception != null)
            {
                throw result._exception;
            }
            return result._numRead;
        }

        internal static void BlockingEndWrite(IAsyncResult asyncResult)
        {
            SynchronousAsyncResult result = asyncResult as SynchronousAsyncResult;
            if ((result == null) || !result.IsWrite)
            {
                __Error.WrongAsyncResult();
            }
            if (result._EndXxxCalled)
            {
                __Error.EndWriteCalledTwice();
            }
            result._EndXxxCalled = true;
            if (result._exception != null)
            {
                throw result._exception;
            }
        }

        public virtual void Close()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void CopyTo(Stream destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (!this.CanRead && !this.CanWrite)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_StreamClosed"));
            }
            if (!destination.CanRead && !destination.CanWrite)
            {
                throw new ObjectDisposedException("destination", Environment.GetResourceString("ObjectDisposed_StreamClosed"));
            }
            if (!this.CanRead)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnreadableStream"));
            }
            if (!destination.CanWrite)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnwritableStream"));
            }
            this.InternalCopyTo(destination, 0x1000);
        }

        public void CopyTo(Stream destination, int bufferSize)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            if (!this.CanRead && !this.CanWrite)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_StreamClosed"));
            }
            if (!destination.CanRead && !destination.CanWrite)
            {
                throw new ObjectDisposedException("destination", Environment.GetResourceString("ObjectDisposed_StreamClosed"));
            }
            if (!this.CanRead)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnreadableStream"));
            }
            if (!destination.CanWrite)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnwritableStream"));
            }
            this.InternalCopyTo(destination, bufferSize);
        }

        [Obsolete("CreateWaitHandle will be removed eventually.  Please use \"new ManualResetEvent(false)\" instead.")]
        protected virtual WaitHandle CreateWaitHandle()
        {
            return new ManualResetEvent(false);
        }

        public void Dispose()
        {
            this.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this._asyncActiveEvent != null))
            {
                this._CloseAsyncActiveEvent(Interlocked.Decrement(ref this._asyncActiveCount));
            }
        }

        public virtual int EndRead(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (this._readDelegate == null)
            {
                throw new ArgumentException(Environment.GetResourceString("InvalidOperation_WrongAsyncResultOrEndReadCalledMultiple"));
            }
            int num = -1;
            try
            {
                num = this._readDelegate.EndInvoke(asyncResult);
            }
            finally
            {
                this._readDelegate = null;
                this._asyncActiveEvent.Set();
                this._CloseAsyncActiveEvent(Interlocked.Decrement(ref this._asyncActiveCount));
            }
            return num;
        }

        public virtual void EndWrite(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (this._writeDelegate == null)
            {
                throw new ArgumentException(Environment.GetResourceString("InvalidOperation_WrongAsyncResultOrEndWriteCalledMultiple"));
            }
            try
            {
                this._writeDelegate.EndInvoke(asyncResult);
            }
            finally
            {
                this._writeDelegate = null;
                this._asyncActiveEvent.Set();
                this._CloseAsyncActiveEvent(Interlocked.Decrement(ref this._asyncActiveCount));
            }
        }

        public abstract void Flush();
        private void InternalCopyTo(Stream destination, int bufferSize)
        {
            int num;
            byte[] buffer = new byte[bufferSize];
            while ((num = this.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, num);
            }
        }

        protected virtual void ObjectInvariant()
        {
        }

        public abstract int Read([In, Out] byte[] buffer, int offset, int count);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual int ReadByte()
        {
            byte[] buffer = new byte[1];
            if (this.Read(buffer, 0, 1) == 0)
            {
                return -1;
            }
            return buffer[0];
        }

        public abstract long Seek(long offset, SeekOrigin origin);
        public abstract void SetLength(long value);
        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public static Stream Synchronized(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (stream is SyncStream)
            {
                return stream;
            }
            return new SyncStream(stream);
        }

        public abstract void Write(byte[] buffer, int offset, int count);
        public virtual void WriteByte(byte value)
        {
            byte[] buffer = new byte[] { value };
            this.Write(buffer, 0, 1);
        }

        public abstract bool CanRead { get; }

        public abstract bool CanSeek { get; }

        [ComVisible(false)]
        public virtual bool CanTimeout
        {
            get
            {
                return false;
            }
        }

        public abstract bool CanWrite { get; }

        public abstract long Length { get; }

        public abstract long Position { get; set; }

        [ComVisible(false)]
        public virtual int ReadTimeout
        {
            get
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TimeoutsNotSupported"));
            }
            set
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TimeoutsNotSupported"));
            }
        }

        [ComVisible(false)]
        public virtual int WriteTimeout
        {
            get
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TimeoutsNotSupported"));
            }
            set
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TimeoutsNotSupported"));
            }
        }

        [Serializable]
        private sealed class NullStream : Stream
        {
            internal NullStream()
            {
            }

            [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                if (!this.CanRead)
                {
                    __Error.ReadNotSupported();
                }
                return base.BlockingBeginRead(buffer, offset, count, callback, state);
            }

            [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                if (!this.CanWrite)
                {
                    __Error.WriteNotSupported();
                }
                return base.BlockingBeginWrite(buffer, offset, count, callback, state);
            }

            protected override void Dispose(bool disposing)
            {
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                if (asyncResult == null)
                {
                    throw new ArgumentNullException("asyncResult");
                }
                return Stream.BlockingEndRead(asyncResult);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                if (asyncResult == null)
                {
                    throw new ArgumentNullException("asyncResult");
                }
                Stream.BlockingEndWrite(asyncResult);
            }

            public override void Flush()
            {
            }

            public override int Read([In, Out] byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override int ReadByte()
            {
                return -1;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return 0L;
            }

            public override void SetLength(long length)
            {
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
            }

            public override void WriteByte(byte value)
            {
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
                    return true;
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
                    return 0L;
                }
            }

            public override long Position
            {
                get
                {
                    return 0L;
                }
                set
                {
                }
            }
        }

        private delegate int ReadDelegate([In, Out] byte[] bytes, int index, int offset);

        private sealed class SynchronousAsyncResult : IAsyncResult
        {
            internal bool _EndXxxCalled;
            internal Exception _exception;
            internal bool _isCompleted;
            internal bool _isWrite;
            internal int _numRead;
            internal object _stateObject;
            internal ManualResetEvent _waitHandle;

            internal SynchronousAsyncResult(object asyncStateObject, bool isWrite)
            {
                this._stateObject = asyncStateObject;
                this._isWrite = isWrite;
                this._waitHandle = new ManualResetEvent(false);
            }

            public object AsyncState
            {
                get
                {
                    return this._stateObject;
                }
            }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    return this._waitHandle;
                }
            }

            public bool CompletedSynchronously
            {
                get
                {
                    return true;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return this._isCompleted;
                }
            }

            internal bool IsWrite
            {
                get
                {
                    return this._isWrite;
                }
            }
        }

        [Serializable]
        internal sealed class SyncStream : Stream, IDisposable
        {
            private Stream _stream;

            internal SyncStream(Stream stream)
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("stream");
                }
                this._stream = stream;
            }

            [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                lock (this._stream)
                {
                    return this._stream.BeginRead(buffer, offset, count, callback, state);
                }
            }

            [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                lock (this._stream)
                {
                    return this._stream.BeginWrite(buffer, offset, count, callback, state);
                }
            }

            public override void Close()
            {
                lock (this._stream)
                {
                    try
                    {
                        this._stream.Close();
                    }
                    finally
                    {
                        base.Dispose(true);
                    }
                }
            }

            protected override void Dispose(bool disposing)
            {
                lock (this._stream)
                {
                    try
                    {
                        if (disposing)
                        {
                            this._stream.Dispose();
                        }
                    }
                    finally
                    {
                        base.Dispose(disposing);
                    }
                }
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                lock (this._stream)
                {
                    return this._stream.EndRead(asyncResult);
                }
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                lock (this._stream)
                {
                    this._stream.EndWrite(asyncResult);
                }
            }

            public override void Flush()
            {
                lock (this._stream)
                {
                    this._stream.Flush();
                }
            }

            public override int Read([In, Out] byte[] bytes, int offset, int count)
            {
                lock (this._stream)
                {
                    return this._stream.Read(bytes, offset, count);
                }
            }

            public override int ReadByte()
            {
                lock (this._stream)
                {
                    return this._stream.ReadByte();
                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                lock (this._stream)
                {
                    return this._stream.Seek(offset, origin);
                }
            }

            public override void SetLength(long length)
            {
                lock (this._stream)
                {
                    this._stream.SetLength(length);
                }
            }

            public override void Write(byte[] bytes, int offset, int count)
            {
                lock (this._stream)
                {
                    this._stream.Write(bytes, offset, count);
                }
            }

            public override void WriteByte(byte b)
            {
                lock (this._stream)
                {
                    this._stream.WriteByte(b);
                }
            }

            public override bool CanRead
            {
                get
                {
                    return this._stream.CanRead;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return this._stream.CanSeek;
                }
            }

            [ComVisible(false)]
            public override bool CanTimeout
            {
                get
                {
                    return this._stream.CanTimeout;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return this._stream.CanWrite;
                }
            }

            public override long Length
            {
                get
                {
                    lock (this._stream)
                    {
                        return this._stream.Length;
                    }
                }
            }

            public override long Position
            {
                get
                {
                    lock (this._stream)
                    {
                        return this._stream.Position;
                    }
                }
                set
                {
                    lock (this._stream)
                    {
                        this._stream.Position = value;
                    }
                }
            }

            [ComVisible(false)]
            public override int ReadTimeout
            {
                get
                {
                    return this._stream.ReadTimeout;
                }
                set
                {
                    this._stream.ReadTimeout = value;
                }
            }

            [ComVisible(false)]
            public override int WriteTimeout
            {
                get
                {
                    return this._stream.WriteTimeout;
                }
                set
                {
                    this._stream.WriteTimeout = value;
                }
            }
        }

        private delegate void WriteDelegate(byte[] bytes, int index, int offset);
    }
}

