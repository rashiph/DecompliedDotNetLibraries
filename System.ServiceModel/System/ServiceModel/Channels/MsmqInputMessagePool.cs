namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal sealed class MsmqInputMessagePool : SynchronizedDisposablePool<MsmqInputMessage>, IMsmqMessagePool, IDisposable
    {
        private int maxPoolSize;

        internal MsmqInputMessagePool(int maxPoolSize) : base(maxPoolSize)
        {
            this.maxPoolSize = maxPoolSize;
        }

        void IMsmqMessagePool.ReturnMessage(MsmqInputMessage message)
        {
            if (!base.Return(message))
            {
                MsmqDiagnostics.PoolFull(this.maxPoolSize);
                message.Dispose();
            }
        }

        MsmqInputMessage IMsmqMessagePool.TakeMessage()
        {
            MsmqInputMessage message = base.Take();
            if (message == null)
            {
                message = new MsmqInputMessage();
            }
            return message;
        }
    }
}

