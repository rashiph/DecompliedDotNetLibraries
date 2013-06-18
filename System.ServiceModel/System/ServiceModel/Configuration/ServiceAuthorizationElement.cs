namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IdentityModel.Policy;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;

    public sealed class ServiceAuthorizationElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            ServiceAuthorizationElement element = (ServiceAuthorizationElement) from;
            this.PrincipalPermissionMode = element.PrincipalPermissionMode;
            this.RoleProviderName = element.RoleProviderName;
            this.ImpersonateCallerForAllOperations = element.ImpersonateCallerForAllOperations;
            this.ServiceAuthorizationManagerType = element.ServiceAuthorizationManagerType;
            AuthorizationPolicyTypeElementCollection authorizationPolicies = element.AuthorizationPolicies;
            AuthorizationPolicyTypeElementCollection elements2 = this.AuthorizationPolicies;
            for (int i = 0; i < authorizationPolicies.Count; i++)
            {
                elements2.Add(authorizationPolicies[i]);
            }
        }

        protected internal override object CreateBehavior()
        {
            ServiceAuthorizationBehavior behavior = new ServiceAuthorizationBehavior {
                PrincipalPermissionMode = this.PrincipalPermissionMode
            };
            string roleProviderName = this.RoleProviderName;
            if (!string.IsNullOrEmpty(roleProviderName))
            {
                behavior.RoleProvider = SystemWebHelper.GetRoleProvider(roleProviderName);
                if (behavior.RoleProvider == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("InvalidRoleProviderSpecifiedInConfig", new object[] { roleProviderName })));
                }
            }
            behavior.ImpersonateCallerForAllOperations = this.ImpersonateCallerForAllOperations;
            string serviceAuthorizationManagerType = this.ServiceAuthorizationManagerType;
            if (!string.IsNullOrEmpty(serviceAuthorizationManagerType))
            {
                Type c = Type.GetType(serviceAuthorizationManagerType, true);
                if (!typeof(ServiceAuthorizationManager).IsAssignableFrom(c))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidServiceAuthorizationManagerType", new object[] { serviceAuthorizationManagerType, typeof(ServiceAuthorizationManager) })));
                }
                behavior.ServiceAuthorizationManager = (ServiceAuthorizationManager) Activator.CreateInstance(c);
            }
            AuthorizationPolicyTypeElementCollection authorizationPolicies = this.AuthorizationPolicies;
            if (authorizationPolicies.Count > 0)
            {
                List<IAuthorizationPolicy> list = new List<IAuthorizationPolicy>(authorizationPolicies.Count);
                for (int i = 0; i < authorizationPolicies.Count; i++)
                {
                    Type type = Type.GetType(authorizationPolicies[i].PolicyType, true);
                    if (!typeof(IAuthorizationPolicy).IsAssignableFrom(type))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidAuthorizationPolicyType", new object[] { authorizationPolicies[i].PolicyType, typeof(IAuthorizationPolicy) })));
                    }
                    list.Add((IAuthorizationPolicy) Activator.CreateInstance(type));
                }
                behavior.ExternalAuthorizationPolicies = list.AsReadOnly();
            }
            return behavior;
        }

        [ConfigurationProperty("authorizationPolicies")]
        public AuthorizationPolicyTypeElementCollection AuthorizationPolicies
        {
            get
            {
                return (AuthorizationPolicyTypeElementCollection) base["authorizationPolicies"];
            }
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(ServiceAuthorizationBehavior);
            }
        }

        [ConfigurationProperty("impersonateCallerForAllOperations", DefaultValue=false)]
        public bool ImpersonateCallerForAllOperations
        {
            get
            {
                return (bool) base["impersonateCallerForAllOperations"];
            }
            set
            {
                base["impersonateCallerForAllOperations"] = value;
            }
        }

        [ServiceModelEnumValidator(typeof(PrincipalPermissionModeHelper)), ConfigurationProperty("principalPermissionMode", DefaultValue=1)]
        public System.ServiceModel.Description.PrincipalPermissionMode PrincipalPermissionMode
        {
            get
            {
                return (System.ServiceModel.Description.PrincipalPermissionMode) base["principalPermissionMode"];
            }
            set
            {
                base["principalPermissionMode"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("principalPermissionMode", typeof(System.ServiceModel.Description.PrincipalPermissionMode), System.ServiceModel.Description.PrincipalPermissionMode.UseWindowsGroups, null, new ServiceModelEnumValidator(typeof(PrincipalPermissionModeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("roleProviderName", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("impersonateCallerForAllOperations", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("serviceAuthorizationManagerType", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("authorizationPolicies", typeof(AuthorizationPolicyTypeElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("roleProviderName", DefaultValue=""), StringValidator(MinLength=0)]
        public string RoleProviderName
        {
            get
            {
                return (string) base["roleProviderName"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["roleProviderName"] = value;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("serviceAuthorizationManagerType", DefaultValue="")]
        public string ServiceAuthorizationManagerType
        {
            get
            {
                return (string) base["serviceAuthorizationManagerType"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["serviceAuthorizationManagerType"] = value;
            }
        }
    }
}

