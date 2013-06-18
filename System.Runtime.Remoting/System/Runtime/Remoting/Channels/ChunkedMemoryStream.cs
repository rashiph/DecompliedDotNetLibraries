namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.IO;
    using System.Runtime.Remoting;

    internal class ChunkedMemoryStream : Stream
    {
        private bool _bClosed;
        private IByteBufferPool _bufferPool;
        private MemoryChunk _chunks;
        private MemoryChunk _readChunk;
        private int _readOffset;
        private MemoryChunk _writeChunk;
        private int _writeOffset;
        private static IByteBufferPool s_defaultBufferPool = new ByteBufferAllocator(0x400);

        public ChunkedMemoryStream(IByteBufferPool bufferPool)
        {
            this._bufferPool = bufferPool;
        }

        private MemoryChunk AllocateMemoryChunk()
        {
            return new MemoryChunk { Buffer = this._bufferPool.GetBuffer(), Next = null };
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                this._bClosed = true;
                if (disposing)
                {
                    this.ReleaseMemoryChunks(this._chunks);
                }
                this._chunks = null;
                this._writeChunk = null;
                this._readChunk = null;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this._bClosed)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Stream_StreamIsClosed"));
            }
            if (this._readChunk == null)
            {
                if (this._chunks == null)
                {
                    return 0;
                }
                this._readChunk = this._chunks;
                this._readOffset = 0;
            }
            byte[] src = this._readChunk.Buffer;
            int length = src.Length;
            if (this._readChunk.Next == null)
            {
                length = this._writeOffset;
            }
            int num2 = 0;
            while (count > 0)
            {
                if (this._readOffset == length)
                {
                    if (this._readChunk.Next == null)
                    {
                        return num2;
                    }
                    this._readChunk = this._readChunk.Next;
                    this._readOffset = 0;
                    src = this._readChunk.Buffer;
                    length = src.Length;
                    if (this._readChunk.Next == null)
                    {
                        length = this._writeOffset;
                    }
                }
                int num3 = Math.Min(count, length - this._readOffset);
                Buffer.BlockCopy(src, this._readOffset, buffer, offset, num3);
                offset += num3;
                count -= num3;
                this._readOffset += num3;
                num2 += num3;
            }
            return num2;
        }

        public override int ReadByte()
        {
            if (this._bClosed)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Stream_StreamIsClosed"));
            }
            if (this._readChunk == null)
            {
                if (this._chunks == null)
                {
                    return 0;
                }
                this._readChunk = this._chunks;
                this._readOffset = 0;
            }
            byte[] buffer = this._readChunk.Buffer;
            int length = buffer.Length;
            if (this._readChunk.Next == null)
            {
                length = this._writeOffset;
            }
            if (this._readOffset == length)
            {
                if (this._readChunk.Next == null)
                {
                    return -1;
                }
                this._readChunk = this._readChunk.Next;
                this._readOffset = 0;
                buffer = this._readChunk.Buffer;
                length = buffer.Length;
                if (this._readChunk.Next == null)
                {
                    length = this._writeOffset;
                }
            }
            return buffer[this._readOffset++];
        }

        private void ReleaseMemoryChunks(MemoryChunk chunk)
        {
            if (!(this._bufferPool is ByteBufferAllocator))
            {
                while (chunk != null)
                {
                    this._bufferPool.ReturnBuffer(chunk.Buffer);
                    chunk = chunk.Next;
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (this._bClosed)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Stream_StreamIsClosed"));
            }
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.Position = offset;
                    break;

                case SeekOrigin.Current:
                    this.Position += offset;
                    break;

                case SeekOrigin.End:
                    this.Position = this.Length + offset;
                    break;
            }
            return this.Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public virtual byte[] ToArray()
        {
            int length = (int) this.Length;
            byte[] buffer = new byte[this.Length];
            MemoryChunk chunk = this._readChunk;
            int num2 = this._readOffset;
            this._readChunk = this._chunks;
            this._readOffset = 0;
            this.Read(buffer, 0, length);
            this._readChunk = chunk;
            this._readOffset = num2;
            return buffer;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this._bClosed)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Stream_StreamIsClosed"));
            }
            if (this._chunks == null)
            {
                this._chunks = this.AllocateMemoryChunk();
                this._writeChunk = this._chunks;
                this._writeOffset = 0;
            }
            byte[] dst = this._writeChunk.Buffer;
            int length = dst.Length;
            while (count > 0)
            {
                if (this._writeOffset == length)
                {
                    this._writeChunk.Next = this.AllocateMemoryChunk();
                    this._writeChunk = this._writeChunk.Next;
                    this._writeOffset = 0;
                    dst = this._writeChunk.Buffer;
                    length = dst.Length;
                }
                int num2 = Math.Min(count, length - this._writeOffset);
                Buffer.BlockCopy(buffer, offset, dst, this._writeOffset, num2);
                offset += num2;
                count -= num2;
                this._writeOffset += num2;
            }
        }

        public override void WriteByte(byte value)
        {
            if (this._bClosed)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Stream_StreamIsClosed"));
            }
            if (this._chunks == null)
            {
                this._chunks = this.AllocateMemoryChunk();
                this._writeChunk = this._chunks;
                this._writeOffset = 0;
            }
            byte[] buffer = this._writeChunk.Buffer;
            int length = buffer.Length;
            if (this._writeOffset == length)
            {
                this._writeChunk.Next = this.AllocateMemoryChunk();
                this._writeChunk = this._writeChunk.Next;
                this._writeOffset = 0;
                buffer = this._writeChunk.Buffer;
                length = buffer.Length;
            }
            buffer[this._writeOffset++] = value;
        }

        public virtual void WriteTo(Stream stream)
        {
            if (this._bClosed)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Stream_StreamIsClosed"));
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (this._readChunk == null)
            {
                if (this._chunks == null)
                {
                    return;
                }
                this._readChunk = this._chunks;
                this._readOffset = 0;
            }
            byte[] buffer = this._readChunk.Buffer;
            int length = buffer.Length;
            if (this._readChunk.Next == null)
            {
                length = this._writeOffset;
            }
            while (true)
            {
                if (this._readOffset == length)
                {
                    if (this._readChunk.Next == null)
                    {
                        return;
                    }
                    this._readChunk = this._readChunk.Next;
                    this._readOffset = 0;
                    buffer = this._readChunk.Buffer;
                    length = buffer.Length;
                    if (this._readChunk.Next == null)
                    {
                        length = this._writeOffset;
                    }
                }
                int count = length - this._readOffset;
                stream.Write(buffer, this._readOffset, count);
                this._readOffset = length;
            }
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
                MemoryChunk next;
                if (this._bClosed)
                {
                    throw new RemotingException(CoreChannel.GetResourceString("Remoting_Stream_StreamIsClosed"));
                }
                int num = 0;
                for (MemoryChunk chunk = this._chunks; chunk != null; chunk = next)
                {
                    next = chunk.Next;
                    if (next != null)
                    {
                        num += chunk.Buffer.Length;
                    }
                    else
                    {
                        num += this._writeOffset;
                    }
                }
                return (long) num;
            }
        }

        public override long Position
        {
            get
            {
                if (this._bClosed)
                {
                    throw new RemotingException(CoreChannel.GetResourceString("Remoting_Stream_StreamIsClosed"));
                }
                if (this._readChunk == null)
                {
                    return 0L;
                }
                int num = 0;
                for (MemoryChunk chunk = this._chunks; chunk != this._readChunk; chunk = chunk.Next)
                {
                    num += chunk.Buffer.Length;
                }
                num += this._readOffset;
                return (long) num;
            }
            set
            {
                if (this._bClosed)
                {
                    throw new RemotingException(CoreChannel.GetResourceString("Remoting_Stream_StreamIsClosed"));
                }
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                MemoryChunk chunk = this._readChunk;
                int num = this._readOffset;
                this._readChunk = null;
                this._readOffset = 0;
                int num2 = (int) value;
                for (MemoryChunk chunk2 = this._chunks; chunk2 != null; chunk2 = chunk2.Next)
                {
                    if ((num2 < chunk2.Buffer.Length) || ((num2 == chunk2.Buffer.Length) && (chunk2.Next == null)))
                    {
                        this._readChunk = chunk2;
                        this._readOffset = num2;
                        break;
                    }
                    num2 -= chunk2.Buffer.Length;
                }
                if (this._readChunk == null)
                {
                    this._readChunk = chunk;
                    this._readOffset = num;
                    throw new ArgumentOutOfRangeException("value");
                }
            }
        }

        private class MemoryChunk
        {
            public byte[] Buffer;
            public ChunkedMemoryStream.MemoryChunk Next;
        }
    }
}

