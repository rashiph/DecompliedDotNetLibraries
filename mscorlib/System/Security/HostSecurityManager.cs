namespace System.Security
{
    using System;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Reflection;
    using System.Runtime.Hosting;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Policy;

    [Serializable, ComVisible(true), SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public class HostSecurityManager
    {
        [SecurityCritical, SecurityPermission(SecurityAction.Assert, Unrestricted=true)]
        public virtual ApplicationTrust DetermineApplicationTrust(Evidence applicationEvidence, Evidence activatorEvidence, TrustManagerContext context)
        {
            if (applicationEvidence == null)
            {
                throw new ArgumentNullException("applicationEvidence");
            }
            ActivationArguments hostEvidence = applicationEvidence.GetHostEvidence<ActivationArguments>();
            if (hostEvidence == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Policy_MissingActivationContextInAppEvidence"));
            }
            ActivationContext activationContext = hostEvidence.ActivationContext;
            if (activationContext == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Policy_MissingActivationContextInAppEvidence"));
            }
            ApplicationTrust applicationTrust = applicationEvidence.GetHostEvidence<ApplicationTrust>();
            if ((applicationTrust != null) && !CmsUtils.CompareIdentities(applicationTrust.ApplicationIdentity, hostEvidence.ApplicationIdentity, ApplicationVersionMatch.MatchExactVersion))
            {
                applicationTrust = null;
            }
            if (applicationTrust == null)
            {
                if ((AppDomain.CurrentDomain.ApplicationTrust != null) && CmsUtils.CompareIdentities(AppDomain.CurrentDomain.ApplicationTrust.ApplicationIdentity, hostEvidence.ApplicationIdentity, ApplicationVersionMatch.MatchExactVersion))
                {
                    applicationTrust = AppDomain.CurrentDomain.ApplicationTrust;
                }
                else
                {
                    applicationTrust = ApplicationSecurityManager.DetermineApplicationTrustInternal(activationContext, context);
                }
            }
            ApplicationSecurityInfo info = new ApplicationSecurityInfo(activationContext);
            if (((applicationTrust != null) && applicationTrust.IsApplicationTrustedToRun) && !info.DefaultRequestSet.IsSubsetOf(applicationTrust.DefaultGrantSet.PermissionSet))
            {
                throw new InvalidOperationException(Environment.GetResourceString("Policy_AppTrustMustGrantAppRequest"));
            }
            return applicationTrust;
        }

        public virtual EvidenceBase GenerateAppDomainEvidence(Type evidenceType)
        {
            return null;
        }

        public virtual EvidenceBase GenerateAssemblyEvidence(Type evidenceType, Assembly assembly)
        {
            return null;
        }

        public virtual Type[] GetHostSuppliedAppDomainEvidenceTypes()
        {
            return null;
        }

        public virtual Type[] GetHostSuppliedAssemblyEvidenceTypes(Assembly assembly)
        {
            return null;
        }

        public virtual Evidence ProvideAppDomainEvidence(Evidence inputEvidence)
        {
            return inputEvidence;
        }

        public virtual Evidence ProvideAssemblyEvidence(Assembly loadedAssembly, Evidence inputEvidence)
        {
            return inputEvidence;
        }

        public virtual PermissionSet ResolvePolicy(Evidence evidence)
        {
            if (evidence == null)
            {
                throw new ArgumentNullException("evidence");
            }
            if (evidence.GetHostEvidence<GacInstalled>() != null)
            {
                return new PermissionSet(PermissionState.Unrestricted);
            }
            if (AppDomain.CurrentDomain.IsHomogenous)
            {
                return AppDomain.CurrentDomain.GetHomogenousGrantSet(evidence);
            }
            if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                return new PermissionSet(PermissionState.Unrestricted);
            }
            return SecurityManager.PolicyManager.CodeGroupResolve(evidence, false);
        }

        [Obsolete("AppDomain policy levels are obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public virtual PolicyLevel DomainPolicy
        {
            get
            {
                if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
                }
                return null;
            }
        }

        public virtual HostSecurityManagerOptions Flags
        {
            get
            {
                return HostSecurityManagerOptions.AllFlags;
            }
        }
    }
}

