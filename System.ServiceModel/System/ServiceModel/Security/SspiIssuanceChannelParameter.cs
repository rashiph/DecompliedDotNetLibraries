namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;

    internal class SspiIssuanceChannelParameter
    {
        private SafeFreeCredentials credentialsHandle;
        private bool getTokenOnOpen;

        public SspiIssuanceChannelParameter(bool getTokenOnOpen, SafeFreeCredentials credentialsHandle)
        {
            this.getTokenOnOpen = getTokenOnOpen;
            this.credentialsHandle = credentialsHandle;
        }

        public SafeFreeCredentials CredentialsHandle
        {
            get
            {
                return this.credentialsHandle;
            }
        }

        public bool GetTokenOnOpen
        {
            get
            {
                return this.getTokenOnOpen;
            }
        }
    }
}

