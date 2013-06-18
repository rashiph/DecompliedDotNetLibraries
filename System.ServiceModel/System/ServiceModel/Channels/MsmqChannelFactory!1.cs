namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal abstract class MsmqChannelFactory<TChannel> : MsmqChannelFactoryBase<TChannel>
    {
        private int maxPoolSize;
        private System.ServiceModel.QueueTransferProtocol queueTransferProtocol;
        private bool useActiveDirectory;

        protected MsmqChannelFactory(MsmqTransportBindingElement bindingElement, BindingContext context) : base(bindingElement, context)
        {
            this.maxPoolSize = bindingElement.MaxPoolSize;
            this.queueTransferProtocol = bindingElement.QueueTransferProtocol;
            this.useActiveDirectory = bindingElement.UseActiveDirectory;
        }

        public int MaxPoolSize
        {
            get
            {
                return this.maxPoolSize;
            }
        }

        public System.ServiceModel.QueueTransferProtocol QueueTransferProtocol
        {
            get
            {
                return this.queueTransferProtocol;
            }
        }

        public bool UseActiveDirectory
        {
            get
            {
                return this.useActiveDirectory;
            }
        }
    }
}

