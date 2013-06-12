namespace System.Windows.Forms
{
    using System;
    using System.IO;

    internal class DataStreamFromComStream : Stream
    {
        private UnsafeNativeMethods.IStream comStream;

        public DataStreamFromComStream(UnsafeNativeMethods.IStream comStream)
        {
            this.comStream = comStream;
        }

        private unsafe int _Read(void* handle, int bytes)
        {
            return this.comStream.Read((IntPtr) handle, bytes);
        }

        private unsafe int _Write(void* handle, int bytes)
        {
            return this.comStream.Write((IntPtr) handle, bytes);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (this.comStream != null))
                {
                    try
                    {
                        this.comStream.Commit(0);
                    }
                    catch (Exception)
                    {
                    }
                }
                this.comStream = null;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        ~DataStreamFromComStream()
        {
            this.Dispose(false);
        }

        public override void Flush()
        {
        }

        public override unsafe int Read(byte[] buffer, int index, int count)
        {
            int num = 0;
            if (((count > 0) && (index >= 0)) && ((count + index) <= buffer.Length))
            {
                fixed (byte* numRef = buffer)
                {
                    num = this._Read((void*) (numRef + index), count);
                }
            }
            return num;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.comStream.Seek(offset, (int) origin);
        }

        public override void SetLength(long value)
        {
            this.comStream.SetSize(value);
        }

        public override unsafe void Write(byte[] buffer, int index, int count)
        {
            int num = 0;
            if (((count > 0) && (index >= 0)) && ((count + index) <= buffer.Length))
            {
                try
                {
                    try
                    {
                        fixed (byte* numRef = buffer)
                        {
                            num = this._Write((void*) (numRef + index), count);
                        }
                    }
                    finally
                    {
                        numRef = null;
                    }
                }
                catch
                {
                }
            }
            if (num < count)
            {
                throw new IOException(System.Windows.Forms.SR.GetString("DataStreamWrite"));
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
                long position = this.Position;
                long num2 = this.Seek(0L, SeekOrigin.End);
                this.Position = position;
                return (num2 - position);
            }
        }

        public override long Position
        {
            get
            {
                return this.Seek(0L, SeekOrigin.Current);
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }
        }
    }
}

