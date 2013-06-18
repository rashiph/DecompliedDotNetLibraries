namespace System.ServiceModel.XamlIntegration
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(SpnEndpointIdentity))]
    public class SpnEndpointIdentityExtension : MarkupExtension
    {
        public SpnEndpointIdentityExtension()
        {
        }

        public SpnEndpointIdentityExtension(SpnEndpointIdentity identity)
        {
            if (identity == null)
            {
                throw FxTrace.Exception.ArgumentNull("identity");
            }
            this.SpnName = (string) identity.IdentityClaim.Resource;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new SpnEndpointIdentity(this.SpnName);
        }

        public string SpnName { get; set; }
    }
}

