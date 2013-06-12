namespace System.Data.SqlTypes
{
    using System;
    using System.IO;

    internal abstract class SqlStreamChars : INullable, IDisposable
    {
        protected SqlStreamChars()
        {
        }

        public virtual void Close()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public abstract void Flush();
        public abstract int Read(char[] buffer, int offset, int count);
        public virtual int ReadChar()
        {
            char[] buffer = new char[1];
            if (this.Read(buffer, 0, 1) == 0)
            {
                return -1;
            }
            return buffer[0];
        }

        public abstract long Seek(long offset, SeekOrigin origin);
        public abstract void SetLength(long value);
        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        public abstract void Write(char[] buffer, int offset, int count);
        public virtual void WriteChar(char value)
        {
            char[] buffer = new char[] { value };
            this.Write(buffer, 0, 1);
        }

        public abstract bool CanRead { get; }

        public abstract bool CanSeek { get; }

        public abstract bool CanWrite { get; }

        public abstract bool IsNull { get; }

        public abstract long Length { get; }

        public static SqlStreamChars Null
        {
            get
            {
                return new NullSqlStreamChars();
            }
        }

        public abstract long Position { get; set; }

        private class NullSqlStreamChars : SqlStreamChars
        {
            internal NullSqlStreamChars()
            {
            }

            public override void Close()
            {
            }

            public override void Flush()
            {
                throw new SqlNullValueException();
            }

            public override int Read(char[] buffer, int offset, int count)
            {
                throw new SqlNullValueException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new SqlNullValueException();
            }

            public override void SetLength(long value)
            {
                throw new SqlNullValueException();
            }

            public override void Write(char[] buffer, int offset, int count)
            {
                throw new SqlNullValueException();
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
                    return false;
                }
            }

            public override bool IsNull
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
                    throw new SqlNullValueException();
                }
            }

            public override long Position
            {
                get
                {
                    throw new SqlNullValueException();
                }
                set
                {
                    throw new SqlNullValueException();
                }
            }
        }
    }
}

