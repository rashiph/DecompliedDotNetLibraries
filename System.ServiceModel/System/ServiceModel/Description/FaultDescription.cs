namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;
    using System.Diagnostics;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    [DebuggerDisplay("Name={name}, Action={action}, DetailType={detailType}")]
    public class FaultDescription
    {
        private string action;
        private Type detailType;
        private CodeTypeReference detailTypeReference;
        private XmlName elementName;
        private bool hasProtectionLevel;
        private XmlName name;
        private string ns;
        private System.Net.Security.ProtectionLevel protectionLevel;

        public FaultDescription(string action)
        {
            if (action == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("action"));
            }
            this.action = action;
        }

        internal void ResetProtectionLevel()
        {
            this.protectionLevel = System.Net.Security.ProtectionLevel.None;
            this.hasProtectionLevel = false;
        }

        internal void SetNameAndElement(XmlName name)
        {
            this.elementName = this.name = name;
        }

        internal void SetNameOnly(XmlName name)
        {
            this.name = name;
        }

        public bool ShouldSerializeProtectionLevel()
        {
            return this.HasProtectionLevel;
        }

        public string Action
        {
            get
            {
                return this.action;
            }
            internal set
            {
                this.action = value;
            }
        }

        public Type DetailType
        {
            get
            {
                return this.detailType;
            }
            set
            {
                this.detailType = value;
            }
        }

        internal CodeTypeReference DetailTypeReference
        {
            get
            {
                return this.detailTypeReference;
            }
            set
            {
                this.detailTypeReference = value;
            }
        }

        internal XmlName ElementName
        {
            get
            {
                return this.elementName;
            }
            set
            {
                this.elementName = value;
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
                return this.name.EncodedName;
            }
            set
            {
                this.SetNameAndElement(new XmlName(value, true));
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

