namespace System.ServiceModel
{
    using System;
    using System.Net.Security;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
    public sealed class FaultContractAttribute : Attribute
    {
        private string action;
        private bool hasProtectionLevel;
        private string name;
        private string ns;
        private System.Net.Security.ProtectionLevel protectionLevel;
        internal const string ProtectionLevelPropertyName = "ProtectionLevel";
        private Type type;

        public FaultContractAttribute(Type detailType)
        {
            if (detailType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("detailType"));
            }
            this.type = detailType;
        }

        public string Action
        {
            get
            {
                return this.action;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.action = value;
            }
        }

        public Type DetailType
        {
            get
            {
                return this.type;
            }
        }

        public bool HasProtectionLevel
        {
            get
            {
                return this.hasProtectionLevel;
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
                if (!string.IsNullOrEmpty(value))
                {
                    NamingHelper.CheckUriProperty(value, "Namespace");
                }
                this.ns = value;
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

