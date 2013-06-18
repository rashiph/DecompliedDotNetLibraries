namespace System.ServiceModel
{
    using System;

    public sealed class NonDualMessageSecurityOverHttp : MessageSecurityOverHttp
    {
        internal const bool DefaultEstablishSecurityContext = true;
        private bool establishSecurityContext = true;

        protected override bool IsSecureConversationEnabled()
        {
            return this.establishSecurityContext;
        }

        public bool EstablishSecurityContext
        {
            get
            {
                return this.establishSecurityContext;
            }
            set
            {
                this.establishSecurityContext = value;
            }
        }
    }
}

