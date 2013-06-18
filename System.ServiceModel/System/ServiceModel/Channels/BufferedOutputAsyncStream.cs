namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class BufferedOutputAsyncStream : Stream
    {
        private int bufferLimit;
        private List<ByteBuffer> buffers;
        private int bufferSize;
        private int currentIndex;
        private Stream stream;

        internal BufferedOutputAsyncStream(Stream stream, int bufferSize, int bufferLimit)
        {
            this.stream = stream;
            this.bufferSize = bufferSize;
            this.bufferLimit = bufferLimit;
            this.buffers = new List<ByteBuffer>();
            this.buffers.Add(new ByteBuffer(this.bufferSize, stream));
            this.currentIndex = 0;
        }

        public override void Close()
        {
            this.CurrentBuffer.Flush();
            this.stream.Close();
            this.WaitForAllWritesToComplete();
        }

        public override void Flush()
        {
            this.CurrentBuffer.Flush();
            this.stream.Flush();
        }

        private void NextBuffer()
        {
            this.currentIndex++;
            if (this.currentIndex == this.buffers.Count)
            {
                if (this.buffers.Count < this.bufferLimit)
                {
                    this.buffers.Add(new ByteBuffer(this.bufferSize, this.stream));
                    return;
                }
                this.currentIndex = 0;
            }
            this.CurrentBuffer.WaitForWriteComplete();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("ReadNotSupported")));
        }

        public override int ReadByte()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("ReadNotSupported")));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SeekNotSupported")));
        }

        public override void SetLength(long value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SeekNotSupported")));
        }

        private void WaitForAllWritesToComplete()
        {
            for (int i = 0; i < this.buffers.Count; i++)
            {
                this.buffers[i].WaitForWriteComplete();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                if (this.CurrentBuffer.IsWritePending)
                {
                    this.NextBuffer();
                }
                int freeBytes = this.CurrentBuffer.FreeBytes;
                if (freeBytes > 0)
                {
                    if (freeBytes > count)
                    {
                        freeBytes = count;
                    }
                    this.CurrentBuffer.CopyData(buffer, offset, freeBytes);
                    offset += freeBytes;
                    count -= freeBytes;
                }
                if (this.CurrentBuffer.FreeBytes == 0)
                {
                    this.CurrentBuffer.Flush();
                }
            }
        }

        public override void WriteByte(byte value)
        {
            if (this.CurrentBuffer.IsWritePending)
            {
                this.NextBuffer();
            }
            this.CurrentBuffer.CopyData(value);
            if (this.CurrentBuffer.FreeBytes == 0)
            {
                this.CurrentBuffer.Flush();
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
                return this.stream.CanWrite;
            }
        }

        private ByteBuffer CurrentBuffer
        {
            get
            {
                return this.buffers[this.currentIndex];
            }
        }

        public override long Length
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("ReadNotSupported")));
            }
        }

        public override long Position
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SeekNotSupported")));
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SeekNotSupported")));
            }
        }

        private class ByteBuffer
        {
            private byte[] bytes;
            private Exception completionException;
            private int position;
            private Stream stream;
            private bool waiting;
            private static AsyncCallback writeCallback = Fx.ThunkCallback(new AsyncCallback(BufferedOutputAsyncStream.ByteBuffer.WriteCallback));
            private bool writePending;

            internal ByteBuffer(int bufferSize, Stream stream)
            {
                this.bytes = DiagnosticUtility.Utility.AllocateByteArray(bufferSize);
                this.stream = stream;
            }

            internal void CopyData(byte value)
            {
                this.bytes[this.position++] = value;
            }

            internal void CopyData(byte[] buffer, int offset, int count)
            {
                Buffer.BlockCopy(buffer, offset, this.bytes, this.position, count);
                this.position += count;
            }

            internal void Flush()
            {
                if (this.position > 0)
                {
                    int position = this.position;
                    this.writePending = true;
                    this.position = 0;
                    IAsyncResult asyncResult = this.stream.BeginWrite(this.bytes, 0, position, writeCallback, this);
                    if (asyncResult.CompletedSynchronously)
                    {
                        this.stream.EndWrite(asyncResult);
                        this.writePending = false;
                    }
                }
            }

            internal void WaitForWriteComplete()
            {
                lock (this.ThisLock)
                {
                    if (this.writePending)
                    {
                        this.waiting = true;
                        Monitor.Wait(this.ThisLock);
                        this.waiting = false;
                    }
                }
                if (this.completionException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.completionException);
                }
            }

            private static void WriteCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    BufferedOutputAsyncStream.ByteBuffer asyncState = (BufferedOutputAsyncStream.ByteBuffer) result.AsyncState;
                    try
                    {
                        asyncState.stream.EndWrite(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.completionException = exception;
                    }
                    lock (asyncState.ThisLock)
                    {
                        asyncState.writePending = false;
                        if (asyncState.waiting)
                        {
                            Monitor.Pulse(asyncState.ThisLock);
                        }
                    }
                }
            }

            internal int FreeBytes
            {
                get
                {
                    return (this.bytes.Length - this.position);
                }
            }

            internal bool IsWritePending
            {
                get
                {
                    return this.writePending;
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
}

