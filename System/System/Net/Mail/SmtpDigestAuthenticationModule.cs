namespace System.Net.Mail
{
    using System;
    using System.Collections;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;

    internal class SmtpDigestAuthenticationModule : ISmtpAuthenticationModule
    {
        private Hashtable sessions = new Hashtable();

        internal SmtpDigestAuthenticationModule()
        {
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        public Authorization Authenticate(string challenge, NetworkCredential credential, object sessionCookie, string spn, ChannelBinding channelBindingToken)
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
                    this.sessions[sessionCookie] = authentication = new NTAuthentication(false, "WDigest", credential, spn, ContextFlags.Connection, channelBindingToken);
                }
                string outgoingBlob = authentication.GetOutgoingBlob(challenge);
                if (!authentication.IsCompleted)
                {
                    return new Authorization(outgoingBlob, false);
                }
                this.sessions.Remove(sessionCookie);
                return new Authorization(outgoingBlob, true);
            }
        }

        public void CloseContext(object sessionCookie)
        {
        }

        public string AuthenticationType
        {
            get
            {
                return "WDigest";
            }
        }
    }
}

