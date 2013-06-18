namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class ConnectionBufferPool : QueuedObjectPool<byte[]>
    {
        private int bufferSize;
        private const int MaxBatchCount = 0x10;
        private const int MaxFreeCountFactor = 4;
        private const int SingleBatchSize = 0x20000;

        public ConnectionBufferPool(int bufferSize)
        {
            int num;
            if (bufferSize < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("bufferSize"));
            }
            this.bufferSize = bufferSize;
            if (bufferSize != 0)
            {
                num = ((0x20000 + bufferSize) - 1) / bufferSize;
                if (num > 0x10)
                {
                    num = 0x10;
                }
            }
            else
            {
                num = 0x10;
            }
            base.Initialize(num, num * 4);
        }

        protected override byte[] Create()
        {
            return DiagnosticUtility.Utility.AllocateByteArray(this.bufferSize);
        }

        public override bool Return(byte[] value)
        {
            Array.Clear(value, 0, value.Length);
            return base.Return(value);
        }

        public int BufferSize
        {
            get
            {
                return this.bufferSize;
            }
        }
    }
}

