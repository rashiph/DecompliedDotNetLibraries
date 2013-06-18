namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Net.Security;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;

    internal abstract class CoordinationBinding : Binding
    {
        internal const int MaxFaultSize = 0x10000;
        private ProtocolVersion protocolVersion;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CoordinationBinding(string name, string ns, ProtocolVersion protocolVersion) : base(name, ns)
        {
            this.protocolVersion = protocolVersion;
        }

        protected abstract void AddBindingElements(BindingElementCollection bindingElements);
        protected void AddCompositeDuplexBindingElement(BindingElementCollection bindingElements, Uri clientBaseAddress)
        {
            bindingElements.Add(new DuplexCorrelationBindingElement());
            InternalDuplexBindingElement item = new InternalDuplexBindingElement(true);
            bindingElements.Add(item);
            CompositeDuplexBindingElement element2 = new CompositeDuplexBindingElement {
                ClientBaseAddress = clientBaseAddress
            };
            bindingElements.Add(element2);
        }

        protected void AddInteropHttpsTransportBindingElement(BindingElementCollection bindingElements)
        {
            HttpsTransportBindingElement item = new HttpsTransportBindingElement {
                RequireClientCertificate = true,
                UseDefaultWebProxy = false
            };
            bindingElements.Add(item);
        }

        protected void AddNamedPipeBindingElement(BindingElementCollection bindingElements)
        {
            NamedPipeTransportBindingElement item = new NamedPipeTransportBindingElement {
                MaxPendingConnections = 50
            };
            item.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint = 0x19;
            item.MaxPendingAccepts = 0x19;
            bindingElements.Add(item);
        }

        protected void AddOneWayBindingElement(BindingElementCollection bindingElements)
        {
            bindingElements.Add(new OneWayBindingElement());
        }

        protected void AddTextEncodingBindingElement(BindingElementCollection bindingElements)
        {
            TextMessageEncodingBindingElement item = new TextMessageEncodingBindingElement {
                WriteEncoding = Encoding.UTF8,
                MessageVersion = MessagingVersionHelper.MessageVersion(this.protocolVersion)
            };
            bindingElements.Add(item);
        }

        protected void AddTransactionFlowBindingElement(BindingElementCollection bindingElements)
        {
            TransactionProtocol transactionProtocol = (this.protocolVersion == ProtocolVersion.Version10) ? TransactionProtocol.WSAtomicTransactionOctober2004 : TransactionProtocol.WSAtomicTransaction11;
            TransactionFlowBindingElement item = new TransactionFlowBindingElement(transactionProtocol) {
                Transactions = false,
                IssuedTokens = TransactionFlowOption.Allowed
            };
            bindingElements.Add(item);
        }

        protected void AddTransportSecurityBindingElement(BindingElementCollection bindingElements)
        {
            TransportSecurityBindingElement item = new TransportSecurityBindingElement {
                MessageSecurityVersion = MessagingVersionHelper.SecurityVersion(this.protocolVersion)
            };
            bindingElements.Add(item);
        }

        protected void AddWindowsStreamSecurityBindingElement(BindingElementCollection bindingElements)
        {
            WindowsStreamSecurityBindingElement item = new WindowsStreamSecurityBindingElement {
                ProtectionLevel = ProtectionLevel.EncryptAndSign
            };
            bindingElements.Add(item);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection bindingElements = new BindingElementCollection();
            this.AddBindingElements(bindingElements);
            return bindingElements;
        }

        public override string Scheme
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return Uri.UriSchemeHttps;
            }
        }

        private class DuplexCorrelationBindingElement : BindingElement
        {
            public override BindingElement Clone()
            {
                return this;
            }

            public override T GetProperty<T>(BindingContext context) where T: class
            {
                if (!(typeof(T) == typeof(ISecurityCapabilities)))
                {
                    return context.GetInnerProperty<T>();
                }
                ISecurityCapabilities innerProperty = context.GetInnerProperty<ISecurityCapabilities>();
                if (innerProperty != null)
                {
                    return (T) new SecurityCapabilities(innerProperty.SupportsClientAuthentication, true, innerProperty.SupportsClientWindowsIdentity, innerProperty.SupportedRequestProtectionLevel, innerProperty.SupportedRequestProtectionLevel);
                }
                return default(T);
            }
        }
    }
}

