namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class PreReadStream : DelegatingStream
    {
        private byte[] preReadBuffer;

        public PreReadStream(Stream stream, byte[] preReadBuffer) : base(stream)
        {
            this.preReadBuffer = preReadBuffer;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            int num;
            if (this.ReadFromBuffer(buffer, offset, count, out num))
            {
                return new CompletedAsyncResult<int>(num, callback, state);
            }
            return base.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult result)
        {
            if (result is CompletedAsyncResult<int>)
            {
                return CompletedAsyncResult<int>.End(result);
            }
            return base.EndRead(result);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num;
            if (this.ReadFromBuffer(buffer, offset, count, out num))
            {
                return num;
            }
            return base.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            if (this.preReadBuffer != null)
            {
                int num;
                byte[] buffer = new byte[1];
                if (this.ReadFromBuffer(buffer, 0, 1, out num))
                {
                    return buffer[0];
                }
            }
            return base.ReadByte();
        }

        private bool ReadFromBuffer(byte[] buffer, int offset, int count, out int bytesRead)
        {
            if (this.preReadBuffer != null)
            {
                if (buffer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
                }
                if (offset >= buffer.Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", offset, System.ServiceModel.SR.GetString("OffsetExceedsBufferBound", new object[] { buffer.Length - 1 })));
                }
                if (count < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", count, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                if (count == 0)
                {
                    bytesRead = 0;
                }
                else
                {
                    buffer[offset] = this.preReadBuffer[0];
                    this.preReadBuffer = null;
                    bytesRead = 1;
                }
                return true;
            }
            bytesRead = -1;
            return false;
        }
    }
}

