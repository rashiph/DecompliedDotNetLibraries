namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;

    internal class SecurityCookieModeValidator : IServiceBehavior
    {
        private void CheckForCookie(SecurityTokenParameters tokenParameters, ServiceEndpoint endpoint)
        {
            bool flag = false;
            SecureConversationSecurityTokenParameters parameters = tokenParameters as SecureConversationSecurityTokenParameters;
            if ((parameters != null) && !parameters.RequireCancellation)
            {
                flag = true;
            }
            SspiSecurityTokenParameters parameters2 = tokenParameters as SspiSecurityTokenParameters;
            if ((parameters2 != null) && !parameters2.RequireCancellation)
            {
                flag = true;
            }
            SspiSecurityTokenParameters parameters3 = tokenParameters as SspiSecurityTokenParameters;
            if ((parameters3 != null) && !parameters3.RequireCancellation)
            {
                flag = true;
            }
            if (flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("RequireNonCookieMode", new object[] { endpoint.Binding.Name, endpoint.Binding.Namespace })));
            }
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription service, ServiceHostBase serviceHostBase)
        {
            foreach (ServiceEndpoint endpoint in service.Endpoints)
            {
                foreach (BindingElement element in endpoint.Binding.CreateBindingElements())
                {
                    SymmetricSecurityBindingElement element2 = element as SymmetricSecurityBindingElement;
                    if (element2 != null)
                    {
                        this.CheckForCookie(element2.ProtectionTokenParameters, endpoint);
                        foreach (SecurityTokenParameters parameters in element2.EndpointSupportingTokenParameters.Endorsing)
                        {
                            this.CheckForCookie(parameters, endpoint);
                        }
                        break;
                    }
                }
            }
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription service, ServiceHostBase serviceHostBase)
        {
        }
    }
}

