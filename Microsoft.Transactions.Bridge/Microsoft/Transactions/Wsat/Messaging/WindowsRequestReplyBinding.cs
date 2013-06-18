namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Net;
    using System.ServiceModel.Channels;

    internal class WindowsRequestReplyBinding : CoordinationBinding
    {
        public WindowsRequestReplyBinding(ProtocolVersion protocolVersion) : base("Windows", "http://schemas.microsoft.com/ws/2006/02/transactions", protocolVersion)
        {
        }

        protected override void AddBindingElements(BindingElementCollection bindingElements)
        {
            base.AddTransactionFlowBindingElement(bindingElements);
            base.AddTextEncodingBindingElement(bindingElements);
            this.AddWindowsHttpsTransportBindingElement(bindingElements);
        }

        private void AddWindowsHttpsTransportBindingElement(BindingElementCollection bindingElements)
        {
            HttpsTransportBindingElement item = new HttpsTransportBindingElement {
                RequireClientCertificate = false,
                UseDefaultWebProxy = false,
                AuthenticationScheme = AuthenticationSchemes.Negotiate
            };
            bindingElements.Add(item);
        }
    }
}

