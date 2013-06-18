namespace System.ServiceModel
{
    using System;
    using System.Net.Security;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    public abstract class MessageContractMemberAttribute : Attribute
    {
        private bool hasProtectionLevel;
        private bool isNameSetExplicit;
        private bool isNamespaceSetExplicit;
        private string name;
        internal const string NamePropertyName = "Name";
        internal const string NamespacePropertyName = "Namespace";
        private string ns;
        private System.Net.Security.ProtectionLevel protectionLevel;
        internal const string ProtectionLevelPropertyName = "ProtectionLevel";

        protected MessageContractMemberAttribute()
        {
        }

        public bool HasProtectionLevel
        {
            get
            {
                return this.hasProtectionLevel;
            }
        }

        internal bool IsNameSetExplicit
        {
            get
            {
                return this.isNameSetExplicit;
            }
        }

        internal bool IsNamespaceSetExplicit
        {
            get
            {
                return this.isNamespaceSetExplicit;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value == string.Empty)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("SFxNameCannotBeEmpty")));
                }
                this.name = value;
                this.isNameSetExplicit = true;
            }
        }

        public string Namespace
        {
            get
            {
                return this.ns;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value.Length > 0)
                {
                    NamingHelper.CheckUriProperty(value, "Namespace");
                }
                this.ns = value;
                this.isNamespaceSetExplicit = true;
            }
        }

        public System.Net.Security.ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.protectionLevel = value;
                this.hasProtectionLevel = true;
            }
        }
    }
}

