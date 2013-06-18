namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal sealed class MsmqTransportReceiveParameters : MsmqReceiveParameters
    {
        private int maxPoolSize;
        private System.ServiceModel.QueueTransferProtocol queueTransferProtocol;
        private bool useActiveDirectory;

        internal MsmqTransportReceiveParameters(MsmqTransportBindingElement bindingElement, MsmqUri.IAddressTranslator addressTranslator) : base(bindingElement, addressTranslator)
        {
            this.maxPoolSize = bindingElement.MaxPoolSize;
            this.useActiveDirectory = bindingElement.UseActiveDirectory;
            this.queueTransferProtocol = bindingElement.QueueTransferProtocol;
        }

        internal int MaxPoolSize
        {
            get
            {
                return this.maxPoolSize;
            }
        }

        internal System.ServiceModel.QueueTransferProtocol QueueTransferProtocol
        {
            get
            {
                return this.queueTransferProtocol;
            }
        }

        internal bool UseActiveDirectory
        {
            get
            {
                return this.useActiveDirectory;
            }
        }
    }
}

