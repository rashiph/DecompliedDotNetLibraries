namespace System.IO.Compression
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;

    public class DeflateStream : Stream
    {
        private bool _leaveOpen;
        private CompressionMode _mode;
        private Stream _stream;
        private int asyncOperations;
        private byte[] buffer;
        private const int bufferSize = 0x1000;
        internal const int DefaultBufferSize = 0x1000;
        private Deflater deflater;
        private IFileFormatWriter formatWriter;
        private Inflater inflater;
        private readonly AsyncWriteDelegate m_AsyncWriterDelegate;
        private readonly AsyncCallback m_CallBack;
        private bool wroteHeader;

        public DeflateStream(Stream stream, CompressionMode mode) : this(stream, mode, false)
        {
        }

        public DeflateStream(Stream stream, CompressionMode mode, bool leaveOpen)
        {
            this._stream = stream;
            this._mode = mode;
            this._leaveOpen = leaveOpen;
            if (this._stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            switch (this._mode)
            {
                case CompressionMode.Decompress:
                    if (!this._stream.CanRead)
                    {
                        throw new ArgumentException(SR.GetString("NotReadableStream"), "stream");
                    }
                    this.inflater = new Inflater();
                    this.m_CallBack = new AsyncCallback(this.ReadCallback);
                    break;

                case CompressionMode.Compress:
                    if (!this._stream.CanWrite)
                    {
                        throw new ArgumentException(SR.GetString("NotWriteableStream"), "stream");
                    }
                    this.deflater = new Deflater();
                    this.m_AsyncWriterDelegate = new AsyncWriteDelegate(this.InternalWrite);
                    this.m_CallBack = new AsyncCallback(this.WriteCallback);
                    break;

                default:
                    throw new ArgumentException(SR.GetString("ArgumentOutOfRange_Enum"), "mode");
            }
            this.buffer = new byte[0x1000];
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            IAsyncResult result2;
            this.EnsureDecompressionMode();
            if (this.asyncOperations != 0)
            {
                throw new InvalidOperationException(SR.GetString("InvalidBeginCall"));
            }
            Interlocked.Increment(ref this.asyncOperations);
            try
            {
                this.ValidateParameters(array, offset, count);
                DeflateStreamAsyncResult state = new DeflateStreamAsyncResult(this, asyncState, asyncCallback, array, offset, count) {
                    isWrite = false
                };
                int result = this.inflater.Inflate(array, offset, count);
                if (result != 0)
                {
                    state.InvokeCallback(true, result);
                    return state;
                }
                if (this.inflater.Finished())
                {
                    state.InvokeCallback(true, 0);
                    return state;
                }
                this._stream.BeginRead(this.buffer, 0, this.buffer.Length, this.m_CallBack, state);
                state.m_CompletedSynchronously &= state.IsCompleted;
                result2 = state;
            }
            catch
            {
                Interlocked.Decrement(ref this.asyncOperations);
                throw;
            }
            return result2;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            IAsyncResult result2;
            this.EnsureCompressionMode();
            if (this.asyncOperations != 0)
            {
                throw new InvalidOperationException(SR.GetString("InvalidBeginCall"));
            }
            Interlocked.Increment(ref this.asyncOperations);
            try
            {
                this.ValidateParameters(array, offset, count);
                DeflateStreamAsyncResult result = new DeflateStreamAsyncResult(this, asyncState, asyncCallback, array, offset, count) {
                    isWrite = true
                };
                this.m_AsyncWriterDelegate.BeginInvoke(array, offset, count, true, this.m_CallBack, result);
                result.m_CompletedSynchronously &= result.IsCompleted;
                result2 = result;
            }
            catch
            {
                Interlocked.Decrement(ref this.asyncOperations);
                throw;
            }
            return result2;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (this._stream != null))
                {
                    this.Flush();
                    if ((this._mode == CompressionMode.Compress) && (this._stream != null))
                    {
                        int deflateOutput;
                        while (!this.deflater.NeedsInput())
                        {
                            deflateOutput = this.deflater.GetDeflateOutput(this.buffer);
                            if (deflateOutput != 0)
                            {
                                this._stream.Write(this.buffer, 0, deflateOutput);
                            }
                        }
                        deflateOutput = this.deflater.Finish(this.buffer);
                        if (deflateOutput > 0)
                        {
                            this.DoWrite(this.buffer, 0, deflateOutput, false);
                        }
                        if ((this.formatWriter != null) && this.wroteHeader)
                        {
                            byte[] footer = this.formatWriter.GetFooter();
                            this._stream.Write(footer, 0, footer.Length);
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    if ((disposing && !this._leaveOpen) && (this._stream != null))
                    {
                        this._stream.Close();
                    }
                }
                finally
                {
                    this._stream = null;
                    base.Dispose(disposing);
                }
            }
        }

        private void DoMaintenance(byte[] array, int offset, int count)
        {
            if (this.formatWriter != null)
            {
                if (!this.wroteHeader && (count > 0))
                {
                    byte[] header = this.formatWriter.GetHeader();
                    this._stream.Write(header, 0, header.Length);
                    this.wroteHeader = true;
                }
                if (count > 0)
                {
                    this.formatWriter.UpdateWithBytesRead(array, offset, count);
                }
            }
        }

        private void DoWrite(byte[] array, int offset, int count, bool isAsync)
        {
            if (isAsync)
            {
                IAsyncResult asyncResult = this._stream.BeginWrite(array, offset, count, null, null);
                this._stream.EndWrite(asyncResult);
            }
            else
            {
                this._stream.Write(array, offset, count);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            this.EnsureDecompressionMode();
            if (this.asyncOperations != 1)
            {
                throw new InvalidOperationException(SR.GetString("InvalidEndCall"));
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (this._stream == null)
            {
                throw new InvalidOperationException(SR.GetString("ObjectDisposed_StreamClosed"));
            }
            DeflateStreamAsyncResult result = asyncResult as DeflateStreamAsyncResult;
            if (result == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            try
            {
                if (!result.IsCompleted)
                {
                    result.AsyncWaitHandle.WaitOne();
                }
            }
            finally
            {
                Interlocked.Decrement(ref this.asyncOperations);
                result.Close();
            }
            if (result.Result is Exception)
            {
                throw ((Exception) result.Result);
            }
            return (int) result.Result;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.EnsureCompressionMode();
            if (this.asyncOperations != 1)
            {
                throw new InvalidOperationException(SR.GetString("InvalidEndCall"));
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (this._stream == null)
            {
                throw new InvalidOperationException(SR.GetString("ObjectDisposed_StreamClosed"));
            }
            DeflateStreamAsyncResult result = asyncResult as DeflateStreamAsyncResult;
            if (result == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            try
            {
                if (!result.IsCompleted)
                {
                    result.AsyncWaitHandle.WaitOne();
                }
            }
            finally
            {
                Interlocked.Decrement(ref this.asyncOperations);
                result.Close();
            }
            if (result.Result is Exception)
            {
                throw ((Exception) result.Result);
            }
        }

        private void EnsureCompressionMode()
        {
            if (this._mode != CompressionMode.Compress)
            {
                throw new InvalidOperationException(SR.GetString("CannotWriteToDeflateStream"));
            }
        }

        private void EnsureDecompressionMode()
        {
            if (this._mode != CompressionMode.Decompress)
            {
                throw new InvalidOperationException(SR.GetString("CannotReadFromDeflateStream"));
            }
        }

        public override void Flush()
        {
            if (this._stream == null)
            {
                throw new ObjectDisposedException(null, SR.GetString("ObjectDisposed_StreamClosed"));
            }
        }

        internal void InternalWrite(byte[] array, int offset, int count, bool isAsync)
        {
            int deflateOutput;
            this.DoMaintenance(array, offset, count);
            while (!this.deflater.NeedsInput())
            {
                deflateOutput = this.deflater.GetDeflateOutput(this.buffer);
                if (deflateOutput != 0)
                {
                    this.DoWrite(this.buffer, 0, deflateOutput, isAsync);
                }
            }
            this.deflater.SetInput(array, offset, count);
            while (!this.deflater.NeedsInput())
            {
                deflateOutput = this.deflater.GetDeflateOutput(this.buffer);
                if (deflateOutput != 0)
                {
                    this.DoWrite(this.buffer, 0, deflateOutput, isAsync);
                }
            }
        }

        public override int Read(byte[] array, int offset, int count)
        {
            this.EnsureDecompressionMode();
            this.ValidateParameters(array, offset, count);
            int num2 = offset;
            int length = count;
            while (true)
            {
                int num = this.inflater.Inflate(array, num2, length);
                num2 += num;
                length -= num;
                if ((length == 0) || this.inflater.Finished())
                {
                    break;
                }
                int num4 = this._stream.Read(this.buffer, 0, this.buffer.Length);
                if (num4 == 0)
                {
                    break;
                }
                this.inflater.SetInput(this.buffer, 0, num4);
            }
            return (count - length);
        }

        private void ReadCallback(IAsyncResult baseStreamResult)
        {
            DeflateStreamAsyncResult asyncState = (DeflateStreamAsyncResult) baseStreamResult.AsyncState;
            asyncState.m_CompletedSynchronously &= baseStreamResult.CompletedSynchronously;
            int length = 0;
            try
            {
                length = this._stream.EndRead(baseStreamResult);
                if (length <= 0)
                {
                    asyncState.InvokeCallback(0);
                }
                else
                {
                    this.inflater.SetInput(this.buffer, 0, length);
                    length = this.inflater.Inflate(asyncState.buffer, asyncState.offset, asyncState.count);
                    if ((length == 0) && !this.inflater.Finished())
                    {
                        this._stream.BeginRead(this.buffer, 0, this.buffer.Length, this.m_CallBack, asyncState);
                    }
                    else
                    {
                        asyncState.InvokeCallback(length);
                    }
                }
            }
            catch (Exception exception)
            {
                asyncState.InvokeCallback(exception);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(SR.GetString("NotSupported"));
        }

        internal void SetFileFormatReader(IFileFormatReader reader)
        {
            if (reader != null)
            {
                this.inflater.SetFileFormatReader(reader);
            }
        }

        internal void SetFileFormatWriter(IFileFormatWriter writer)
        {
            if (writer != null)
            {
                this.formatWriter = writer;
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(SR.GetString("NotSupported"));
        }

        private void ValidateParameters(byte[] array, int offset, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if ((array.Length - offset) < count)
            {
                throw new ArgumentException(SR.GetString("InvalidArgumentOffsetCount"));
            }
            if (this._stream == null)
            {
                throw new ObjectDisposedException(null, SR.GetString("ObjectDisposed_StreamClosed"));
            }
        }

        public override void Write(byte[] array, int offset, int count)
        {
            this.EnsureCompressionMode();
            this.ValidateParameters(array, offset, count);
            this.InternalWrite(array, offset, count, false);
        }

        private void WriteCallback(IAsyncResult asyncResult)
        {
            DeflateStreamAsyncResult asyncState = (DeflateStreamAsyncResult) asyncResult.AsyncState;
            asyncState.m_CompletedSynchronously &= asyncResult.CompletedSynchronously;
            try
            {
                this.m_AsyncWriterDelegate.EndInvoke(asyncResult);
            }
            catch (Exception exception)
            {
                asyncState.InvokeCallback(exception);
                return;
            }
            asyncState.InvokeCallback(null);
        }

        public Stream BaseStream
        {
            get
            {
                return this._stream;
            }
        }

        public override bool CanRead
        {
            get
            {
                if (this._stream == null)
                {
                    return false;
                }
                return ((this._mode == CompressionMode.Decompress) && this._stream.CanRead);
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
                if (this._stream == null)
                {
                    return false;
                }
                return ((this._mode == CompressionMode.Compress) && this._stream.CanWrite);
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException(SR.GetString("NotSupported"));
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException(SR.GetString("NotSupported"));
            }
            set
            {
                throw new NotSupportedException(SR.GetString("NotSupported"));
            }
        }

        internal delegate void AsyncWriteDelegate(byte[] array, int offset, int count, bool isAsync);
    }
}

