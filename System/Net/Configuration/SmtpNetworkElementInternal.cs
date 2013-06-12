namespace System.Net.Configuration
{
    using System;
    using System.Net;

    internal sealed class SmtpNetworkElementInternal
    {
        private string clientDomain;
        private NetworkCredential credential;
        private bool enableSsl;
        private string host;
        private int port;
        private string targetname;

        internal SmtpNetworkElementInternal(SmtpNetworkElement element)
        {
            this.host = element.Host;
            this.port = element.Port;
            this.targetname = element.TargetName;
            this.clientDomain = element.ClientDomain;
            this.enableSsl = element.EnableSsl;
            if (element.DefaultCredentials)
            {
                this.credential = (NetworkCredential) CredentialCache.DefaultCredentials;
            }
            else if ((element.UserName != null) && (element.UserName.Length > 0))
            {
                this.credential = new NetworkCredential(element.UserName, element.Password);
            }
        }

        internal string ClientDomain
        {
            get
            {
                return this.clientDomain;
            }
        }

        internal NetworkCredential Credential
        {
            get
            {
                return this.credential;
            }
        }

        internal bool EnableSsl
        {
            get
            {
                return this.enableSsl;
            }
        }

        internal string Host
        {
            get
            {
                return this.host;
            }
        }

        internal int Port
        {
            get
            {
                return this.port;
            }
        }

        internal string TargetName
        {
            get
            {
                return this.targetname;
            }
        }
    }
}

