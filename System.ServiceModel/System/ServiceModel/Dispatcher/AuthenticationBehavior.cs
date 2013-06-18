namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Policy;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;

    internal sealed class AuthenticationBehavior
    {
        private AuditLogLocation auditLogLocation;
        private AuditLevel messageAuthenticationAuditLevel;
        private ServiceAuthenticationManager serviceAuthenticationManager;
        private bool suppressAuditFailure;

        private AuthenticationBehavior(ServiceAuthenticationManager authenticationManager)
        {
            this.serviceAuthenticationManager = authenticationManager;
        }

        public void Authenticate(ref MessageRpc rpc)
        {
            SecurityMessageProperty orCreate = SecurityMessageProperty.GetOrCreate(rpc.Request);
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = orCreate.ServiceSecurityContext.AuthorizationPolicies;
            try
            {
                authorizationPolicies = this.serviceAuthenticationManager.Authenticate(orCreate.ServiceSecurityContext.AuthorizationPolicies, rpc.Channel.ListenUri, ref rpc.Request);
                if (authorizationPolicies == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AuthenticationManagerShouldNotReturnNull")));
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.AuthenticationFailed(rpc.Request, rpc.Channel.ListenUri);
                }
                if (AuditLevel.Failure == (this.messageAuthenticationAuditLevel & AuditLevel.Failure))
                {
                    try
                    {
                        string identityNamesFromContext;
                        AuthorizationContext authorizationContext = orCreate.ServiceSecurityContext.AuthorizationContext;
                        if (authorizationContext != null)
                        {
                            identityNamesFromContext = System.ServiceModel.Security.SecurityUtils.GetIdentityNamesFromContext(authorizationContext);
                        }
                        else
                        {
                            identityNamesFromContext = System.ServiceModel.Security.SecurityUtils.AnonymousIdentity.Name;
                        }
                        SecurityAuditHelper.WriteMessageAuthenticationFailureEvent(this.auditLogLocation, this.suppressAuditFailure, rpc.Request, rpc.Channel.ListenUri, rpc.Request.Headers.Action, identityNamesFromContext, exception);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateFailedAuthenticationFaultException());
            }
            rpc.Request.Properties.Security.ServiceSecurityContext.AuthorizationPolicies = authorizationPolicies;
            if (AuditLevel.Success == (this.messageAuthenticationAuditLevel & AuditLevel.Success))
            {
                string name;
                AuthorizationContext authContext = orCreate.ServiceSecurityContext.AuthorizationContext;
                if (authContext != null)
                {
                    name = System.ServiceModel.Security.SecurityUtils.GetIdentityNamesFromContext(authContext);
                }
                else
                {
                    name = System.ServiceModel.Security.SecurityUtils.AnonymousIdentity.Name;
                }
                SecurityAuditHelper.WriteMessageAuthenticationSuccessEvent(this.auditLogLocation, this.suppressAuditFailure, rpc.Request, rpc.Channel.ListenUri, rpc.Request.Headers.Action, name);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static AuthenticationBehavior CreateAuthenticationBehavior(DispatchRuntime dispatch)
        {
            return new AuthenticationBehavior(dispatch.ServiceAuthenticationManager) { auditLogLocation = dispatch.SecurityAuditLogLocation, suppressAuditFailure = dispatch.SuppressAuditFailure, messageAuthenticationAuditLevel = dispatch.MessageAuthenticationAuditLevel };
        }

        internal static Exception CreateFailedAuthenticationFaultException()
        {
            SecurityVersion version = SecurityVersion.Default;
            FaultCode code = FaultCode.CreateSenderFaultCode(version.InvalidSecurityFaultCode.Value, version.HeaderNamespace.Value);
            return new FaultException(new FaultReason(System.ServiceModel.SR.GetString("AuthenticationOfClientFailed"), CultureInfo.CurrentCulture), code);
        }

        public static AuthenticationBehavior TryCreate(DispatchRuntime dispatch)
        {
            if (dispatch == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatch");
            }
            if (!dispatch.RequiresAuthentication)
            {
                return null;
            }
            return CreateAuthenticationBehavior(dispatch);
        }
    }
}

