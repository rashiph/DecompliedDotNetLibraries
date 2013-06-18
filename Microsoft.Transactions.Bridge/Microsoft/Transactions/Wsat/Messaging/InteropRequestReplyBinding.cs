namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel.Channels;

    internal class InteropRequestReplyBinding : CoordinationBinding
    {
        private Uri clientBaseAddress;

        public InteropRequestReplyBinding(Uri clientBaseAddress, ProtocolVersion protocolVersion) : base("Interop", AtomicTransactionStrings.Version(protocolVersion).Namespace, protocolVersion)
        {
            this.clientBaseAddress = clientBaseAddress;
        }

        protected override void AddBindingElements(BindingElementCollection bindingElements)
        {
            base.AddCompositeDuplexBindingElement(bindingElements, this.clientBaseAddress);
            base.AddOneWayBindingElement(bindingElements);
            base.AddTextEncodingBindingElement(bindingElements);
            base.AddInteropHttpsTransportBindingElement(bindingElements);
        }
    }
}

