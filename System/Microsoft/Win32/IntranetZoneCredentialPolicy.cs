namespace Microsoft.Win32
{
    using System;
    using System.Net;

    public class IntranetZoneCredentialPolicy : ICredentialPolicy
    {
        private IInternetSecurityManager _ManagerRef;
        private const int URLZONE_INTRANET = 1;

        public IntranetZoneCredentialPolicy()
        {
            ExceptionHelper.ControlPolicyPermission.Demand();
            this._ManagerRef = (IInternetSecurityManager) new InternetSecurityManager();
        }

        public virtual bool ShouldSendCredential(Uri challengeUri, WebRequest request, NetworkCredential credential, IAuthenticationModule authModule)
        {
            int num;
            this._ManagerRef.MapUrlToZone(challengeUri.AbsoluteUri, out num, 0);
            return (num == 1);
        }
    }
}

