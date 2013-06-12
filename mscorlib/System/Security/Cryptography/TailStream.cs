namespace System.Security.Cryptography
{
    using System;
    using System.IO;
    using System.Security;

    internal sealed class TailStream : Stream
    {
        private byte[] _Buffer;
        private bool _BufferFull;
        private int _BufferIndex;
        private int _BufferSize;

        public TailStream(int bufferSize)
        {
            this._Buffer = new byte[bufferSize];
            this._BufferSize = bufferSize;
        }

        public void Clear()
        {
            this.Close();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (this._Buffer != null)
                    {
                        Array.Clear(this._Buffer, 0, this._Buffer.Length);
                    }
                    this._Buffer = null;
                }
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
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnreadableStream"));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnseekableStream"));
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnseekableStream"));
        }

        [SecuritySafeCritical]
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this._Buffer == null)
            {
                throw new ObjectDisposedException("TailStream");
            }
            if (count != 0)
            {
                if (this._BufferFull)
                {
                    if (count > this._BufferSize)
                    {
                        System.Buffer.InternalBlockCopy(buffer, (offset + count) - this._BufferSize, this._Buffer, 0, this._BufferSize);
                    }
                    else
                    {
                        System.Buffer.InternalBlockCopy(this._Buffer, this._BufferSize - count, this._Buffer, 0, this._BufferSize - count);
                        System.Buffer.InternalBlockCopy(buffer, offset, this._Buffer, this._BufferSize - count, count);
                    }
                }
                else if (count > this._BufferSize)
                {
                    System.Buffer.InternalBlockCopy(buffer, (offset + count) - this._BufferSize, this._Buffer, 0, this._BufferSize);
                    this._BufferFull = true;
                }
                else if ((count + this._BufferIndex) >= this._BufferSize)
                {
                    System.Buffer.InternalBlockCopy(this._Buffer, (this._BufferIndex + count) - this._BufferSize, this._Buffer, 0, this._BufferSize - count);
                    System.Buffer.InternalBlockCopy(buffer, offset, this._Buffer, this._BufferIndex, count);
                    this._BufferFull = true;
                }
                else
                {
                    System.Buffer.InternalBlockCopy(buffer, offset, this._Buffer, this._BufferIndex, count);
                    this._BufferIndex += count;
                }
            }
        }

        public byte[] Buffer
        {
            get
            {
                return (byte[]) this._Buffer.Clone();
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
                return (this._Buffer != null);
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnseekableStream"));
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnseekableStream"));
            }
            set
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnseekableStream"));
            }
        }
    }
}

