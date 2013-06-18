namespace System.ServiceModel.XamlIntegration
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(UpnEndpointIdentity))]
    public class UpnEndpointIdentityExtension : MarkupExtension
    {
        public UpnEndpointIdentityExtension()
        {
        }

        public UpnEndpointIdentityExtension(UpnEndpointIdentity identity)
        {
            if (identity == null)
            {
                throw FxTrace.Exception.ArgumentNull("identity");
            }
            this.UpnName = (string) identity.IdentityClaim.Resource;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new UpnEndpointIdentity(this.UpnName);
        }

        public string UpnName { get; set; }
    }
}

