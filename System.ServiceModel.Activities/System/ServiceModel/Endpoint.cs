namespace System.ServiceModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.XamlIntegration;
    using System.Xml.Linq;

    public class Endpoint
    {
        private Collection<AddressHeader> headers;

        public EndpointAddress GetAddress()
        {
            return this.GetAddress(null);
        }

        public EndpointAddress GetAddress(ServiceHostBase host)
        {
            if (this.AddressUri == null)
            {
                string errorMessageEndpointName = ContractValidationHelper.GetErrorMessageEndpointName(this.Name);
                string errorMessageEndpointServiceContractName = ContractValidationHelper.GetErrorMessageEndpointServiceContractName(this.ServiceContractName);
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.MissingUriInEndpoint(errorMessageEndpointName, errorMessageEndpointServiceContractName)));
            }
            Uri addressUri = null;
            if (this.AddressUri.IsAbsoluteUri)
            {
                addressUri = this.AddressUri;
            }
            else
            {
                if (this.Binding == null)
                {
                    string str3 = ContractValidationHelper.GetErrorMessageEndpointName(this.Name);
                    string str4 = ContractValidationHelper.GetErrorMessageEndpointServiceContractName(this.ServiceContractName);
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.RelativeUriRequiresBinding(str3, str4, this.AddressUri)));
                }
                if (host == null)
                {
                    string str5 = ContractValidationHelper.GetErrorMessageEndpointName(this.Name);
                    string str6 = ContractValidationHelper.GetErrorMessageEndpointServiceContractName(this.ServiceContractName);
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.RelativeUriRequiresHost(str5, str6, this.AddressUri)));
                }
                addressUri = host.MakeAbsoluteUri(this.AddressUri, this.Binding);
            }
            return new EndpointAddress(addressUri, this.Identity, new AddressHeaderCollection(this.Headers));
        }

        [DefaultValue((string) null)]
        public Uri AddressUri { get; set; }

        [DefaultValue((string) null)]
        public string BehaviorConfigurationName { get; set; }

        [DefaultValue((string) null)]
        public System.ServiceModel.Channels.Binding Binding { get; set; }

        public Collection<AddressHeader> Headers
        {
            get
            {
                if (this.headers == null)
                {
                    this.headers = new Collection<AddressHeader>();
                }
                return this.headers;
            }
        }

        [TypeConverter(typeof(EndpointIdentityConverter)), DefaultValue((string) null)]
        public EndpointIdentity Identity { get; set; }

        [DefaultValue((string) null)]
        public Uri ListenUri { get; set; }

        [DefaultValue((string) null)]
        public string Name { get; set; }

        [DefaultValue((string) null), TypeConverter(typeof(ServiceXNameTypeConverter))]
        public XName ServiceContractName { get; set; }
    }
}

