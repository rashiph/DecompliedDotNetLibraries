namespace System.Runtime
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    internal class BufferedOutputStream : Stream
    {
        private InternalBufferManager bufferManager;
        private bool bufferReturned;
        private bool callerReturnsBuffer;
        private int chunkCount;
        private byte[][] chunks;
        private byte[] currentChunk;
        private int currentChunkSize;
        private bool initialized;
        private int maxSize;
        private int maxSizeQuota;
        private int totalSize;

        public BufferedOutputStream()
        {
            this.chunks = new byte[4][];
        }

        public BufferedOutputStream(int maxSize) : this(0, maxSize, InternalBufferManager.Create(0L, 0x7fffffff))
        {
        }

        public BufferedOutputStream(int initialSize, int maxSize, InternalBufferManager bufferManager) : this()
        {
            this.Reinitialize(initialSize, maxSize, bufferManager);
        }

        private void AllocNextChunk(int minimumChunkSize)
        {
            int num;
            if (this.currentChunk.Length > 0x3fffffff)
            {
                num = 0x7fffffff;
            }
            else
            {
                num = this.currentChunk.Length * 2;
            }
            if (minimumChunkSize > num)
            {
                num = minimumChunkSize;
            }
            byte[] buffer = this.bufferManager.TakeBuffer(num);
            if (this.chunkCount == this.chunks.Length)
            {
                byte[][] destinationArray = new byte[this.chunks.Length * 2][];
                Array.Copy(this.chunks, destinationArray, this.chunks.Length);
                this.chunks = destinationArray;
            }
            this.chunks[this.chunkCount++] = buffer;
            this.currentChunk = buffer;
            this.currentChunkSize = 0;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            throw Fx.Exception.AsError(new NotSupportedException(SRCore.ReadNotSupported));
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            this.Write(buffer, offset, size);
            return new CompletedAsyncResult(callback, state);
        }

        public void Clear()
        {
            if (!this.callerReturnsBuffer)
            {
                for (int i = 0; i < this.chunkCount; i++)
                {
                    this.bufferManager.ReturnBuffer(this.chunks[i]);
                    this.chunks[i] = null;
                }
            }
            this.callerReturnsBuffer = false;
            this.initialized = false;
            this.bufferReturned = false;
            this.chunkCount = 0;
            this.currentChunk = null;
        }

        public override void Close()
        {
        }

        protected virtual Exception CreateQuotaExceededException(int maxSizeQuota)
        {
            return new InvalidOperationException(SRCore.BufferedOutputStreamQuotaExceeded(maxSizeQuota));
        }

        public override int EndRead(IAsyncResult result)
        {
            throw Fx.Exception.AsError(new NotSupportedException(SRCore.ReadNotSupported));
        }

        public override void EndWrite(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            throw Fx.Exception.AsError(new NotSupportedException(SRCore.ReadNotSupported));
        }

        public override int ReadByte()
        {
            throw Fx.Exception.AsError(new NotSupportedException(SRCore.ReadNotSupported));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Reinitialize(int initialSize, int maxSizeQuota, InternalBufferManager bufferManager)
        {
            this.Reinitialize(initialSize, maxSizeQuota, maxSizeQuota, bufferManager);
        }

        public void Reinitialize(int initialSize, int maxSizeQuota, int effectiveMaxSize, InternalBufferManager bufferManager)
        {
            this.maxSizeQuota = maxSizeQuota;
            this.maxSize = effectiveMaxSize;
            this.bufferManager = bufferManager;
            this.currentChunk = bufferManager.TakeBuffer(initialSize);
            this.currentChunkSize = 0;
            this.totalSize = 0;
            this.chunkCount = 1;
            this.chunks[0] = this.currentChunk;
            this.initialized = true;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw Fx.Exception.AsError(new NotSupportedException(SRCore.SeekNotSupported));
        }

        public override void SetLength(long value)
        {
            throw Fx.Exception.AsError(new NotSupportedException(SRCore.SeekNotSupported));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Skip(int size)
        {
            this.WriteCore(null, 0, size);
        }

        public byte[] ToArray(out int bufferSize)
        {
            byte[] currentChunk;
            if (this.chunkCount == 1)
            {
                currentChunk = this.currentChunk;
                bufferSize = this.currentChunkSize;
                this.callerReturnsBuffer = true;
            }
            else
            {
                currentChunk = this.bufferManager.TakeBuffer(this.totalSize);
                int dstOffset = 0;
                int num2 = this.chunkCount - 1;
                for (int i = 0; i < num2; i++)
                {
                    byte[] src = this.chunks[i];
                    Buffer.BlockCopy(src, 0, currentChunk, dstOffset, src.Length);
                    dstOffset += src.Length;
                }
                Buffer.BlockCopy(this.currentChunk, 0, currentChunk, dstOffset, this.currentChunkSize);
                bufferSize = this.totalSize;
            }
            this.bufferReturned = true;
            return currentChunk;
        }

        public MemoryStream ToMemoryStream()
        {
            int num;
            return new MemoryStream(this.ToArray(out num), 0, num);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override void Write(byte[] buffer, int offset, int size)
        {
            this.WriteCore(buffer, offset, size);
        }

        public override void WriteByte(byte value)
        {
            if (this.totalSize == this.maxSize)
            {
                throw Fx.Exception.AsError(this.CreateQuotaExceededException(this.maxSize));
            }
            if (this.currentChunkSize == this.currentChunk.Length)
            {
                this.AllocNextChunk(1);
            }
            this.currentChunk[this.currentChunkSize++] = value;
        }

        private void WriteCore(byte[] buffer, int offset, int size)
        {
            if (size < 0)
            {
                throw Fx.Exception.ArgumentOutOfRange("size", size, SRCore.ValueMustBeNonNegative);
            }
            if ((0x7fffffff - size) < this.totalSize)
            {
                throw Fx.Exception.AsError(this.CreateQuotaExceededException(this.maxSizeQuota));
            }
            int num = this.totalSize + size;
            if (num > this.maxSize)
            {
                throw Fx.Exception.AsError(this.CreateQuotaExceededException(this.maxSizeQuota));
            }
            int count = this.currentChunk.Length - this.currentChunkSize;
            if (size > count)
            {
                if (count > 0)
                {
                    if (buffer != null)
                    {
                        Buffer.BlockCopy(buffer, offset, this.currentChunk, this.currentChunkSize, count);
                    }
                    this.currentChunkSize = this.currentChunk.Length;
                    offset += count;
                    size -= count;
                }
                this.AllocNextChunk(size);
            }
            if (buffer != null)
            {
                Buffer.BlockCopy(buffer, offset, this.currentChunk, this.currentChunkSize, size);
            }
            this.totalSize = num;
            this.currentChunkSize += size;
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
                return (long) this.totalSize;
            }
        }

        public override long Position
        {
            get
            {
                throw Fx.Exception.AsError(new NotSupportedException(SRCore.SeekNotSupported));
            }
            set
            {
                throw Fx.Exception.AsError(new NotSupportedException(SRCore.SeekNotSupported));
            }
        }
    }
}

