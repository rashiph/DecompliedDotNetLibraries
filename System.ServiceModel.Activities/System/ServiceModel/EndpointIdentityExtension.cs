namespace System.ServiceModel
{
    using System;
    using System.IdentityModel.Claims;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Activities;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(EndpointIdentity))]
    public class EndpointIdentityExtension : MarkupExtension
    {
        public EndpointIdentityExtension()
        {
        }

        public EndpointIdentityExtension(EndpointIdentity identity)
        {
            if (identity == null)
            {
                throw FxTrace.Exception.ArgumentNull("identity");
            }
            this.ClaimType = identity.IdentityClaim.ClaimType;
            this.ClaimRight = identity.IdentityClaim.Right;
            this.ClaimResource = identity.IdentityClaim.Resource;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Claim identity = new Claim(this.ClaimType, this.ClaimResource, this.ClaimRight);
            return EndpointIdentity.CreateIdentity(identity);
        }

        public object ClaimResource { get; set; }

        public string ClaimRight { get; set; }

        public string ClaimType { get; set; }
    }
}

