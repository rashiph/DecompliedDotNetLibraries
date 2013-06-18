namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    public sealed class ServiceAuthenticationBehavior : IServiceBehavior
    {
        internal System.ServiceModel.ServiceAuthenticationManager defaultServiceAuthenticationManager;
        private bool isAuthenticationManagerSet;
        private bool isReadOnly;
        private System.ServiceModel.ServiceAuthenticationManager serviceAuthenticationManager;

        public ServiceAuthenticationBehavior()
        {
            this.ServiceAuthenticationManager = this.defaultServiceAuthenticationManager;
        }

        private ServiceAuthenticationBehavior(ServiceAuthenticationBehavior other)
        {
            this.serviceAuthenticationManager = other.ServiceAuthenticationManager;
            this.isReadOnly = other.isReadOnly;
        }

        internal ServiceAuthenticationBehavior Clone()
        {
            return new ServiceAuthenticationBehavior(this);
        }

        private ServiceEndpoint FindMatchingServiceEndpoint(System.ServiceModel.Description.ServiceDescription description, EndpointDispatcher endpointDispatcher)
        {
            foreach (ServiceEndpoint endpoint in description.Endpoints)
            {
                if (endpoint.Address.Equals(endpointDispatcher.EndpointAddress))
                {
                    return endpoint;
                }
            }
            return null;
        }

        private SecurityStandardsManager GetConfiguredSecurityStandardsManager(Binding binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            SecurityBindingElement element = binding.CreateBindingElements().Find<SecurityBindingElement>();
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("binding", System.ServiceModel.SR.GetString("NoSecurityBindingElementFound"));
            }
            return new SecurityStandardsManager(element.MessageSecurityVersion, new WSSecurityTokenSerializer(element.MessageSecurityVersion.SecurityVersion));
        }

        private bool IsSecureConversationBinding(Binding binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            SecurityBindingElement sbe = binding.CreateBindingElements().Find<SecurityBindingElement>();
            if (sbe != null)
            {
                foreach (SecurityTokenParameters parameters in new System.ServiceModel.Security.SecurityTokenParametersEnumerable(sbe, true))
                {
                    if (parameters is SecureConversationSecurityTokenParameters)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        public bool ShouldSerializeServiceAuthenticationManager()
        {
            return this.isAuthenticationManagerSet;
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            System.ServiceModel.ServiceAuthenticationManager manager = parameters.Find<System.ServiceModel.ServiceAuthenticationManager>();
            if (manager != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MultipleAuthenticationManagersInServiceBindingParameters", new object[] { manager })));
            }
            if (this.serviceAuthenticationManager != null)
            {
                parameters.Add(this.serviceAuthenticationManager);
            }
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("description"));
            }
            if (serviceHostBase == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceHostBase"));
            }
            if (this.serviceAuthenticationManager != null)
            {
                for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
                {
                    ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                    if ((channelDispatcher != null) && !ServiceMetadataBehavior.IsHttpGetMetadataDispatcher(description, channelDispatcher))
                    {
                        foreach (EndpointDispatcher dispatcher2 in channelDispatcher.Endpoints)
                        {
                            DispatchRuntime dispatchRuntime = dispatcher2.DispatchRuntime;
                            dispatchRuntime.ServiceAuthenticationManager = this.serviceAuthenticationManager;
                            ServiceEndpoint endpoint = this.FindMatchingServiceEndpoint(description, dispatcher2);
                            if ((endpoint != null) && this.IsSecureConversationBinding(endpoint.Binding))
                            {
                                SecurityStandardsManager configuredSecurityStandardsManager = this.GetConfiguredSecurityStandardsManager(endpoint.Binding);
                                dispatchRuntime.ServiceAuthenticationManager = new ServiceAuthenticationManagerWrapper(this.serviceAuthenticationManager, new string[] { configuredSecurityStandardsManager.SecureConversationDriver.CloseAction.Value });
                            }
                        }
                    }
                }
            }
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        private void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
        }

        public System.ServiceModel.ServiceAuthenticationManager ServiceAuthenticationManager
        {
            get
            {
                return this.serviceAuthenticationManager;
            }
            set
            {
                this.ThrowIfImmutable();
                this.serviceAuthenticationManager = value;
                this.isAuthenticationManagerSet = true;
            }
        }
    }
}

