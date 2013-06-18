namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal class ApplyHostConfigurationBehavior : IServiceBehavior
    {
        internal ApplyHostConfigurationBehavior()
        {
        }

        private void FailActivationIfEndpointsHaveAbsoluteAddress(ServiceHostBase service)
        {
            foreach (ServiceEndpoint endpoint in service.Description.Endpoints)
            {
                if (IsSchemeHttpOrHttps(endpoint.Binding.Scheme))
                {
                    if (endpoint.UnresolvedListenUri != null)
                    {
                        ThrowIfAbsolute(endpoint.UnresolvedListenUri);
                    }
                    else
                    {
                        ThrowIfAbsolute(endpoint.UnresolvedAddress);
                    }
                }
            }
            ServiceDebugBehavior behavior = service.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (behavior != null)
            {
                if (behavior.HttpHelpPageEnabled)
                {
                    ThrowIfAbsolute(behavior.HttpHelpPageUrl);
                }
                if (behavior.HttpsHelpPageEnabled)
                {
                    ThrowIfAbsolute(behavior.HttpsHelpPageUrl);
                }
            }
            ServiceMetadataBehavior behavior2 = service.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (behavior2 != null)
            {
                if (behavior2.HttpGetEnabled)
                {
                    ThrowIfAbsolute(behavior2.HttpGetUrl);
                }
                if (behavior2.HttpsGetEnabled)
                {
                    ThrowIfAbsolute(behavior2.HttpsGetUrl);
                }
            }
        }

        private static bool IsSchemeHttpOrHttps(string scheme)
        {
            if (string.Compare(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return (string.Compare(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) == 0);
            }
            return true;
        }

        private void SetEndpointAddressFilterToIgnorePort(ServiceHostBase service)
        {
            for (int i = 0; i < service.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher dispatcher = service.ChannelDispatchers[i] as ChannelDispatcher;
                if ((dispatcher != null) && IsSchemeHttpOrHttps(dispatcher.Listener.Uri.Scheme))
                {
                    for (int j = 0; j < dispatcher.Endpoints.Count; j++)
                    {
                        EndpointDispatcher dispatcher2 = dispatcher.Endpoints[j];
                        EndpointAddressMessageFilter addressFilter = dispatcher2.AddressFilter as EndpointAddressMessageFilter;
                        if (addressFilter != null)
                        {
                            addressFilter.ComparePort = false;
                        }
                    }
                }
            }
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase service, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase service)
        {
            if (ServiceHostingEnvironment.MultipleSiteBindingsEnabled)
            {
                this.SetEndpointAddressFilterToIgnorePort(service);
            }
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase service)
        {
            if ((service.Description.Endpoints != null) && ServiceHostingEnvironment.MultipleSiteBindingsEnabled)
            {
                this.FailActivationIfEndpointsHaveAbsoluteAddress(service);
            }
        }

        private static void ThrowIfAbsolute(Uri uri)
        {
            if ((uri != null) && uri.IsAbsoluteUri)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_SharedEndpointRequiresRelativeEndpoint(uri.ToString())));
            }
        }
    }
}

