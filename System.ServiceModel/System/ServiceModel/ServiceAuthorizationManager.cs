namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    public class ServiceAuthorizationManager
    {
        public virtual bool CheckAccess(OperationContext operationContext)
        {
            if (operationContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationContext");
            }
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = this.GetAuthorizationPolicies(operationContext);
            operationContext.IncomingMessageProperties.Security.ServiceSecurityContext = new ServiceSecurityContext(authorizationPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance);
            return this.CheckAccessCore(operationContext);
        }

        public virtual bool CheckAccess(OperationContext operationContext, ref Message message)
        {
            return this.CheckAccess(operationContext);
        }

        protected virtual bool CheckAccessCore(OperationContext operationContext)
        {
            return true;
        }

        protected virtual ReadOnlyCollection<IAuthorizationPolicy> GetAuthorizationPolicies(OperationContext operationContext)
        {
            SecurityMessageProperty security = operationContext.IncomingMessageProperties.Security;
            if (security == null)
            {
                return EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            ReadOnlyCollection<IAuthorizationPolicy> externalAuthorizationPolicies = security.ExternalAuthorizationPolicies;
            if (security.ServiceSecurityContext == null)
            {
                return (externalAuthorizationPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance);
            }
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = security.ServiceSecurityContext.AuthorizationPolicies;
            if ((externalAuthorizationPolicies == null) || (externalAuthorizationPolicies.Count <= 0))
            {
                return authorizationPolicies;
            }
            List<IAuthorizationPolicy> list = new List<IAuthorizationPolicy>(authorizationPolicies);
            list.AddRange(externalAuthorizationPolicies);
            return list.AsReadOnly();
        }
    }
}

