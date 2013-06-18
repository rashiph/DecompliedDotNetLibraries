namespace System.Deployment.Application
{
    using System;
    using System.Security.Policy;
    using System.Threading;

    internal static class ApplicationTrust
    {
        public static System.Security.Policy.ApplicationTrust PersistTrustWithoutEvaluation(ActivationContext actCtx)
        {
            ApplicationSecurityInfo info = new ApplicationSecurityInfo(actCtx);
            System.Security.Policy.ApplicationTrust trust = new System.Security.Policy.ApplicationTrust(actCtx.Identity) {
                IsApplicationTrustedToRun = true,
                DefaultGrantSet = new PolicyStatement(info.DefaultRequestSet, PolicyStatementAttribute.Nothing),
                Persist = true,
                ApplicationIdentity = actCtx.Identity
            };
            ApplicationSecurityManager.UserApplicationTrusts.Add(trust);
            return trust;
        }

        public static void RemoveCachedTrust(DefinitionAppId appId)
        {
            ApplicationSecurityManager.UserApplicationTrusts.Remove(appId.ToApplicationIdentity(), ApplicationVersionMatch.MatchExactVersion);
        }

        public static System.Security.Policy.ApplicationTrust RequestTrust(SubscriptionState subState, bool isShellVisible, bool isUpdate, ActivationContext actCtx)
        {
            TrustManagerContext tmc = new TrustManagerContext {
                IgnorePersistedDecision = false,
                NoPrompt = false,
                Persist = true
            };
            return RequestTrust(subState, isShellVisible, isUpdate, actCtx, tmc);
        }

        public static System.Security.Policy.ApplicationTrust RequestTrust(SubscriptionState subState, bool isShellVisible, bool isUpdate, ActivationContext actCtx, TrustManagerContext tmc)
        {
            Logger.AddMethodCall("ApplicationTrust.RequestTrust(isShellVisible=" + isShellVisible.ToString() + ", isUpdate=" + isUpdate.ToString() + ", subState.IsInstalled=" + subState.IsInstalled.ToString() + ") called.");
            if (!subState.IsInstalled || (subState.IsShellVisible != isShellVisible))
            {
                tmc.IgnorePersistedDecision = true;
            }
            if (isUpdate)
            {
                tmc.PreviousApplicationIdentity = subState.CurrentBind.ToApplicationIdentity();
            }
            bool flag = false;
            try
            {
                Logger.AddInternalState("Calling ApplicationSecurityManager.DetermineApplicationTrust().");
                Logger.AddInternalState("Trust Manager Context=" + Logger.Serialize(tmc));
                flag = ApplicationSecurityManager.DetermineApplicationTrust(actCtx, tmc);
            }
            catch (TypeLoadException exception)
            {
                throw new InvalidDeploymentException(Resources.GetString("Ex_InvalidTrustInfo"), exception);
            }
            if (!flag)
            {
                throw new TrustNotGrantedException(Resources.GetString("Ex_NoTrust"));
            }
            Logger.AddInternalState("Trust granted.");
            System.Security.Policy.ApplicationTrust trust = null;
            for (int i = 0; i < 5; i++)
            {
                trust = ApplicationSecurityManager.UserApplicationTrusts[actCtx.Identity.FullName];
                if (trust != null)
                {
                    break;
                }
                Thread.Sleep(10);
            }
            if (trust == null)
            {
                throw new InvalidDeploymentException(Resources.GetString("Ex_InvalidMatchTrust"));
            }
            return trust;
        }
    }
}

