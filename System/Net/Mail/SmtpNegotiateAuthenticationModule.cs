namespace System.Net.Mail
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;

    internal class SmtpNegotiateAuthenticationModule : ISmtpAuthenticationModule
    {
        private Hashtable sessions = new Hashtable();

        internal SmtpNegotiateAuthenticationModule()
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
                    NTAuthentication clientContext = this.sessions[sessionCookie] as NTAuthentication;
                    if (clientContext == null)
                    {
                        if (credential == null)
                        {
                            return null;
                        }
                        this.sessions[sessionCookie] = clientContext = new NTAuthentication(false, "Negotiate", credential, spn, ContextFlags.AcceptStream | ContextFlags.Connection, channelBindingToken);
                    }
                    string token = null;
                    if (!clientContext.IsCompleted)
                    {
                        SecurityStatus status;
                        byte[] incomingBlob = null;
                        if (challenge != null)
                        {
                            incomingBlob = Convert.FromBase64String(challenge);
                        }
                        byte[] inArray = clientContext.GetOutgoingBlob(incomingBlob, false, out status);
                        if (clientContext.IsCompleted && (inArray == null))
                        {
                            token = "\r\n";
                        }
                        if (inArray != null)
                        {
                            token = Convert.ToBase64String(inArray);
                        }
                    }
                    else
                    {
                        token = this.GetSecurityLayerOutgoingBlob(challenge, clientContext);
                    }
                    authorization = new Authorization(token, clientContext.IsCompleted);
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
            NTAuthentication authentication = null;
            lock (this.sessions)
            {
                authentication = this.sessions[sessionCookie] as NTAuthentication;
                if (authentication != null)
                {
                    this.sessions.Remove(sessionCookie);
                }
            }
            if (authentication != null)
            {
                authentication.CloseContext();
            }
        }

        private string GetSecurityLayerOutgoingBlob(string challenge, NTAuthentication clientContext)
        {
            int num;
            if (challenge == null)
            {
                return null;
            }
            byte[] buffer = Convert.FromBase64String(challenge);
            try
            {
                num = clientContext.VerifySignature(buffer, 0, buffer.Length);
            }
            catch (Win32Exception)
            {
                return null;
            }
            if (((num < 4) || (buffer[0] != 1)) || (((buffer[1] != 0) || (buffer[2] != 0)) || (buffer[3] != 0)))
            {
                return null;
            }
            byte[] output = null;
            try
            {
                num = clientContext.MakeSignature(buffer, 0, 4, ref output);
            }
            catch (Win32Exception)
            {
                return null;
            }
            return Convert.ToBase64String(output, 0, num);
        }

        public string AuthenticationType
        {
            get
            {
                return "gssapi";
            }
        }
    }
}

