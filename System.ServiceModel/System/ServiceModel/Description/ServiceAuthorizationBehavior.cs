namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Web.Security;

    public sealed class ServiceAuthorizationBehavior : IServiceBehavior
    {
        internal const bool DefaultImpersonateCallerForAllOperations = false;
        internal const System.ServiceModel.Description.PrincipalPermissionMode DefaultPrincipalPermissionMode = System.ServiceModel.Description.PrincipalPermissionMode.UseWindowsGroups;
        private ReadOnlyCollection<IAuthorizationPolicy> externalAuthorizationPolicies;
        private bool impersonateCallerForAllOperations;
        private bool isAuthorizationManagerSet;
        private bool isExternalPoliciesSet;
        private bool isReadOnly;
        private System.ServiceModel.Description.PrincipalPermissionMode principalPermissionMode;
        private object roleProvider;
        private System.ServiceModel.ServiceAuthorizationManager serviceAuthorizationManager;

        public ServiceAuthorizationBehavior()
        {
            this.impersonateCallerForAllOperations = false;
            this.principalPermissionMode = System.ServiceModel.Description.PrincipalPermissionMode.UseWindowsGroups;
        }

        private ServiceAuthorizationBehavior(ServiceAuthorizationBehavior other)
        {
            this.impersonateCallerForAllOperations = other.impersonateCallerForAllOperations;
            this.principalPermissionMode = other.principalPermissionMode;
            this.roleProvider = other.roleProvider;
            this.isExternalPoliciesSet = other.isExternalPoliciesSet;
            this.isAuthorizationManagerSet = other.isAuthorizationManagerSet;
            if (other.isExternalPoliciesSet || other.isAuthorizationManagerSet)
            {
                this.CopyAuthorizationPoliciesAndManager(other);
            }
            this.isReadOnly = other.isReadOnly;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ApplyAuthorizationPoliciesAndManager(DispatchRuntime behavior)
        {
            if (this.externalAuthorizationPolicies != null)
            {
                behavior.ExternalAuthorizationPolicies = this.externalAuthorizationPolicies;
            }
            if (this.serviceAuthorizationManager != null)
            {
                behavior.ServiceAuthorizationManager = this.serviceAuthorizationManager;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ApplyRoleProvider(DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.RoleProvider = (System.Web.Security.RoleProvider) this.roleProvider;
        }

        internal ServiceAuthorizationBehavior Clone()
        {
            return new ServiceAuthorizationBehavior(this);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CopyAuthorizationPoliciesAndManager(ServiceAuthorizationBehavior other)
        {
            this.externalAuthorizationPolicies = other.externalAuthorizationPolicies;
            this.serviceAuthorizationManager = other.serviceAuthorizationManager;
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        public bool ShouldSerializeExternalAuthorizationPolicies()
        {
            return this.isExternalPoliciesSet;
        }

        public bool ShouldSerializeServiceAuthorizationManager()
        {
            return this.isAuthorizationManagerSet;
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("description"));
            }
            if (serviceHostBase == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceHostBase"));
            }
            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if ((channelDispatcher != null) && !ServiceMetadataBehavior.IsHttpGetMetadataDispatcher(description, channelDispatcher))
                {
                    foreach (EndpointDispatcher dispatcher2 in channelDispatcher.Endpoints)
                    {
                        DispatchRuntime dispatchRuntime = dispatcher2.DispatchRuntime;
                        dispatchRuntime.PrincipalPermissionMode = this.principalPermissionMode;
                        if (!dispatcher2.IsSystemEndpoint)
                        {
                            dispatchRuntime.ImpersonateCallerForAllOperations = this.impersonateCallerForAllOperations;
                        }
                        if (this.roleProvider != null)
                        {
                            this.ApplyRoleProvider(dispatchRuntime);
                        }
                        if (this.isAuthorizationManagerSet || this.isExternalPoliciesSet)
                        {
                            this.ApplyAuthorizationPoliciesAndManager(dispatchRuntime);
                        }
                    }
                }
            }
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        private void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
        }

        public ReadOnlyCollection<IAuthorizationPolicy> ExternalAuthorizationPolicies
        {
            get
            {
                return this.externalAuthorizationPolicies;
            }
            set
            {
                this.ThrowIfImmutable();
                this.isExternalPoliciesSet = true;
                this.externalAuthorizationPolicies = value;
            }
        }

        [DefaultValue(false)]
        public bool ImpersonateCallerForAllOperations
        {
            get
            {
                return this.impersonateCallerForAllOperations;
            }
            set
            {
                this.ThrowIfImmutable();
                this.impersonateCallerForAllOperations = value;
            }
        }

        [DefaultValue(1)]
        public System.ServiceModel.Description.PrincipalPermissionMode PrincipalPermissionMode
        {
            get
            {
                return this.principalPermissionMode;
            }
            set
            {
                if (!PrincipalPermissionModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.ThrowIfImmutable();
                this.principalPermissionMode = value;
            }
        }

        [DefaultValue((string) null)]
        public System.Web.Security.RoleProvider RoleProvider
        {
            get
            {
                return (System.Web.Security.RoleProvider) this.roleProvider;
            }
            set
            {
                this.ThrowIfImmutable();
                this.roleProvider = value;
            }
        }

        public System.ServiceModel.ServiceAuthorizationManager ServiceAuthorizationManager
        {
            get
            {
                return this.serviceAuthorizationManager;
            }
            set
            {
                this.ThrowIfImmutable();
                this.isAuthorizationManagerSet = true;
                this.serviceAuthorizationManager = value;
            }
        }
    }
}

