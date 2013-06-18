namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.ServiceModel;

    public sealed class RecipientServiceModelSecurityTokenRequirement : ServiceModelSecurityTokenRequirement
    {
        public RecipientServiceModelSecurityTokenRequirement()
        {
            base.Properties.Add(ServiceModelSecurityTokenRequirement.IsInitiatorProperty, false);
        }

        public override string ToString()
        {
            return base.InternalToString();
        }

        public System.ServiceModel.AuditLogLocation AuditLogLocation
        {
            get
            {
                return base.GetPropertyOrDefault<System.ServiceModel.AuditLogLocation>(ServiceModelSecurityTokenRequirement.AuditLogLocationProperty, System.ServiceModel.AuditLogLocation.Default);
            }
            set
            {
                base.Properties[ServiceModelSecurityTokenRequirement.AuditLogLocationProperty] = value;
            }
        }

        public Uri ListenUri
        {
            get
            {
                return base.GetPropertyOrDefault<Uri>(ServiceModelSecurityTokenRequirement.ListenUriProperty, null);
            }
            set
            {
                base.Properties[ServiceModelSecurityTokenRequirement.ListenUriProperty] = value;
            }
        }

        public AuditLevel MessageAuthenticationAuditLevel
        {
            get
            {
                return base.GetPropertyOrDefault<AuditLevel>(ServiceModelSecurityTokenRequirement.MessageAuthenticationAuditLevelProperty, AuditLevel.None);
            }
            set
            {
                base.Properties[ServiceModelSecurityTokenRequirement.MessageAuthenticationAuditLevelProperty] = value;
            }
        }

        public bool SuppressAuditFailure
        {
            get
            {
                return base.GetPropertyOrDefault<bool>(ServiceModelSecurityTokenRequirement.SuppressAuditFailureProperty, true);
            }
            set
            {
                base.Properties[ServiceModelSecurityTokenRequirement.SuppressAuditFailureProperty] = value;
            }
        }
    }
}

