namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.IO;
    using System.Runtime.Remoting.Channels;
    using System.Text;

    internal sealed class HttpChunkedResponseStream : HttpServerResponseStream
    {
        private byte[] _byteBuffer = new byte[1];
        private byte[] _chunk;
        private int _chunkOffset;
        private int _chunkSize;
        private static byte[] _endChunk = Encoding.ASCII.GetBytes("\r\n");
        private Stream _outputStream;
        private static byte[] _trailer = Encoding.ASCII.GetBytes("0\r\n\r\n");

        internal HttpChunkedResponseStream(Stream outputStream)
        {
            this._outputStream = outputStream;
            this._chunk = CoreChannel.BufferPool.GetBuffer();
            this._chunkSize = this._chunk.Length - 2;
            this._chunkOffset = 0;
            this._chunk[this._chunkSize - 2] = 13;
            this._chunk[this._chunkSize - 1] = 10;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (this._chunkOffset > 0)
                    {
                        this.FlushChunk();
                    }
                    this._outputStream.Write(_trailer, 0, _trailer.Length);
                    this._outputStream.Flush();
                }
                CoreChannel.BufferPool.ReturnBuffer(this._chunk);
                this._chunk = null;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
            if (this._chunkOffset > 0)
            {
                this.FlushChunk();
            }
            this._outputStream.Flush();
        }

        private void FlushChunk()
        {
            this.WriteChunk(this._chunk, 0, this._chunkOffset);
            this._chunkOffset = 0;
        }

        private byte[] IntToHexChars(int i)
        {
            string s = "";
            while (i > 0)
            {
                int num = i % 0x10;
                switch (num)
                {
                    case 10:
                        s = 'A' + s;
                        break;

                    case 11:
                        s = 'B' + s;
                        break;

                    case 12:
                        s = 'C' + s;
                        break;

                    case 13:
                        s = 'D' + s;
                        break;

                    case 14:
                        s = 'E' + s;
                        break;

                    case 15:
                        s = 'F' + s;
                        break;

                    default:
                        s = ((char) (num + 0x30)) + s;
                        break;
                }
                i /= 0x10;
            }
            s = s + "\r\n";
            return Encoding.ASCII.GetBytes(s);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                if ((this._chunkOffset == 0) && (count >= this._chunkSize))
                {
                    this.WriteChunk(buffer, offset, count);
                    return;
                }
                int length = Math.Min(this._chunkSize - this._chunkOffset, count);
                Array.Copy(buffer, offset, this._chunk, this._chunkOffset, length);
                this._chunkOffset += length;
                count -= length;
                offset += length;
                if (this._chunkOffset == this._chunkSize)
                {
                    this.FlushChunk();
                }
            }
        }

        public override void WriteByte(byte value)
        {
            this._byteBuffer[0] = value;
            this.Write(this._byteBuffer, 0, 1);
        }

        private void WriteChunk(byte[] buffer, int offset, int count)
        {
            byte[] buffer2 = this.IntToHexChars(count);
            this._outputStream.Write(buffer2, 0, buffer2.Length);
            if (buffer == this._chunk)
            {
                this._outputStream.Write(this._chunk, offset, count + 2);
            }
            else
            {
                this._outputStream.Write(buffer, offset, count);
                this._outputStream.Write(_endChunk, 0, _endChunk.Length);
            }
        }
    }
}

