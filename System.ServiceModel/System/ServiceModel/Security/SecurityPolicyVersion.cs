namespace System.ServiceModel.Security
{
    using System;

    public abstract class SecurityPolicyVersion
    {
        private readonly string prefix;
        private readonly string spNamespace;

        internal SecurityPolicyVersion(string ns, string prefix)
        {
            this.spNamespace = ns;
            this.prefix = prefix;
        }

        public string Namespace
        {
            get
            {
                return this.spNamespace;
            }
        }

        public string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        public static SecurityPolicyVersion WSSecurityPolicy11
        {
            get
            {
                return WSSecurityPolicyVersion11.Instance;
            }
        }

        public static SecurityPolicyVersion WSSecurityPolicy12
        {
            get
            {
                return WSSecurityPolicyVersion12.Instance;
            }
        }

        private class WSSecurityPolicyVersion11 : SecurityPolicyVersion
        {
            private static readonly SecurityPolicyVersion.WSSecurityPolicyVersion11 instance = new SecurityPolicyVersion.WSSecurityPolicyVersion11();

            protected WSSecurityPolicyVersion11() : base("http://schemas.xmlsoap.org/ws/2005/07/securitypolicy", "sp")
            {
            }

            public static SecurityPolicyVersion Instance
            {
                get
                {
                    return instance;
                }
            }
        }

        private class WSSecurityPolicyVersion12 : SecurityPolicyVersion
        {
            private static readonly SecurityPolicyVersion.WSSecurityPolicyVersion12 instance = new SecurityPolicyVersion.WSSecurityPolicyVersion12();

            protected WSSecurityPolicyVersion12() : base("http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200702", "sp")
            {
            }

            public static SecurityPolicyVersion Instance
            {
                get
                {
                    return instance;
                }
            }
        }
    }
}

