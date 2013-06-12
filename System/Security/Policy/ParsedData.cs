namespace System.Security.Policy
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    internal class ParsedData
    {
        private string appName;
        private string appPublisher;
        private string authenticodedPublisher;
        private X509Certificate2 certificate;
        private bool disallowTrustOverride;
        private bool requestsShellIntegration;
        private string supportUrl;

        public string AppName
        {
            get
            {
                return this.appName;
            }
            set
            {
                this.appName = value;
            }
        }

        public string AppPublisher
        {
            get
            {
                return this.appPublisher;
            }
            set
            {
                this.appPublisher = value;
            }
        }

        public string AuthenticodedPublisher
        {
            get
            {
                return this.authenticodedPublisher;
            }
            set
            {
                this.authenticodedPublisher = value;
            }
        }

        public X509Certificate2 Certificate
        {
            get
            {
                return this.certificate;
            }
            set
            {
                this.certificate = value;
            }
        }

        public bool RequestsShellIntegration
        {
            get
            {
                return this.requestsShellIntegration;
            }
            set
            {
                this.requestsShellIntegration = value;
            }
        }

        public string SupportUrl
        {
            get
            {
                return this.supportUrl;
            }
            set
            {
                this.supportUrl = value;
            }
        }

        public bool UseManifestForTrust
        {
            get
            {
                return this.disallowTrustOverride;
            }
            set
            {
                this.disallowTrustOverride = value;
            }
        }
    }
}

