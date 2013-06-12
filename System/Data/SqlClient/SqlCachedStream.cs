namespace System.Data.SqlClient
{
    using System;
    using System.Collections;
    using System.Data.Common;
    using System.IO;

    internal sealed class SqlCachedStream : Stream
    {
        private ArrayList _cachedBytes;
        private int _currentArrayIndex;
        private int _currentPosition;
        private long _totalLength;

        internal SqlCachedStream(SqlCachedBuffer sqlBuf)
        {
            this._cachedBytes = sqlBuf.CachedBytes;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (this._cachedBytes != null))
                {
                    this._cachedBytes.Clear();
                }
                this._cachedBytes = null;
                this._currentPosition = 0;
                this._currentArrayIndex = 0;
                this._totalLength = 0L;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
            throw ADP.NotSupported();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num2 = 0;
            byte[] sourceArray = null;
            if (this._cachedBytes == null)
            {
                throw ADP.StreamClosed("Read");
            }
            if (buffer == null)
            {
                throw ADP.ArgumentNull("buffer");
            }
            if ((offset < 0) || (count < 0))
            {
                throw ADP.ArgumentOutOfRange(string.Empty, (offset < 0) ? "offset" : "count");
            }
            if ((buffer.Length - offset) < count)
            {
                throw ADP.ArgumentOutOfRange("count");
            }
            if (this._cachedBytes.Count > this._currentArrayIndex)
            {
                sourceArray = (byte[]) this._cachedBytes[this._currentArrayIndex];
                while (count > 0)
                {
                    if (sourceArray.Length <= this._currentPosition)
                    {
                        this._currentArrayIndex++;
                        if (this._cachedBytes.Count <= this._currentArrayIndex)
                        {
                            return num2;
                        }
                        sourceArray = (byte[]) this._cachedBytes[this._currentArrayIndex];
                        this._currentPosition = 0;
                    }
                    int length = sourceArray.Length - this._currentPosition;
                    if (length > count)
                    {
                        length = count;
                    }
                    Array.Copy(sourceArray, this._currentPosition, buffer, offset, length);
                    this._currentPosition += length;
                    count -= length;
                    offset += length;
                    num2 += length;
                }
                return num2;
            }
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long lPos = 0L;
            if (this._cachedBytes == null)
            {
                throw ADP.StreamClosed("Read");
            }
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.SetInternalPosition(offset, "offset");
                    return lPos;

                case SeekOrigin.Current:
                    lPos = offset + this.Position;
                    this.SetInternalPosition(lPos, "offset");
                    return lPos;

                case SeekOrigin.End:
                    lPos = this.TotalLength + offset;
                    this.SetInternalPosition(lPos, "offset");
                    return lPos;
            }
            throw ADP.InvalidSeekOrigin("offset");
        }

        private void SetInternalPosition(long lPos, string argumentName)
        {
            long num = lPos;
            byte[] buffer = null;
            if (num < 0L)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
            for (int i = 0; i < this._cachedBytes.Count; i++)
            {
                buffer = (byte[]) this._cachedBytes[i];
                if (num > buffer.Length)
                {
                    num -= buffer.Length;
                }
                else
                {
                    this._currentArrayIndex = i;
                    this._currentPosition = (int) num;
                    return;
                }
            }
            if (num > 0L)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public override void SetLength(long value)
        {
            throw ADP.NotSupported();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw ADP.NotSupported();
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
                return this.TotalLength;
            }
        }

        public override long Position
        {
            get
            {
                long num = 0L;
                byte[] buffer = null;
                if (this._currentArrayIndex > 0)
                {
                    for (int i = 0; i < this._currentArrayIndex; i++)
                    {
                        buffer = (byte[]) this._cachedBytes[i];
                        num += buffer.Length;
                    }
                }
                return (num + this._currentPosition);
            }
            set
            {
                if (this._cachedBytes == null)
                {
                    throw ADP.StreamClosed("set_Position");
                }
                this.SetInternalPosition(value, "set_Position");
            }
        }

        private long TotalLength
        {
            get
            {
                if ((this._totalLength == 0L) && (this._cachedBytes != null))
                {
                    long num2 = 0L;
                    byte[] buffer = null;
                    for (int i = 0; i < this._cachedBytes.Count; i++)
                    {
                        buffer = (byte[]) this._cachedBytes[i];
                        num2 += buffer.Length;
                    }
                    this._totalLength = num2;
                }
                return this._totalLength;
            }
        }
    }
}

