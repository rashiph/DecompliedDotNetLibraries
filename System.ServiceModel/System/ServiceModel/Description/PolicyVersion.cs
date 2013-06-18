namespace System.ServiceModel.Description
{
    using System;

    public sealed class PolicyVersion
    {
        private string policyNamespace;
        private static PolicyVersion policyVersion12 = new PolicyVersion("http://schemas.xmlsoap.org/ws/2004/09/policy");
        private static PolicyVersion policyVersion15 = new PolicyVersion("http://www.w3.org/ns/ws-policy");

        private PolicyVersion(string policyNamespace)
        {
            this.policyNamespace = policyNamespace;
        }

        public override string ToString()
        {
            return this.policyNamespace;
        }

        public static PolicyVersion Default
        {
            get
            {
                return policyVersion12;
            }
        }

        public string Namespace
        {
            get
            {
                return this.policyNamespace;
            }
        }

        public static PolicyVersion Policy12
        {
            get
            {
                return policyVersion12;
            }
        }

        public static PolicyVersion Policy15
        {
            get
            {
                return policyVersion15;
            }
        }
    }
}

