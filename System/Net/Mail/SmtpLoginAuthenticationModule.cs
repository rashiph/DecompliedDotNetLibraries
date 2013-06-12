namespace System.Net.Mail
{
    using System;
    using System.Collections;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.Text;

    internal class SmtpLoginAuthenticationModule : ISmtpAuthenticationModule
    {
        private Hashtable sessions = new Hashtable();

        internal SmtpLoginAuthenticationModule()
        {
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        public Authorization Authenticate(string challenge, NetworkCredential credential, object sessionCookie, string spn, ChannelBinding channelBindingToken)
        {
            Authorization authorization;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "Authenticate", (string) null);
            }
            try
            {
                lock (this.sessions)
                {
                    NetworkCredential credential2 = this.sessions[sessionCookie] as NetworkCredential;
                    if (credential2 == null)
                    {
                        if ((credential == null) || (credential is SystemNetworkCredential))
                        {
                            return null;
                        }
                        this.sessions[sessionCookie] = credential;
                        string userName = credential.UserName;
                        string domain = credential.Domain;
                        if ((domain != null) && (domain.Length > 0))
                        {
                            userName = domain + @"\" + userName;
                        }
                        return new Authorization(Convert.ToBase64String(Encoding.UTF8.GetBytes(userName)), false);
                    }
                    this.sessions.Remove(sessionCookie);
                    authorization = new Authorization(Convert.ToBase64String(Encoding.UTF8.GetBytes(credential2.Password)), true);
                }
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "Authenticate", (string) null);
                }
            }
            return authorization;
        }

        public void CloseContext(object sessionCookie)
        {
        }

        public string AuthenticationType
        {
            get
            {
                return "login";
            }
        }
    }
}

