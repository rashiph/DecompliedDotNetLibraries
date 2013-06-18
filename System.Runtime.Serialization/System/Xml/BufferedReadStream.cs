namespace System.Xml
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;

    internal class BufferedReadStream : Stream
    {
        private bool readMore;
        private byte[] storedBuffer;
        private int storedLength;
        private int storedOffset;
        private Stream stream;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public BufferedReadStream(Stream stream) : this(stream, false)
        {
        }

        public BufferedReadStream(Stream stream, bool readMore)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            this.stream = stream;
            this.readMore = readMore;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!this.CanRead)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("ReadNotSupportedOnStream", new object[] { this.stream.GetType().FullName })));
            }
            return this.stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("WriteNotSupportedOnStream", new object[] { this.stream.GetType().FullName })));
        }

        public override void Close()
        {
            this.stream.Close();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (!this.CanRead)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("ReadNotSupportedOnStream", new object[] { this.stream.GetType().FullName })));
            }
            return this.stream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("WriteNotSupportedOnStream", new object[] { this.stream.GetType().FullName })));
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public void Push(byte[] buffer, int offset, int count)
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
            if (!this.CanRead)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("ReadNotSupportedOnStream", new object[] { this.stream.GetType().FullName })));
            }
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > buffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { buffer.Length })));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (buffer.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { buffer.Length - offset })));
            }
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
            return (num + this.stream.Read(buffer, offset, count));
        }

        public int ReadBlock(byte[] buffer, int offset, int count)
        {
            int num;
            int num2 = 0;
            while ((num2 < count) && ((num = this.Read(buffer, offset + num2, count - num2)) != 0))
            {
                num2 += num;
            }
            return num2;
        }

        public override int ReadByte()
        {
            if (this.storedOffset < this.storedLength)
            {
                return this.storedBuffer[this.storedOffset++];
            }
            return base.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("SeekNotSupportedOnStream", new object[] { this.stream.GetType().FullName })));
        }

        public override void SetLength(long value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("SeekNotSupportedOnStream", new object[] { this.stream.GetType().FullName })));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("WriteNotSupportedOnStream", new object[] { this.stream.GetType().FullName })));
        }

        public override bool CanRead
        {
            get
            {
                return this.stream.CanRead;
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("SeekNotSupportedOnStream", new object[] { this.stream.GetType().FullName })));
            }
        }

        public override long Position
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("SeekNotSupportedOnStream", new object[] { this.stream.GetType().FullName })));
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("SeekNotSupportedOnStream", new object[] { this.stream.GetType().FullName })));
            }
        }
    }
}

