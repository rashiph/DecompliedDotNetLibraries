namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Threading;

    internal class PreReadConnection : DelegatingConnection
    {
        private int asyncBytesRead;
        private int preReadCount;
        private byte[] preReadData;
        private int preReadOffset;

        public PreReadConnection(IConnection innerConnection, byte[] initialData) : this(innerConnection, initialData, 0, initialData.Length)
        {
        }

        public PreReadConnection(IConnection innerConnection, byte[] initialData, int initialOffset, int initialSize) : base(innerConnection)
        {
            this.preReadData = initialData;
            this.preReadOffset = initialOffset;
            this.preReadCount = initialSize;
        }

        public void AddPreReadData(byte[] initialData, int initialOffset, int initialSize)
        {
            if (this.preReadCount > 0)
            {
                byte[] preReadData = this.preReadData;
                this.preReadData = DiagnosticUtility.Utility.AllocateByteArray(initialSize + this.preReadCount);
                Buffer.BlockCopy(preReadData, this.preReadOffset, this.preReadData, 0, this.preReadCount);
                Buffer.BlockCopy(initialData, initialOffset, this.preReadData, this.preReadCount, initialSize);
                this.preReadOffset = 0;
                this.preReadCount += initialSize;
            }
            else
            {
                this.preReadData = initialData;
                this.preReadOffset = initialOffset;
                this.preReadCount = initialSize;
            }
        }

        public override AsyncReadResult BeginRead(int offset, int size, TimeSpan timeout, WaitCallback callback, object state)
        {
            ConnectionUtilities.ValidateBufferBounds(this.AsyncReadBufferSize, offset, size);
            if (this.preReadCount > 0)
            {
                int count = Math.Min(size, this.preReadCount);
                Buffer.BlockCopy(this.preReadData, this.preReadOffset, this.AsyncReadBuffer, offset, count);
                this.preReadOffset += count;
                this.preReadCount -= count;
                this.asyncBytesRead = count;
                return AsyncReadResult.Completed;
            }
            return base.BeginRead(offset, size, timeout, callback, state);
        }

        public override int EndRead()
        {
            if (this.asyncBytesRead > 0)
            {
                int asyncBytesRead = this.asyncBytesRead;
                this.asyncBytesRead = 0;
                return asyncBytesRead;
            }
            return base.EndRead();
        }

        public override int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);
            if (this.preReadCount > 0)
            {
                int count = Math.Min(size, this.preReadCount);
                Buffer.BlockCopy(this.preReadData, this.preReadOffset, buffer, offset, count);
                this.preReadOffset += count;
                this.preReadCount -= count;
                return count;
            }
            return base.Read(buffer, offset, size, timeout);
        }
    }
}

