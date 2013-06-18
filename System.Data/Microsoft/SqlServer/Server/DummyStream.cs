namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data;
    using System.IO;

    internal sealed class DummyStream : Stream
    {
        private long m_size;

        private void DontDoIt()
        {
            throw new Exception(Res.GetString("Sql_InternalError"));
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.DontDoIt();
            return -1;
        }

        public override long Seek(long value, SeekOrigin loc)
        {
            this.DontDoIt();
            return -1L;
        }

        public override void SetLength(long value)
        {
            this.m_size = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.m_size += count;
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
                return true;
            }
        }

        public override long Length
        {
            get
            {
                return this.m_size;
            }
        }

        public override long Position
        {
            get
            {
                return this.m_size;
            }
            set
            {
                this.m_size = value;
            }
        }
    }
}

