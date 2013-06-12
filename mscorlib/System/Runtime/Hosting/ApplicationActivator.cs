namespace System.Runtime.Hosting
{
    using System;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Security.Policy;

    [ComVisible(true)]
    public class ApplicationActivator
    {
        public virtual ObjectHandle CreateInstance(ActivationContext activationContext)
        {
            return this.CreateInstance(activationContext, null);
        }

        [SecuritySafeCritical]
        public virtual ObjectHandle CreateInstance(ActivationContext activationContext, string[] activationCustomData)
        {
            if (activationContext == null)
            {
                throw new ArgumentNullException("activationContext");
            }
            if (CmsUtils.CompareIdentities(AppDomain.CurrentDomain.ActivationContext, activationContext))
            {
                ManifestRunner runner = new ManifestRunner(AppDomain.CurrentDomain, activationContext);
                return new ObjectHandle(runner.ExecuteAsAssembly());
            }
            AppDomainSetup adSetup = new AppDomainSetup(new ActivationArguments(activationContext, activationCustomData));
            AppDomainSetup setupInformation = AppDomain.CurrentDomain.SetupInformation;
            adSetup.AppDomainManagerType = setupInformation.AppDomainManagerType;
            adSetup.AppDomainManagerAssembly = setupInformation.AppDomainManagerAssembly;
            return CreateInstanceHelper(adSetup);
        }

        [SecuritySafeCritical]
        protected static ObjectHandle CreateInstanceHelper(AppDomainSetup adSetup)
        {
            if (adSetup.ActivationArguments == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MissingActivationArguments"));
            }
            adSetup.ActivationArguments.ActivateInstance = true;
            Evidence activatorEvidence = AppDomain.CurrentDomain.Evidence;
            Evidence applicationEvidence = CmsUtils.MergeApplicationEvidence(null, adSetup.ActivationArguments.ApplicationIdentity, adSetup.ActivationArguments.ActivationContext, adSetup.ActivationArguments.ActivationData);
            ApplicationTrust trust = AppDomain.CurrentDomain.HostSecurityManager.DetermineApplicationTrust(applicationEvidence, activatorEvidence, new TrustManagerContext());
            if ((trust == null) || !trust.IsApplicationTrustedToRun)
            {
                throw new PolicyException(Environment.GetResourceString("Policy_NoExecutionPermission"), -2146233320, null);
            }
            ObjRef objectRef = AppDomain.nCreateInstance(adSetup.ActivationArguments.ApplicationIdentity.FullName, adSetup, applicationEvidence, (applicationEvidence == null) ? AppDomain.CurrentDomain.InternalEvidence : null, AppDomain.CurrentDomain.GetSecurityDescriptor());
            if (objectRef == null)
            {
                return null;
            }
            return (RemotingServices.Unmarshal(objectRef) as ObjectHandle);
        }
    }
}

