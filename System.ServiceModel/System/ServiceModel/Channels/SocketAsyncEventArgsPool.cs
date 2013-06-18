namespace System.ServiceModel.Channels
{
    using System;
    using System.Net.Sockets;
    using System.ServiceModel;

    internal class SocketAsyncEventArgsPool : QueuedObjectPool<SocketAsyncEventArgs>
    {
        private int acceptBufferSize;
        private const int MaxBatchCount = 0x10;
        private const int MaxFreeCountFactor = 4;
        private const int SingleBatchSize = 0x20000;

        public SocketAsyncEventArgsPool(int acceptBufferSize)
        {
            if (acceptBufferSize <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("acceptBufferSize"));
            }
            this.acceptBufferSize = acceptBufferSize;
            int batchAllocCount = ((0x20000 + acceptBufferSize) - 1) / acceptBufferSize;
            if (batchAllocCount > 0x10)
            {
                batchAllocCount = 0x10;
            }
            base.Initialize(batchAllocCount, batchAllocCount * 4);
        }

        protected override SocketAsyncEventArgs Create()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            byte[] buffer = DiagnosticUtility.Utility.AllocateByteArray(this.acceptBufferSize);
            args.SetBuffer(buffer, 0, this.acceptBufferSize);
            return args;
        }

        public override bool Return(SocketAsyncEventArgs value)
        {
            if (!base.Return(value))
            {
                value.Dispose();
                return false;
            }
            return true;
        }
    }
}

