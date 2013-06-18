namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;

    internal class MaxMessageSizeStream : DelegatingStream
    {
        private long bytesWritten;
        private long maxMessageSize;
        private long totalBytesRead;

        public MaxMessageSizeStream(Stream stream, long maxMessageSize) : base(stream)
        {
            this.maxMessageSize = maxMessageSize;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            count = this.PrepareRead(count);
            return base.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.PrepareWrite(count);
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        internal static Exception CreateMaxReceivedMessageSizeExceededException(long maxMessageSize)
        {
            string message = System.ServiceModel.SR.GetString("MaxReceivedMessageSizeExceeded", new object[] { maxMessageSize });
            return new CommunicationException(message, new QuotaExceededException(message));
        }

        internal static Exception CreateMaxSentMessageSizeExceededException(long maxMessageSize)
        {
            string message = System.ServiceModel.SR.GetString("MaxSentMessageSizeExceeded", new object[] { maxMessageSize });
            return new CommunicationException(message, new QuotaExceededException(message));
        }

        public override int EndRead(IAsyncResult result)
        {
            return this.FinishRead(base.EndRead(result));
        }

        private int FinishRead(int bytesRead)
        {
            this.totalBytesRead += bytesRead;
            return bytesRead;
        }

        private int PrepareRead(int bytesToRead)
        {
            if (this.totalBytesRead >= this.maxMessageSize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateMaxReceivedMessageSizeExceededException(this.maxMessageSize));
            }
            long num = this.maxMessageSize - this.totalBytesRead;
            if (num > 0x7fffffffL)
            {
                return bytesToRead;
            }
            return Math.Min(bytesToRead, (int) (this.maxMessageSize - this.totalBytesRead));
        }

        private void PrepareWrite(int bytesToWrite)
        {
            if ((this.bytesWritten + bytesToWrite) > this.maxMessageSize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateMaxSentMessageSizeExceededException(this.maxMessageSize));
            }
            this.bytesWritten += bytesToWrite;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = this.PrepareRead(count);
            return this.FinishRead(base.Read(buffer, offset, count));
        }

        public override int ReadByte()
        {
            this.PrepareRead(1);
            int num = base.ReadByte();
            if (num != -1)
            {
                this.FinishRead(1);
            }
            return num;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.PrepareWrite(count);
            base.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            this.PrepareWrite(1);
            base.WriteByte(value);
        }
    }
}

