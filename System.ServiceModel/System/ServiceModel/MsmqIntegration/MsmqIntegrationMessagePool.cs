namespace System.ServiceModel.MsmqIntegration
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal sealed class MsmqIntegrationMessagePool : SynchronizedDisposablePool<MsmqIntegrationInputMessage>, IMsmqMessagePool, IDisposable
    {
        private int maxPoolSize;

        internal MsmqIntegrationMessagePool(int maxPoolSize) : base(maxPoolSize)
        {
            this.maxPoolSize = maxPoolSize;
        }

        void IMsmqMessagePool.ReturnMessage(MsmqInputMessage message)
        {
            if (!base.Return(message as MsmqIntegrationInputMessage))
            {
                MsmqDiagnostics.PoolFull(this.maxPoolSize);
                message.Dispose();
            }
        }

        MsmqInputMessage IMsmqMessagePool.TakeMessage()
        {
            MsmqIntegrationInputMessage message = base.Take();
            if (message == null)
            {
                message = new MsmqIntegrationInputMessage();
            }
            return message;
        }
    }
}

