namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public sealed class ServiceSecurityAuditElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            ServiceSecurityAuditElement element = (ServiceSecurityAuditElement) from;
            this.AuditLogLocation = element.AuditLogLocation;
            this.SuppressAuditFailure = element.SuppressAuditFailure;
            this.ServiceAuthorizationAuditLevel = element.ServiceAuthorizationAuditLevel;
            this.MessageAuthenticationAuditLevel = element.MessageAuthenticationAuditLevel;
        }

        protected internal override object CreateBehavior()
        {
            return new ServiceSecurityAuditBehavior { AuditLogLocation = this.AuditLogLocation, SuppressAuditFailure = this.SuppressAuditFailure, ServiceAuthorizationAuditLevel = this.ServiceAuthorizationAuditLevel, MessageAuthenticationAuditLevel = this.MessageAuthenticationAuditLevel };
        }

        [ConfigurationProperty("auditLogLocation", DefaultValue=0), ServiceModelEnumValidator(typeof(AuditLogLocationHelper))]
        public System.ServiceModel.AuditLogLocation AuditLogLocation
        {
            get
            {
                return (System.ServiceModel.AuditLogLocation) base["auditLogLocation"];
            }
            set
            {
                base["auditLogLocation"] = value;
            }
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(ServiceSecurityAuditBehavior);
            }
        }

        [ServiceModelEnumValidator(typeof(AuditLevelHelper)), ConfigurationProperty("messageAuthenticationAuditLevel", DefaultValue=0)]
        public AuditLevel MessageAuthenticationAuditLevel
        {
            get
            {
                return (AuditLevel) base["messageAuthenticationAuditLevel"];
            }
            set
            {
                base["messageAuthenticationAuditLevel"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("auditLogLocation", typeof(System.ServiceModel.AuditLogLocation), System.ServiceModel.AuditLogLocation.Default, null, new ServiceModelEnumValidator(typeof(AuditLogLocationHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("suppressAuditFailure", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("serviceAuthorizationAuditLevel", typeof(AuditLevel), AuditLevel.None, null, new ServiceModelEnumValidator(typeof(AuditLevelHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("messageAuthenticationAuditLevel", typeof(AuditLevel), AuditLevel.None, null, new ServiceModelEnumValidator(typeof(AuditLevelHelper)), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ServiceModelEnumValidator(typeof(AuditLevelHelper)), ConfigurationProperty("serviceAuthorizationAuditLevel", DefaultValue=0)]
        public AuditLevel ServiceAuthorizationAuditLevel
        {
            get
            {
                return (AuditLevel) base["serviceAuthorizationAuditLevel"];
            }
            set
            {
                base["serviceAuthorizationAuditLevel"] = value;
            }
        }

        [ConfigurationProperty("suppressAuditFailure", DefaultValue=true)]
        public bool SuppressAuditFailure
        {
            get
            {
                return (bool) base["suppressAuditFailure"];
            }
            set
            {
                base["suppressAuditFailure"] = value;
            }
        }
    }
}

