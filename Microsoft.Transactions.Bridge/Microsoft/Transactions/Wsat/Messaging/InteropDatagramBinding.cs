namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel.Channels;

    internal class InteropDatagramBinding : CoordinationBinding
    {
        public InteropDatagramBinding(ProtocolVersion protocolVersion) : base("Interop", AtomicTransactionStrings.Version(protocolVersion).Namespace, protocolVersion)
        {
        }

        protected override void AddBindingElements(BindingElementCollection bindingElements)
        {
            base.AddTextEncodingBindingElement(bindingElements);
            base.AddInteropHttpsTransportBindingElement(bindingElements);
        }
    }
}

