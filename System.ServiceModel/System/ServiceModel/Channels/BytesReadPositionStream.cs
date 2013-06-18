namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;

    internal abstract class BytesReadPositionStream : DelegatingStream
    {
        private int bytesSent;

        protected BytesReadPositionStream(Stream stream) : base(stream)
        {
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.bytesSent += count;
            return base.BaseStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.BaseStream.Write(buffer, offset, count);
            this.bytesSent += count;
        }

        public override void WriteByte(byte value)
        {
            base.BaseStream.WriteByte(value);
            this.bytesSent++;
        }

        public override long Position
        {
            get
            {
                return (long) this.bytesSent;
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SeekNotSupported")));
            }
        }
    }
}

