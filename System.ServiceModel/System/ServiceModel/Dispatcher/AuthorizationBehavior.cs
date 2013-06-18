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

    internal sealed class AuthorizationBehavior
    {
        private AuditLogLocation auditLogLocation;
        private static ServiceAuthorizationManager DefaultServiceAuthorizationManager = new ServiceAuthorizationManager();
        private ReadOnlyCollection<IAuthorizationPolicy> externalAuthorizationPolicies;
        private AuditLevel serviceAuthorizationAuditLevel;
        private ServiceAuthorizationManager serviceAuthorizationManager;
        private bool suppressAuditFailure;

        private AuthorizationBehavior()
        {
        }

        public void Authorize(ref MessageRpc rpc)
        {
            SecurityMessageProperty orCreate = SecurityMessageProperty.GetOrCreate(rpc.Request);
            orCreate.ExternalAuthorizationPolicies = this.externalAuthorizationPolicies;
            ServiceAuthorizationManager manager = this.serviceAuthorizationManager ?? DefaultServiceAuthorizationManager;
            try
            {
                if (!manager.CheckAccess(rpc.OperationContext, ref rpc.Request))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateAccessDeniedFaultException());
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
                    PerformanceCounters.AuthorizationFailed(rpc.Operation.Name);
                }
                if (AuditLevel.Failure == (this.serviceAuthorizationAuditLevel & AuditLevel.Failure))
                {
                    try
                    {
                        string identityNamesFromContext;
                        string authContextId = null;
                        AuthorizationContext authorizationContext = orCreate.ServiceSecurityContext.AuthorizationContext;
                        if (authorizationContext != null)
                        {
                            identityNamesFromContext = System.ServiceModel.Security.SecurityUtils.GetIdentityNamesFromContext(authorizationContext);
                            authContextId = authorizationContext.Id;
                        }
                        else
                        {
                            identityNamesFromContext = System.ServiceModel.Security.SecurityUtils.AnonymousIdentity.Name;
                            authContextId = "<null>";
                        }
                        SecurityAuditHelper.WriteServiceAuthorizationFailureEvent(this.auditLogLocation, this.suppressAuditFailure, rpc.Request, rpc.Request.Headers.To, rpc.Request.Headers.Action, identityNamesFromContext, authContextId, (manager == DefaultServiceAuthorizationManager) ? "<default>" : manager.GetType().Name, exception);
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
                throw;
            }
            if (AuditLevel.Success == (this.serviceAuthorizationAuditLevel & AuditLevel.Success))
            {
                string name;
                string id;
                AuthorizationContext authContext = orCreate.ServiceSecurityContext.AuthorizationContext;
                if (authContext != null)
                {
                    name = System.ServiceModel.Security.SecurityUtils.GetIdentityNamesFromContext(authContext);
                    id = authContext.Id;
                }
                else
                {
                    name = System.ServiceModel.Security.SecurityUtils.AnonymousIdentity.Name;
                    id = "<null>";
                }
                SecurityAuditHelper.WriteServiceAuthorizationSuccessEvent(this.auditLogLocation, this.suppressAuditFailure, rpc.Request, rpc.Request.Headers.To, rpc.Request.Headers.Action, name, id, (manager == DefaultServiceAuthorizationManager) ? "<default>" : manager.GetType().Name);
            }
        }

        internal static Exception CreateAccessDeniedFaultException()
        {
            SecurityVersion version = SecurityVersion.Default;
            FaultCode code = FaultCode.CreateSenderFaultCode(version.FailedAuthenticationFaultCode.Value, version.HeaderNamespace.Value);
            return new FaultException(new FaultReason(System.ServiceModel.SR.GetString("AccessDenied"), CultureInfo.CurrentCulture), code);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static AuthorizationBehavior CreateAuthorizationBehavior(DispatchRuntime dispatch)
        {
            return new AuthorizationBehavior { externalAuthorizationPolicies = dispatch.ExternalAuthorizationPolicies, serviceAuthorizationManager = dispatch.ServiceAuthorizationManager, auditLogLocation = dispatch.SecurityAuditLogLocation, suppressAuditFailure = dispatch.SuppressAuditFailure, serviceAuthorizationAuditLevel = dispatch.ServiceAuthorizationAuditLevel };
        }

        public static AuthorizationBehavior TryCreate(DispatchRuntime dispatch)
        {
            if (dispatch == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("dispatch"));
            }
            if (!dispatch.RequiresAuthorization)
            {
                return null;
            }
            return CreateAuthorizationBehavior(dispatch);
        }
    }
}

