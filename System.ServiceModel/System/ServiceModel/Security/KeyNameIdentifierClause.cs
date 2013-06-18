namespace System.ServiceModel.Security
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;

    public class KeyNameIdentifierClause : SecurityKeyIdentifierClause
    {
        private string keyName;

        public KeyNameIdentifierClause(string keyName) : base(null)
        {
            if (keyName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyName");
            }
            this.keyName = keyName;
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            KeyNameIdentifierClause objB = keyIdentifierClause as KeyNameIdentifierClause;
            return (object.ReferenceEquals(this, objB) || ((objB != null) && objB.Matches(this.keyName)));
        }

        public bool Matches(string keyName)
        {
            return (this.keyName == keyName);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "KeyNameIdentifierClause(KeyName = '{0}')", new object[] { this.KeyName });
        }

        public string KeyName
        {
            get
            {
                return this.keyName;
            }
        }
    }
}

