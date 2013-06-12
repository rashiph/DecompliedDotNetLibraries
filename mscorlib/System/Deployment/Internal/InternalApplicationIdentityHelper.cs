namespace System.Deployment.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(false)]
    public static class InternalApplicationIdentityHelper
    {
        [SecurityCritical]
        public static object GetInternalAppId(ApplicationIdentity id)
        {
            return id.Identity;
        }
    }
}

