namespace System.Security.Cryptography
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class CryptoStream : Stream, IDisposable
    {
        private bool _canRead;
        private bool _canWrite;
        private bool _finalBlockTransformed;
        private int _InputBlockSize;
        private byte[] _InputBuffer;
        private int _InputBufferIndex;
        private int _OutputBlockSize;
        private byte[] _OutputBuffer;
        private int _OutputBufferIndex;
        private Stream _stream;
        private ICryptoTransform _Transform;
        private CryptoStreamMode _transformMode;

        public CryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode)
        {
            this._stream = stream;
            this._transformMode = mode;
            this._Transform = transform;
            switch (this._transformMode)
            {
                case CryptoStreamMode.Read:
                    if (!this._stream.CanRead)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotReadable"), "stream");
                    }
                    this._canRead = true;
                    break;

                case CryptoStreamMode.Write:
                    if (!this._stream.CanWrite)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotWritable"), "stream");
                    }
                    this._canWrite = true;
                    break;

                default:
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
            }
            this.InitializeBuffer();
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
                    if (!this._finalBlockTransformed)
                    {
                        this.FlushFinalBlock();
                    }
                    this._stream.Close();
                }
                if (this._InputBuffer != null)
                {
                    Array.Clear(this._InputBuffer, 0, this._InputBuffer.Length);
                }
                if (this._OutputBuffer != null)
                {
                    Array.Clear(this._OutputBuffer, 0, this._OutputBuffer.Length);
                }
                this._InputBuffer = null;
                this._OutputBuffer = null;
                this._canRead = false;
                this._canWrite = false;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
        }

        public void FlushFinalBlock()
        {
            if (this._finalBlockTransformed)
            {
                throw new NotSupportedException(Environment.GetResourceString("Cryptography_CryptoStream_FlushFinalBlockTwice"));
            }
            byte[] buffer = this._Transform.TransformFinalBlock(this._InputBuffer, 0, this._InputBufferIndex);
            this._finalBlockTransformed = true;
            if (this._canWrite && (this._OutputBufferIndex > 0))
            {
                this._stream.Write(this._OutputBuffer, 0, this._OutputBufferIndex);
                this._OutputBufferIndex = 0;
            }
            if (this._canWrite)
            {
                this._stream.Write(buffer, 0, buffer.Length);
            }
            CryptoStream stream = this._stream as CryptoStream;
            if (stream != null)
            {
                if (!stream.HasFlushedFinalBlock)
                {
                    stream.FlushFinalBlock();
                }
            }
            else
            {
                this._stream.Flush();
            }
            if (this._InputBuffer != null)
            {
                Array.Clear(this._InputBuffer, 0, this._InputBuffer.Length);
            }
            if (this._OutputBuffer != null)
            {
                Array.Clear(this._OutputBuffer, 0, this._OutputBuffer.Length);
            }
        }

        private void InitializeBuffer()
        {
            if (this._Transform != null)
            {
                this._InputBlockSize = this._Transform.InputBlockSize;
                this._InputBuffer = new byte[this._InputBlockSize];
                this._OutputBlockSize = this._Transform.OutputBlockSize;
                this._OutputBuffer = new byte[this._OutputBlockSize];
            }
        }

        [SecuritySafeCritical]
        public override int Read([In, Out] byte[] buffer, int offset, int count)
        {
            int num4;
            if (!this.CanRead)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnreadableStream"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            int byteCount = count;
            int dstOffsetBytes = offset;
            if (this._OutputBufferIndex != 0)
            {
                if (this._OutputBufferIndex > count)
                {
                    Buffer.InternalBlockCopy(this._OutputBuffer, 0, buffer, offset, count);
                    Buffer.InternalBlockCopy(this._OutputBuffer, count, this._OutputBuffer, 0, this._OutputBufferIndex - count);
                    this._OutputBufferIndex -= count;
                    return count;
                }
                Buffer.InternalBlockCopy(this._OutputBuffer, 0, buffer, offset, this._OutputBufferIndex);
                byteCount -= this._OutputBufferIndex;
                dstOffsetBytes += this._OutputBufferIndex;
                this._OutputBufferIndex = 0;
            }
            if (this._finalBlockTransformed)
            {
                return (count - byteCount);
            }
            int num3 = 0;
            if ((byteCount > this._OutputBlockSize) && this._Transform.CanTransformMultipleBlocks)
            {
                int num5 = byteCount / this._OutputBlockSize;
                int num6 = num5 * this._InputBlockSize;
                byte[] dst = new byte[num6];
                Buffer.InternalBlockCopy(this._InputBuffer, 0, dst, 0, this._InputBufferIndex);
                num3 = this._InputBufferIndex + this._stream.Read(dst, this._InputBufferIndex, num6 - this._InputBufferIndex);
                this._InputBufferIndex = 0;
                if (num3 <= this._InputBlockSize)
                {
                    this._InputBuffer = dst;
                    this._InputBufferIndex = num3;
                }
                else
                {
                    int srcOffsetBytes = (num3 / this._InputBlockSize) * this._InputBlockSize;
                    int num8 = num3 - srcOffsetBytes;
                    if (num8 != 0)
                    {
                        this._InputBufferIndex = num8;
                        Buffer.InternalBlockCopy(dst, srcOffsetBytes, this._InputBuffer, 0, num8);
                    }
                    byte[] outputBuffer = new byte[(srcOffsetBytes / this._InputBlockSize) * this._OutputBlockSize];
                    num4 = this._Transform.TransformBlock(dst, 0, srcOffsetBytes, outputBuffer, 0);
                    Buffer.InternalBlockCopy(outputBuffer, 0, buffer, dstOffsetBytes, num4);
                    Array.Clear(dst, 0, dst.Length);
                    Array.Clear(outputBuffer, 0, outputBuffer.Length);
                    byteCount -= num4;
                    dstOffsetBytes += num4;
                }
            }
            while (byteCount > 0)
            {
                while (this._InputBufferIndex < this._InputBlockSize)
                {
                    num3 = this._stream.Read(this._InputBuffer, this._InputBufferIndex, this._InputBlockSize - this._InputBufferIndex);
                    if (num3 == 0)
                    {
                        byte[] buffer4 = this._Transform.TransformFinalBlock(this._InputBuffer, 0, this._InputBufferIndex);
                        this._OutputBuffer = buffer4;
                        this._OutputBufferIndex = buffer4.Length;
                        this._finalBlockTransformed = true;
                        if (byteCount < this._OutputBufferIndex)
                        {
                            Buffer.InternalBlockCopy(this._OutputBuffer, 0, buffer, dstOffsetBytes, byteCount);
                            this._OutputBufferIndex -= byteCount;
                            Buffer.InternalBlockCopy(this._OutputBuffer, byteCount, this._OutputBuffer, 0, this._OutputBufferIndex);
                            return count;
                        }
                        Buffer.InternalBlockCopy(this._OutputBuffer, 0, buffer, dstOffsetBytes, this._OutputBufferIndex);
                        byteCount -= this._OutputBufferIndex;
                        this._OutputBufferIndex = 0;
                        return (count - byteCount);
                    }
                    this._InputBufferIndex += num3;
                }
                num4 = this._Transform.TransformBlock(this._InputBuffer, 0, this._InputBlockSize, this._OutputBuffer, 0);
                this._InputBufferIndex = 0;
                if (byteCount >= num4)
                {
                    Buffer.InternalBlockCopy(this._OutputBuffer, 0, buffer, dstOffsetBytes, num4);
                    dstOffsetBytes += num4;
                    byteCount -= num4;
                }
                else
                {
                    Buffer.InternalBlockCopy(this._OutputBuffer, 0, buffer, dstOffsetBytes, byteCount);
                    this._OutputBufferIndex = num4 - byteCount;
                    Buffer.InternalBlockCopy(this._OutputBuffer, byteCount, this._OutputBuffer, 0, this._OutputBufferIndex);
                    return count;
                }
            }
            return count;
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
            int num3;
            if (!this.CanWrite)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnwritableStream"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            int byteCount = count;
            int inputOffset = offset;
            if (this._InputBufferIndex > 0)
            {
                if (count < (this._InputBlockSize - this._InputBufferIndex))
                {
                    Buffer.InternalBlockCopy(buffer, offset, this._InputBuffer, this._InputBufferIndex, count);
                    this._InputBufferIndex += count;
                    return;
                }
                Buffer.InternalBlockCopy(buffer, offset, this._InputBuffer, this._InputBufferIndex, this._InputBlockSize - this._InputBufferIndex);
                inputOffset += this._InputBlockSize - this._InputBufferIndex;
                byteCount -= this._InputBlockSize - this._InputBufferIndex;
                this._InputBufferIndex = this._InputBlockSize;
            }
            if (this._OutputBufferIndex > 0)
            {
                this._stream.Write(this._OutputBuffer, 0, this._OutputBufferIndex);
                this._OutputBufferIndex = 0;
            }
            if (this._InputBufferIndex == this._InputBlockSize)
            {
                num3 = this._Transform.TransformBlock(this._InputBuffer, 0, this._InputBlockSize, this._OutputBuffer, 0);
                this._stream.Write(this._OutputBuffer, 0, num3);
                this._InputBufferIndex = 0;
            }
            while (byteCount > 0)
            {
                if (byteCount >= this._InputBlockSize)
                {
                    if (this._Transform.CanTransformMultipleBlocks)
                    {
                        int num4 = byteCount / this._InputBlockSize;
                        int inputCount = num4 * this._InputBlockSize;
                        byte[] outputBuffer = new byte[num4 * this._OutputBlockSize];
                        num3 = this._Transform.TransformBlock(buffer, inputOffset, inputCount, outputBuffer, 0);
                        this._stream.Write(outputBuffer, 0, num3);
                        inputOffset += inputCount;
                        byteCount -= inputCount;
                    }
                    else
                    {
                        num3 = this._Transform.TransformBlock(buffer, inputOffset, this._InputBlockSize, this._OutputBuffer, 0);
                        this._stream.Write(this._OutputBuffer, 0, num3);
                        inputOffset += this._InputBlockSize;
                        byteCount -= this._InputBlockSize;
                    }
                }
                else
                {
                    Buffer.InternalBlockCopy(buffer, inputOffset, this._InputBuffer, 0, byteCount);
                    this._InputBufferIndex += byteCount;
                    return;
                }
            }
        }

        public override bool CanRead
        {
            get
            {
                return this._canRead;
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
                return this._canWrite;
            }
        }

        public bool HasFlushedFinalBlock
        {
            get
            {
                return this._finalBlockTransformed;
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

