namespace System.ServiceModel
{
    using System;
    using System.Net.Security;
    using System.ServiceModel.Security;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OperationContractAttribute : Attribute
    {
        private string action;
        internal const string ActionPropertyName = "Action";
        private bool asyncPattern;
        private bool hasProtectionLevel;
        private bool isInitiating = true;
        private bool isOneWay;
        private bool isTerminating;
        private string name;
        private System.Net.Security.ProtectionLevel protectionLevel;
        internal const string ProtectionLevelPropertyName = "ProtectionLevel";
        private string replyAction;
        internal const string ReplyActionPropertyName = "ReplyAction";

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

        public bool AsyncPattern
        {
            get
            {
                return this.asyncPattern;
            }
            set
            {
                this.asyncPattern = value;
            }
        }

        public bool HasProtectionLevel
        {
            get
            {
                return this.hasProtectionLevel;
            }
        }

        public bool IsInitiating
        {
            get
            {
                return this.isInitiating;
            }
            set
            {
                this.isInitiating = value;
            }
        }

        public bool IsOneWay
        {
            get
            {
                return this.isOneWay;
            }
            set
            {
                this.isOneWay = value;
            }
        }

        public bool IsTerminating
        {
            get
            {
                return this.isTerminating;
            }
            set
            {
                this.isTerminating = value;
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
                if (value == "")
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("SFxNameCannotBeEmpty")));
                }
                this.name = value;
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

        public string ReplyAction
        {
            get
            {
                return this.replyAction;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.replyAction = value;
            }
        }
    }
}

