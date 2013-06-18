namespace System.Web
{
    using System;
    using System.IO;

    internal class HttpInputStream : Stream
    {
        private HttpRawUploadedContent _data;
        private int _length;
        private int _offset;
        private int _pos;

        internal HttpInputStream(HttpRawUploadedContent data, int offset, int length)
        {
            this.Init(data, offset, length);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.Uninit();
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

        internal byte[] GetAsByteArray()
        {
            if (this._length == 0)
            {
                return null;
            }
            return this._data.GetAsByteArray(this._offset, this._length);
        }

        protected void Init(HttpRawUploadedContent data, int offset, int length)
        {
            this._data = data;
            this._offset = offset;
            this._length = length;
            this._pos = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int length = this._length - this._pos;
            if (count < length)
            {
                length = count;
            }
            if (length > 0)
            {
                this._data.CopyBytes(this._offset + this._pos, buffer, offset, length);
            }
            this._pos += length;
            return length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            int num = this._pos;
            int num2 = (int) offset;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    num = num2;
                    break;

                case SeekOrigin.Current:
                    num = this._pos + num2;
                    break;

                case SeekOrigin.End:
                    num = this._length + num2;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("origin");
            }
            if ((num < 0) || (num > this._length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            this._pos = num;
            return (long) this._pos;
        }

        public override void SetLength(long length)
        {
            throw new NotSupportedException();
        }

        protected void Uninit()
        {
            this._data = null;
            this._offset = 0;
            this._length = 0;
            this._pos = 0;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        internal void WriteTo(Stream s)
        {
            if ((this._data != null) && (this._length > 0))
            {
                this._data.WriteBytes(this._offset, this._length, s);
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
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return (long) this._length;
            }
        }

        public override long Position
        {
            get
            {
                return (long) this._pos;
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }
        }
    }
}

