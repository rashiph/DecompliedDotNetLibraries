namespace System.Net.Mail
{
    using System;
    using System.Collections;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;

    internal class SmtpNtlmAuthenticationModule : ISmtpAuthenticationModule
    {
        private Hashtable sessions = new Hashtable();

        internal SmtpNtlmAuthenticationModule()
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
                    NTAuthentication authentication = this.sessions[sessionCookie] as NTAuthentication;
                    if (authentication == null)
                    {
                        if (credential == null)
                        {
                            return null;
                        }
                        this.sessions[sessionCookie] = authentication = new NTAuthentication(false, "Ntlm", credential, spn, ContextFlags.Connection, channelBindingToken);
                    }
                    string outgoingBlob = authentication.GetOutgoingBlob(challenge);
                    if (!authentication.IsCompleted)
                    {
                        return new Authorization(outgoingBlob, false);
                    }
                    this.sessions.Remove(sessionCookie);
                    authorization = new Authorization(outgoingBlob, true);
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
                return "ntlm";
            }
        }
    }
}

