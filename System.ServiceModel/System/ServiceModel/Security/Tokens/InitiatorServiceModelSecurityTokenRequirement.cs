namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.ServiceModel;

    public sealed class InitiatorServiceModelSecurityTokenRequirement : ServiceModelSecurityTokenRequirement
    {
        public InitiatorServiceModelSecurityTokenRequirement()
        {
            base.Properties.Add(ServiceModelSecurityTokenRequirement.IsInitiatorProperty, true);
        }

        public override string ToString()
        {
            return base.InternalToString();
        }

        internal bool IsOutOfBandToken
        {
            get
            {
                return base.GetPropertyOrDefault<bool>(ServiceModelSecurityTokenRequirement.IsOutOfBandTokenProperty, false);
            }
            set
            {
                base.Properties[ServiceModelSecurityTokenRequirement.IsOutOfBandTokenProperty] = value;
            }
        }

        public EndpointAddress TargetAddress
        {
            get
            {
                return base.GetPropertyOrDefault<EndpointAddress>(ServiceModelSecurityTokenRequirement.TargetAddressProperty, null);
            }
            set
            {
                base.Properties[ServiceModelSecurityTokenRequirement.TargetAddressProperty] = value;
            }
        }

        public Uri Via
        {
            get
            {
                return base.GetPropertyOrDefault<Uri>(ServiceModelSecurityTokenRequirement.ViaProperty, null);
            }
            set
            {
                base.Properties[ServiceModelSecurityTokenRequirement.ViaProperty] = value;
            }
        }
    }
}

