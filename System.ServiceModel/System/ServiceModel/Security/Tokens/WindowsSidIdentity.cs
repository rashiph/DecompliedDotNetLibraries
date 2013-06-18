namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Security.Principal;
    using System.ServiceModel;

    internal class WindowsSidIdentity : IIdentity
    {
        private string authenticationType;
        private string name;
        private System.Security.Principal.SecurityIdentifier sid;

        public WindowsSidIdentity(System.Security.Principal.SecurityIdentifier sid)
        {
            if (sid == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sid");
            }
            this.sid = sid;
            this.authenticationType = string.Empty;
        }

        public WindowsSidIdentity(System.Security.Principal.SecurityIdentifier sid, string name, string authenticationType)
        {
            if (sid == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sid");
            }
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            if (authenticationType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authenticationType");
            }
            this.sid = sid;
            this.name = name;
            this.authenticationType = authenticationType;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            WindowsSidIdentity identity = obj as WindowsSidIdentity;
            if (identity == null)
            {
                return false;
            }
            return (this.sid == identity.SecurityIdentifier);
        }

        public override int GetHashCode()
        {
            return this.sid.GetHashCode();
        }

        public string AuthenticationType
        {
            get
            {
                return this.authenticationType;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return true;
            }
        }

        public string Name
        {
            get
            {
                if (this.name == null)
                {
                    this.name = ((NTAccount) this.sid.Translate(typeof(NTAccount))).Value;
                }
                return this.name;
            }
        }

        public System.Security.Principal.SecurityIdentifier SecurityIdentifier
        {
            get
            {
                return this.sid;
            }
        }
    }
}

