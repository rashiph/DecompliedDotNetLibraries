namespace System.Net.Mime
{
    using System;
    using System.IO;
    using System.Net;

    internal class SevenBitStream : DelegatedStream, IEncodableStream
    {
        internal SevenBitStream(Stream stream) : base(stream)
        {
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            this.CheckBytes(buffer, offset, count);
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        private void CheckBytes(byte[] buffer, int offset, int count)
        {
            for (int i = count; i < (offset + count); i++)
            {
                if (buffer[i] > 0x7f)
                {
                    throw new FormatException(SR.GetString("Mail7BitStreamInvalidCharacter"));
                }
            }
        }

        public int DecodeBytes(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public int EncodeBytes(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public string GetEncodedString()
        {
            throw new NotImplementedException();
        }

        public Stream GetStream()
        {
            return this;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            this.CheckBytes(buffer, offset, count);
            base.Write(buffer, offset, count);
        }
    }
}

