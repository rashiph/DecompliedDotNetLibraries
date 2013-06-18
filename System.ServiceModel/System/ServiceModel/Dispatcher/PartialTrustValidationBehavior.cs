namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.MsmqIntegration;

    internal class PartialTrustValidationBehavior : IServiceBehavior, IEndpointBehavior
    {
        private static PartialTrustValidationBehavior instance;

        public void AddBindingParameters(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            this.ValidateEndpoint(endpoint);
        }

        public void Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            for (int i = 0; i < description.Endpoints.Count; i++)
            {
                ServiceEndpoint endpoint = description.Endpoints[i];
                if (endpoint != null)
                {
                    this.ValidateEndpoint(endpoint);
                }
            }
        }

        private void ValidateEndpoint(ServiceEndpoint endpoint)
        {
            if (endpoint.Binding != null)
            {
                new BindingValidator(endpoint.Binding).Validate();
            }
        }

        internal static PartialTrustValidationBehavior Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PartialTrustValidationBehavior();
                }
                return instance;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BindingValidator
        {
            private static System.Type[] unsupportedBindings;
            private static System.Type[] unsupportedBindingElements;
            private Binding binding;
            private static readonly PermissionSet fullTrust;
            internal BindingValidator(Binding binding)
            {
                this.binding = binding;
            }

            internal void Validate()
            {
                System.Type bindingType = this.binding.GetType();
                if (this.IsUnsupportedBindingType(bindingType))
                {
                    this.UnsupportedSecurityCheck("FullTrustOnlyBindingSecurityCheck1", bindingType);
                }
                string resource = typeof(WSHttpBinding).IsAssignableFrom(bindingType) ? "FullTrustOnlyBindingElementSecurityCheckWSHttpBinding1" : "FullTrustOnlyBindingElementSecurityCheck1";
                foreach (BindingElement element in this.binding.CreateBindingElements())
                {
                    System.Type type = element.GetType();
                    if ((element != null) && this.IsUnsupportedBindingElementType(type))
                    {
                        this.UnsupportedSecurityCheck(resource, type);
                    }
                }
            }

            private bool IsUnsupportedBindingType(System.Type bindingType)
            {
                for (int i = 0; i < unsupportedBindings.Length; i++)
                {
                    if (unsupportedBindings[i] == bindingType)
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool IsUnsupportedBindingElementType(System.Type bindingElementType)
            {
                for (int i = 0; i < unsupportedBindingElements.Length; i++)
                {
                    if (unsupportedBindingElements[i] == bindingElementType)
                    {
                        return true;
                    }
                }
                return false;
            }

            private void UnsupportedSecurityCheck(string resource, System.Type type)
            {
                try
                {
                    fullTrust.Demand();
                }
                catch (SecurityException)
                {
                    throw new InvalidOperationException(System.ServiceModel.SR.GetString(resource, new object[] { this.binding.Name, type }));
                }
            }

            static BindingValidator()
            {
                unsupportedBindings = new System.Type[] { typeof(NetTcpBinding), typeof(NetNamedPipeBinding), typeof(WSDualHttpBinding), typeof(WS2007FederationHttpBinding), typeof(WSFederationHttpBinding), typeof(NetMsmqBinding), typeof(NetPeerTcpBinding), typeof(MsmqIntegrationBinding) };
                unsupportedBindingElements = new System.Type[] { typeof(AsymmetricSecurityBindingElement), typeof(CompositeDuplexBindingElement), typeof(MsmqTransportBindingElement), typeof(NamedPipeTransportBindingElement), typeof(OneWayBindingElement), typeof(PeerCustomResolverBindingElement), typeof(PeerTransportBindingElement), typeof(PnrpPeerResolverBindingElement), typeof(ReliableSessionBindingElement), typeof(SymmetricSecurityBindingElement), typeof(TcpTransportBindingElement), typeof(TransportSecurityBindingElement), typeof(MtomMessageEncodingBindingElement) };
                fullTrust = new PermissionSet(PermissionState.Unrestricted);
            }
        }
    }
}

