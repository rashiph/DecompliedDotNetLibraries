namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;
    using System.IO;

    internal abstract class TcpReadingStream : Stream
    {
        protected TcpReadingStream()
        {
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public void ReadToEnd()
        {
            byte[] buffer = new byte[0x40];
            while (this.Read(buffer, 0, 0x40) > 0)
            {
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
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

        public virtual bool FoundEnd
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
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}

