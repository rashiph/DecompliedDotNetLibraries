namespace System.ServiceModel
{
    using System;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;

    internal static class AspNetPartialTrustHelpers
    {
        [SecurityCritical]
        private static SecurityContext aspNetSecurityContext;
        [SecurityCritical]
        private static bool isInitialized;

        [SecurityCritical, AspNetHostingPermission(SecurityAction.Assert, Level=AspNetHostingPermissionLevel.Unrestricted)]
        private static NamedPermissionSet GetHttpRuntimeNamedPermissionSet()
        {
            return HttpRuntime.GetNamedPermissionSet();
        }

        private static bool IsFullTrust(PermissionSet perms)
        {
            if (perms != null)
            {
                return perms.IsUnrestricted();
            }
            return true;
        }

        [SecuritySafeCritical]
        internal static void PartialTrustInvoke(ContextCallback callback, object state)
        {
            if (NeedPartialTrustInvoke)
            {
                SecurityContext.Run(aspNetSecurityContext.CreateCopy(), callback, state);
            }
            else
            {
                callback(state);
            }
        }

        internal static bool NeedPartialTrustInvoke
        {
            [SecuritySafeCritical]
            get
            {
                if (!isInitialized)
                {
                    NamedPermissionSet httpRuntimeNamedPermissionSet = GetHttpRuntimeNamedPermissionSet();
                    if (!IsFullTrust(httpRuntimeNamedPermissionSet))
                    {
                        try
                        {
                            httpRuntimeNamedPermissionSet.PermitOnly();
                            aspNetSecurityContext = PartialTrustHelpers.CaptureSecurityContextNoIdentityFlow();
                        }
                        finally
                        {
                            CodeAccessPermission.RevertPermitOnly();
                        }
                    }
                    isInitialized = true;
                }
                return (aspNetSecurityContext != null);
            }
        }
    }
}

