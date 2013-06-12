namespace System.Net
{
    using System;
    using System.IO;

    internal class BufferedReadStream : DelegatedStream
    {
        private bool readMore;
        private byte[] storedBuffer;
        private int storedLength;
        private int storedOffset;

        internal BufferedReadStream(Stream stream) : this(stream, false)
        {
        }

        internal BufferedReadStream(Stream stream, bool readMore) : base(stream)
        {
            this.readMore = readMore;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ReadAsyncResult result = new ReadAsyncResult(this, callback, state);
            result.Read(buffer, offset, count);
            return result;
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return ReadAsyncResult.End(asyncResult);
        }

        internal void Push(byte[] buffer, int offset, int count)
        {
            if (count != 0)
            {
                if (this.storedOffset == this.storedLength)
                {
                    if ((this.storedBuffer == null) || (this.storedBuffer.Length < count))
                    {
                        this.storedBuffer = new byte[count];
                    }
                    this.storedOffset = 0;
                    this.storedLength = count;
                }
                else if (count <= this.storedOffset)
                {
                    this.storedOffset -= count;
                }
                else if (count <= ((this.storedBuffer.Length - this.storedLength) + this.storedOffset))
                {
                    Buffer.BlockCopy(this.storedBuffer, this.storedOffset, this.storedBuffer, count, this.storedLength - this.storedOffset);
                    this.storedLength += count - this.storedOffset;
                    this.storedOffset = 0;
                }
                else
                {
                    byte[] dst = new byte[(count + this.storedLength) - this.storedOffset];
                    Buffer.BlockCopy(this.storedBuffer, this.storedOffset, dst, count, this.storedLength - this.storedOffset);
                    this.storedLength += count - this.storedOffset;
                    this.storedOffset = 0;
                    this.storedBuffer = dst;
                }
                Buffer.BlockCopy(buffer, offset, this.storedBuffer, this.storedOffset, count);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = 0;
            if (this.storedOffset < this.storedLength)
            {
                num = Math.Min(count, this.storedLength - this.storedOffset);
                Buffer.BlockCopy(this.storedBuffer, this.storedOffset, buffer, offset, num);
                this.storedOffset += num;
                if ((num == count) || !this.readMore)
                {
                    return num;
                }
                offset += num;
                count -= num;
            }
            return (num + base.Read(buffer, offset, count));
        }

        public override int ReadByte()
        {
            if (this.storedOffset < this.storedLength)
            {
                return this.storedBuffer[this.storedOffset++];
            }
            return base.ReadByte();
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

        private class ReadAsyncResult : LazyAsyncResult
        {
            private static AsyncCallback onRead = new AsyncCallback(BufferedReadStream.ReadAsyncResult.OnRead);
            private BufferedReadStream parent;
            private int read;

            internal ReadAsyncResult(BufferedReadStream parent, AsyncCallback callback, object state) : base(null, state, callback)
            {
                this.parent = parent;
            }

            internal static int End(IAsyncResult result)
            {
                BufferedReadStream.ReadAsyncResult result2 = (BufferedReadStream.ReadAsyncResult) result;
                result2.InternalWaitForCompletion();
                return result2.read;
            }

            private static void OnRead(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    BufferedReadStream.ReadAsyncResult asyncState = (BufferedReadStream.ReadAsyncResult) result.AsyncState;
                    try
                    {
                        asyncState.read += asyncState.parent.BaseStream.EndRead(result);
                        asyncState.InvokeCallback();
                    }
                    catch (Exception exception)
                    {
                        if (asyncState.IsCompleted)
                        {
                            throw;
                        }
                        asyncState.InvokeCallback(exception);
                    }
                }
            }

            internal void Read(byte[] buffer, int offset, int count)
            {
                if (this.parent.storedOffset < this.parent.storedLength)
                {
                    this.read = Math.Min(count, this.parent.storedLength - this.parent.storedOffset);
                    Buffer.BlockCopy(this.parent.storedBuffer, this.parent.storedOffset, buffer, offset, this.read);
                    this.parent.storedOffset += this.read;
                    if ((this.read == count) || !this.parent.readMore)
                    {
                        base.InvokeCallback();
                        return;
                    }
                    count -= this.read;
                    offset += this.read;
                }
                IAsyncResult asyncResult = this.parent.BaseStream.BeginRead(buffer, offset, count, onRead, this);
                if (asyncResult.CompletedSynchronously)
                {
                    this.read += this.parent.BaseStream.EndRead(asyncResult);
                    base.InvokeCallback();
                }
            }
        }
    }
}

