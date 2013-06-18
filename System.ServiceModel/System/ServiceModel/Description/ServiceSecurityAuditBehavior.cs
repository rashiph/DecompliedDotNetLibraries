namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public sealed class ServiceSecurityAuditBehavior : IServiceBehavior
    {
        private System.ServiceModel.AuditLogLocation auditLogLocation;
        internal const System.ServiceModel.AuditLogLocation defaultAuditLogLocation = System.ServiceModel.AuditLogLocation.Default;
        internal const AuditLevel defaultMessageAuthenticationAuditLevel = AuditLevel.None;
        internal const AuditLevel defaultServiceAuthorizationAuditLevel = AuditLevel.None;
        internal const bool defaultSuppressAuditFailure = true;
        private AuditLevel messageAuthenticationAuditLevel;
        private AuditLevel serviceAuthorizationAuditLevel;
        private bool suppressAuditFailure;

        public ServiceSecurityAuditBehavior()
        {
            this.auditLogLocation = System.ServiceModel.AuditLogLocation.Default;
            this.suppressAuditFailure = true;
            this.serviceAuthorizationAuditLevel = AuditLevel.None;
            this.messageAuthenticationAuditLevel = AuditLevel.None;
        }

        private ServiceSecurityAuditBehavior(ServiceSecurityAuditBehavior behavior)
        {
            this.auditLogLocation = behavior.auditLogLocation;
            this.suppressAuditFailure = behavior.suppressAuditFailure;
            this.serviceAuthorizationAuditLevel = behavior.serviceAuthorizationAuditLevel;
            this.messageAuthenticationAuditLevel = behavior.messageAuthenticationAuditLevel;
        }

        internal ServiceSecurityAuditBehavior Clone()
        {
            return new ServiceSecurityAuditBehavior(this);
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));
            }
            parameters.Add(this);
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
                ChannelDispatcher dispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if (dispatcher != null)
                {
                    foreach (EndpointDispatcher dispatcher2 in dispatcher.Endpoints)
                    {
                        if (!dispatcher2.IsSystemEndpoint)
                        {
                            DispatchRuntime dispatchRuntime = dispatcher2.DispatchRuntime;
                            dispatchRuntime.SecurityAuditLogLocation = this.auditLogLocation;
                            dispatchRuntime.SuppressAuditFailure = this.suppressAuditFailure;
                            dispatchRuntime.ServiceAuthorizationAuditLevel = this.serviceAuthorizationAuditLevel;
                            dispatchRuntime.MessageAuthenticationAuditLevel = this.messageAuthenticationAuditLevel;
                        }
                    }
                }
            }
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        public System.ServiceModel.AuditLogLocation AuditLogLocation
        {
            get
            {
                return this.auditLogLocation;
            }
            set
            {
                if (!AuditLogLocationHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.auditLogLocation = value;
            }
        }

        public AuditLevel MessageAuthenticationAuditLevel
        {
            get
            {
                return this.messageAuthenticationAuditLevel;
            }
            set
            {
                if (!AuditLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.messageAuthenticationAuditLevel = value;
            }
        }

        public AuditLevel ServiceAuthorizationAuditLevel
        {
            get
            {
                return this.serviceAuthorizationAuditLevel;
            }
            set
            {
                if (!AuditLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.serviceAuthorizationAuditLevel = value;
            }
        }

        public bool SuppressAuditFailure
        {
            get
            {
                return this.suppressAuditFailure;
            }
            set
            {
                this.suppressAuditFailure = value;
            }
        }
    }
}

