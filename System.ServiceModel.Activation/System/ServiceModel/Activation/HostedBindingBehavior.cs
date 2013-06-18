namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    internal class HostedBindingBehavior : IServiceBehavior
    {
        private System.ServiceModel.Activation.VirtualPathExtension virtualPathExtension;

        internal HostedBindingBehavior(System.ServiceModel.Activation.VirtualPathExtension virtualPathExtension)
        {
            this.virtualPathExtension = virtualPathExtension;
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
            if (parameters == null)
            {
                throw FxTrace.Exception.ArgumentNull("parameters");
            }
            parameters.Add(this.virtualPathExtension);
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        public System.ServiceModel.Activation.VirtualPathExtension VirtualPathExtension
        {
            get
            {
                return this.virtualPathExtension;
            }
        }
    }
}

