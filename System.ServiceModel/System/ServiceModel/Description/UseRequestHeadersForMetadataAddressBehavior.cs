namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class UseRequestHeadersForMetadataAddressBehavior : IServiceBehavior
    {
        private Dictionary<string, int> defaultPortsByScheme;

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public IDictionary<string, int> DefaultPortsByScheme
        {
            get
            {
                if (this.defaultPortsByScheme == null)
                {
                    this.defaultPortsByScheme = new Dictionary<string, int>();
                }
                return this.defaultPortsByScheme;
            }
        }
    }
}

