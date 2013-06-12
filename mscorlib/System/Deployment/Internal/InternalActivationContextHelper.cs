namespace System.Deployment.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(false)]
    public static class InternalActivationContextHelper
    {
        [SecuritySafeCritical]
        public static object GetActivationContextData(ActivationContext appInfo)
        {
            return appInfo.ActivationContextData;
        }

        [SecuritySafeCritical]
        public static object GetApplicationComponentManifest(ActivationContext appInfo)
        {
            return appInfo.ApplicationComponentManifest;
        }

        [SecuritySafeCritical]
        public static byte[] GetApplicationManifestBytes(ActivationContext appInfo)
        {
            if (appInfo == null)
            {
                throw new ArgumentNullException("appInfo");
            }
            return appInfo.GetApplicationManifestBytes();
        }

        [SecuritySafeCritical]
        public static object GetDeploymentComponentManifest(ActivationContext appInfo)
        {
            return appInfo.DeploymentComponentManifest;
        }

        [SecuritySafeCritical]
        public static byte[] GetDeploymentManifestBytes(ActivationContext appInfo)
        {
            if (appInfo == null)
            {
                throw new ArgumentNullException("appInfo");
            }
            return appInfo.GetDeploymentManifestBytes();
        }

        public static bool IsFirstRun(ActivationContext appInfo)
        {
            return (appInfo.LastApplicationStateResult == ActivationContext.ApplicationStateDisposition.RunningFirstTime);
        }

        public static void PrepareForExecution(ActivationContext appInfo)
        {
            appInfo.PrepareForExecution();
        }
    }
}

